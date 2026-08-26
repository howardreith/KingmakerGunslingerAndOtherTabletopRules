# Summon Same-Turn Activation Journal

## 2026-08-26 — mission intake and baseline proof

- Fetched `origin` before branching. Local `master`, fetched `origin/master`,
  and the mission baseline are exactly
  `cf1ca7aedf34ee76690f8864daedc9319a8e21a6`; the ancestry check passed.
- Remote identity is
  `git@github.com:howardreith/KingmakerGunslingerAndOtherTabletopRules.git`.
- Starting worktree was clean and exactly synchronized (`+0/-0`).
- Repository version surfaces identify `0.0.103`; the starting commit is tagged
  `v0.0.103`.
- Created `codex/summon-same-turn-activation` directly from that clean SHA.
- Read repository safety/runtime policy and the accepted 0.0.103 summoning,
  Expanded Summoning, Acadamae, fatigue/Cord, duration, packaging, and
  compatibility qualification records before source work.
- Frozen controls include the real player spellbook path, native summon
  duration plus installed six-second lifecycle grace, Acadamae default-OFF
  toggle and Standard action/save/consequence, summon menu acceptance, and
  exact optional-profile restoration.
- No gameplay behavior, version surface, package, game installation, save, or
  external mod has been changed.
- Current uncertainty: the defect and first divergent 2.1.7b turn/combat field
  are not yet proven. No implementation hypothesis is accepted as fact.
- Baseline gates on the branch: `git diff --check` PASS; repository validation
  PASS; complete dependency-free domain/reflection suite PASS 1,288/1,288;
  clean exact-reference Release, output, firearm asset/SoundBank, deterministic
  package, and strict standalone-package validation PASS with zero deployment.
- Baseline local-runtime package SHA-256:
  `70C84677E16283D1822FC6327A4B894CC157DC46B2DCA33C7C0EC124CE13BB40`;
  DLL SHA-256:
  `67BD6751F0748510F15470771F91863F81D20DBA764EE3667150DE3C17C8F365`.
- Next concrete action: inventory current summon/Acadamae/runtime-test source
  and installed turn-based APIs, then add a guarded real-player-path diagnostic
  that reproduces the accelerated summon lost-turn boundary before any repair.

## 2026-08-26 - installed API investigation and failing reproduction

- The installed game and private reference copies of `Assembly-CSharp.dll` are
  byte-identical, SHA-256
  `3B6450FFEC440E296E586F71C711B195AED144B28D53E1CBB29406D18FEF5AFB`.
- Bounded Kingmaker 2.1.7b IL proves `RuleSummonUnit.OnTrigger` decides its
  turn-based appearance branch from
  `Context.SourceAbility.IsFullRoundAction`, the immutable blueprint flag.
  That branch leaves `SummonedUnitAppearBuff` for six seconds and adds six
  seconds to `SummonedUnitBuff` lifecycle duration.
- Installed `AbilityData` is the actual invocation authority: legitimate
  Quicken produces `ActionType=Swift`, `RuntimeActionType=Swift`, and
  `RequireFullRoundAction=false`, while native Summon Monster I's blueprint
  remains `IsFullRoundAction=true`. `AbilityExecutionContext.Ability` survives
  unchanged through `RuleCastSpell`, `ContextActionSpawnMonster`, and every
  genuine `RuleSummonUnit` in that invocation.
- Added guarded scenario `summon-same-turn-activation`. It is autonomous,
  explicitly working-save-only, and creates a disposable level-20 Wizard via
  native level-up APIs. It prepares a real level-five Quickened native Summon
  Monster I slot and uses the actual `UnitUseAbility -> RuleCastSpell ->
  ContextActionSpawnMonster -> RuleSummonUnit` path. It never directly creates
  a summon rule or runs a spawn action.
- Definitive pre-fix run ID:
  `20260826T1445424590411Z-3b96e766c8b144449164781c019dcc51`;
  evidence directory:
  `20260826T1445424287989Z-summon-same-turn-activation`; expected status FAIL.
- The real command was Swift and changed only the caster's native Swift
  cooldown (`0 -> 6`); Standard and Move remained zero and the caster retained
  current-turn ownership. Exact context reference correlation and all three
  real spell/summon boundaries passed.
