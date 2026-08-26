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
