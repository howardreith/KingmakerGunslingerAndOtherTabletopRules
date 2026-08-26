# Summon Same-Turn Activation Qualification

Status: STANDALONE MATRIX AUTOMATED-RUNTIME QUALIFIED; OPTIONAL PROFILES AND FINAL VERSION PENDING

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

## Standalone runtime matrix

All rows below use guarded Steam App ID 640820 launches, the named disposable
`KMG_AUTOMATION_WORKING` save, actual spellbook/command/rule/spawn paths, and
structured installed-engine state. None writes a save.

### Acadamae Standard

- Run ID: `20260826T1631499603377Z-713c7efcecab4c98963f8ca5a72b6650`
- Evidence directory:
  `20260826T1631499361131Z-summon-same-turn-acadamae`
- Result: PASS 11/11
- Real path/action: one prepared slot spent; Standard command; one
  `RuleCastSpell`, spawn graph, and `RuleSummonUnit`
- Caster economy: Swift available, Standard unavailable, Move unavailable;
  caster remains current actor, matching the already accepted native cooldown
  relationship after a Standard cast
- Acadamae consequence: exactly one completed consequence and one combat-log
  publication; forced natural 20 passed DC 16; no fatigue
- Summon turns: exactly one lawful current-round opportunity and one normal
  next-round opportunity; native commands in both
- Duration/cleanup: exact 12 seconds; expired at cast round + 2; exact cleanup

### KMG multiple summon

- Run ID: `20260826T1701469324812Z-78e74018a3c4426194e6ebae8fc9632a`
- Evidence directory:
  `20260826T1701469168563Z-summon-same-turn-multiple`
- Result: PASS
- Real path/action: legitimate Quickened KMG `1d4+1` option; four exact KMG
  Eagles created through one cast/spawn graph and four summon rules
- Caster economy: Swift spent; Standard and Move retained
- Per-unit result: each distinct Eagle received exactly one lawful cast-round
  opportunity and one normal next-round opportunity, native commands, exact
  initiative association, one duplicate-callback no-op, and 120-second
  lifecycle state
- Cleanup: exact for every created unit

### Ordinary, Acadamae-OFF, cancelled, and non-summon controls

- Run ID: `20260826T1717124156319Z-44e2710f9b4042e0a7b46c6a9a64c668`
- Evidence directory:
  `20260826T1717123909284Z-summon-same-turn-native-control`
- Result: PASS
- Ordinary action: real Full-Round spell, no metamagic, one slot spent,
  Acadamae OFF, no Acadamae consequence
- Native timing: canonical six-second appearance lock and lifecycle grace are
  retained; a locked next-round scheduled entry remains native; first lawful
  opportunity is cast round + 2
- Duration: exact 18 seconds for the two-round control plus native six-second
  grace
- Cancellation: out-of-range real command leaves the slot available and
  produces zero summon rules
- Non-summon control: live combat entity receives no summon lifecycle,
  appearance state, or summon callback

### RTwP control

- Run ID: `20260826T1725541089257Z-fb0b13a2dfab4e3a95e33571bf99925c`
- Evidence directory:
  `20260826T1725540776552Z-summon-same-turn-rtwp-control`
- Result: PASS
- Real path/action: legitimate Quickened summon through the real player path
- Decision: `RealTimeWithPause`; production performs no normalization
- Native state: initial two-second appearance state clears natively; no
  current turn, turn-order entry, or forced turn; native AI issues one command
- Duration/cleanup: exact 120 seconds; exact cleanup

### Fresh Quickened repeat

- Run ID: `20260826T1729361837486Z-690d0e2c18d9463bbd16232d6d070ab0`
- Evidence directory:
  `20260826T1729361665999Z-summon-same-turn-activation`
- Result: PASS 10/10
- Caster economy: Swift spent; Standard and Move remain available; caster
  remains current actor
- Summon turns: exactly one lawful cast-round opportunity and one normal
  next-round opportunity; one current-round `Bite1d4` attack and native AI
  commands in both observed turns
- Duration/idempotence/cleanup: exact 120 seconds; `AlreadyEligible` duplicate
  no-op; exact cleanup

### Cleanup-adjusted Quickened repeat

- Run ID: `20260826T1738486014413Z-aa53964345fa417da346a60852014684`
- Evidence directory:
  `20260826T1738485857381Z-summon-same-turn-activation`
- Result: PASS 10/10 after command-end/scene-dispose cleanup was frozen
- Package SHA-256:
  `747C2EA31528125994300E6B2769E9E38789A68194A66D90A92FEE9568F16F55`
- DLL SHA-256:
  `CDA404CAA5C5916C067CD0AD609399060B668E306026220E9FCD6454387CFE90`
- Result parity: Swift only spent; caster retains Standard/Move/current-turn
  ownership; one lawful summon opportunity in each observed round; one exact
  current-round weapon rule; 120-second lifecycle; duplicate no-op; exact
  cleanup

The implementation also clears its exact-reference command correlation on the
authoritative command end, scene disposal/load transition, and runtime reset.
It owns no serialized marker, initiative entry, turn token, or spawned-unit
state. Complete deterministic domain/reflection tests pass 1,305/1,305 and
runtime preflight passes 154 assertions.

This section is standalone automated runtime qualification of the 0.0.103
investigation source. The hashes identify this checkpoint, not the final
versioned artifact. Optional compatibility profiles, the final 0.0.104
artifact gates/hashes, frozen-source fresh-process repeats, and human
acceptance remain pending.