- At spawn in cast round 1, the genuine summoned dog was live but not yet
  combat-enrolled, carried `SummonedUnitAppearBuff=6s`, and carried lifecycle
  `126s` although `RuleSummonUnit.Duration + BonusDuration` was `120s`.
- At the exact first `TurnController.Prepare` in cast round 1, the summon was
  enrolled, had Standard/Move/Swift available, and had
  `CanActInCombat=true`, but `IsAbleToAct=false` while the appearance lock was
  present. It received no lawful opportunity. At round 2 `Prepare`, the same
  unit had the same resources, the lock was absent, and `IsAbleToAct=true`.
  This is the first proven divergent boundary.
- Scenario cleanup restored the exact 259-unit and three-party-member reference
  snapshots and `Player.IsInCombat=false`; it did not write a save.
- Focused source contract, runtime preflight (150 assertions), repository
  validation, clean Release compilation, and the complete 1,289-test domain
  suite passed while establishing the expected runtime failure.
- No production behavior has been changed yet. Next action: add a pure,
  fail-closed decision policy and a summon-rule postfix that corrects only a
  genuine invocation whose live `AbilityData` is accelerated during its
  caster's current turn.

## 2026-08-26 - narrow repair and first fixed player-path PASS

- Added a stateless, fail-closed policy plus a Harmony 1.2 postfix on the exact
  installed `RuleSummonUnit.OnTrigger(RulebookEventContext)` boundary. The
  runtime requires a real summoning spell with a spellbook, exact invocation
  caster/context identity, turn-based combat, the caster's still-active turn,
  a live invocation that is no longer Full-Round, an immutable blueprint that
  remains Full-Round, and the canonical summon lifecycle/context chain.
- The repair does not insert a turn, assign initiative, run a command, advance
  combat, or mutate caster cooldowns. It removes the exact canonical
  `SummonedUnitAppearBuff` and subtracts only the exact native six-second grace
  from `SummonedUnitBuff`. Owlcat's existing combat enrollment, initiative,
  `TurnController`, action resources, and AI then provide the lawful turn.
- A first diagnostic attempt returned `CasterMismatch`: installed
  `AbilityData.Caster` is the caster descriptor, while `RuleSummonUnit.Initiator`
  is the unit. A second bounded trace proved summon buff contexts are child
  `MechanicsContext` instances rooted in the same exact
  `SourceAbilityContext`, not the raw rule context. The final guards use those
  exact Kingmaker 2.1.7b equivalents; neither guard was weakened.
- Added fourteen focused policy/source-contract cases covering combat/mode and
  provenance exclusions, native timing, missing opportunity, acted state,
  duplicate callbacks, multiple distinct units, following-round state,
  partial compatible state, and ambiguous context/lifecycle failure.
- Complete deterministic domain/reflection suite: PASS 1,303/1,303. Repository
  validation, clean exact-reference Release compilation, output validation,
  firearm asset/SoundBank checks, deterministic package construction, strict
  package validation, runtime preflight, and `git diff --check` all pass for
  this source state.
- First fixed real-player-path PASS run:
  `20260826T1534299629829Z-686e0463d5254e4b871d5f7a7fec1827` in evidence
  directory `20260826T1534299473030Z-summon-same-turn-activation`.
- The legitimate Quickened Summon Monster I remained Swift (`0 -> 6` cooldown),
  Standard and Move remained unspent (`0 -> 0`), and the caster retained turn
  ownership. The exact KMG selection used the real spellbook,
  `UnitUseAbility`, `RuleCastSpell`, spawn action, and one `RuleSummonUnit`.
- The summoned dog received exactly one lawful `TurnController.Prepare` in the
  cast round and issued exactly one authoritative `RuleAttackWithWeapon`
  (`Bite1d4`). It received exactly one normal opportunity in the following
  round. Owlcat queued/replaced two internal `UnitAttack` commands in the cast
  round and one next round; the rule-level action count proves there was no
  duplicate action.
- Canonical lifecycle was exactly 120 seconds, not the failing 126 seconds.
  Re-invoking the production repair returned `AlreadyEligible` and changed no
  buff. Cleanup restored the exact unit/party snapshots, ended combat, and
  wrote no save.
- This is a source-qualified repair and an initial standalone automated runtime
  PASS, not the completed runtime matrix and not human acceptance. Next action:
  checkpoint the repair, then qualify native, Acadamae, multi-creature, RTwP,
  cancellation/non-summon, lifecycle, and optional-mod controls.

## 2026-08-26 - exact accelerated-command correlation and standalone matrix

- The real Acadamae player path exposed a second installed-engine boundary.
  Its command is correctly constructed and executed as Standard, but after the
  prepared slot is spent a later query of the same `AbilityData` can return
  `RequireFullRoundAction=true`. Treating that post-spend getter as historical
  command truth therefore rejected an actual accelerated cast.
- Added an ephemeral exact-reference correlation from the authoritative
  three-argument `UnitUseAbility` constructor through its exact
  `RuleCastSpell` and deferred summon graph. It accepts only real spellbook
  summoning spells whose immutable blueprint is Full-Round and whose command
  was actually constructed as Standard or Swift with a non-Full-Round live
  invocation. Entries end with `UnitUseAbility.OnEnded`, clear on scene
  disposal/load transition and runtime reset, are never serialized, and cannot
  match a different ability, caster, rule, or arbitrary spawned entity.
- Acadamae PASS
  `20260826T1631499603377Z-713c7efcecab4c98963f8ca5a72b6650`:
  the real prepared spell consumed one slot, stayed Standard, produced one
  summon rule and one Fortitude save/publication, retained the accepted native
  `Swift/Standard/Move = available/unavailable/unavailable` post-cast economy,
  and the summon received one lawful opportunity in each of the cast and next
  rounds. Its 12-second lifecycle expired exactly at cast round + 2.
- KMG multiple PASS
  `20260826T1701469324812Z-78e74018a3c4426194e6ebae8fc9632a`:
  one legitimate Quickened KMG `1d4+1` spell created four distinct Eagles.
  Every unit independently received exactly one lawful cast-round opportunity,
  one normal next-round opportunity, one idempotent duplicate no-op, the same
  native initiative, and exact 120-second lifecycle state.
- Ordinary/Acadamae-OFF/negative-control PASS
  `20260826T1717124156319Z-44e2710f9b4042e0a7b46c6a9a64c668`:
  the native Full-Round control was untouched. Installed accepted behavior is
  a six-second appearance/lifecycle grace, a locked next-round scheduled entry,
  and the first lawful opportunity at cast round + 2. The out-of-range real
  command spent no slot and emitted no summon rule; a live non-summon combat
  entity received no lifecycle, appearance buff, or special callback.
- RTwP PASS
  `20260826T1725541089257Z-fb0b13a2dfab4e3a95e33571bf99925c`:
  the shared summon callback returned `RealTimeWithPause`, left native
  two-second appearance behavior and exact 120-second duration intact, created
  no turn/order/forced-turn state, and allowed native summon AI to issue its
  command.
- Fresh Quickened repeat PASS
  `20260826T1729361837486Z-690d0e2c18d9463bbd16232d6d070ab0`:
  Swift alone was spent, Standard/Move remained available, the dog received
  exactly one lawful opportunity and native AI action in each observed round,
  duration remained 120 seconds, duplicate observation was `AlreadyEligible`,
  and cleanup was exact.
- After adding explicit scene-dispose/runtime-reset cleanup, clean Release,
  package, strict-package, and a second fresh Quickened run all passed. Run
  `20260826T1738486014413Z-aa53964345fa417da346a60852014684`
  repeated the exact Swift-only, current-round, next-round, duration,
  idempotence, and cleanup results against package SHA-256
  `747C2EA31528125994300E6B2769E9E38789A68194A66D90A92FEE9568F16F55`
  and DLL SHA-256
  `CDA404CAA5C5916C067CD0AD609399060B668E306026220E9FCD6454387CFE90`.
- Diagnostic attempts that failed before these qualifying runs were retained
  as investigation evidence. They proved the post-slot-spend Acadamae getter,
  the deferred `RuleCastSpell` spawn boundary, ordinary native two-round
  appearance timing, and RTwP's distinct two-second appearance behavior; no
  failed attempt is counted as acceptance evidence.
- Runtime preflight now passes 154 assertions. The complete deterministic
  domain/reflection suite passes 1,305/1,305, including exact installed-type
  source contracts and correlated/idempotent policy cases.
- Standalone matrix qualification is automated evidence, not human
  acceptance. Next action: checkpoint and push this tranche, run the supported
  compatibility profiles, advance once to 0.0.104, and repeat the principal
  accelerated cases from fresh processes against the frozen versioned build.
