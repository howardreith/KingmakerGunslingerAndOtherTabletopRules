# Overnight Gunslinger Bug-Fix Journal

## 2026-08-19 mission intake and baseline

- Required branch: codex/gunslinger-overnight-bugfixes.
- Fetched origin/master: d13268d3abe9ffe89c8195b213c1eee194328672.
- Baseline subject: Seal Urban Barbarian 0.0.87 qualification.
- Baseline version: 0.0.87; informational version
  0.0.87-urban-barbarian-human-review-repair-4.
- Primary worktree was preserved because tmp-phase0-clone/ is unrelated and
  untracked. Isolated worktree:
  .worktrees/gunslinger-overnight-bugfixes.
- No pre-existing local/remote overnight branch and no Git lock were found.
- Qualified ancestors 4f28dcf (runtime foundation), 71368cb (accepted rare
  firearms), and 9e71075 (Elven Branched Spear qualification) are present.
- First Build-Local attempt: repository validation PASS, domain compile FAIL
  because isolated worktree lacked ignored GamePath.props and therefore had an
  empty KingmakerManagedDir/Newtonsoft reference. This was an environment
  materialization failure, not a source failure.
- Safe repair: created ignored/uncommitted GamePath.props using the established
  qualified private reference-bundle paths; no network or dependency install.
- Second .\scripts\Build-Local.ps1: PASS. Repository validation PASS; complete
  1,150/1,150 domain/reflection tests PASS; clean exact-reference Release PASS;
  focused supply icons PASS; build-output PASS; SoundBank PASS; deterministic
  standalone and local-runtime packages PASS strict validation.
- Starting local-runtime package:
  artifacts/local-runtime/0.0.87/KingmakerGunslinger-0.0.87-local-runtime.zip
- Package SHA-256:
  7572D7664C02BD07A1DD713971CFD9E9A46C02E32011E0F7A410F25F5D597CED
- DLL SHA-256:
  1E15AE66DEB0132D80F3774DA0A243AAE13388B91874898695926A966736F78F
- Firearm AssetBundle:
  CC9DA6B2FB43FD2932971E3CCE015610497E4C2DB657F62DBA675A31DE327B20
- Elven Branched Spear AssetBundle:
  6E9FE86E43072361EEC3357D9C73E17ADD71D22BAF257FB8C7ED6F52931CE777
- KMG_Firearms.bnk:
  0E9F88C562F4F937A8941ACE0F241BB31A7ED56B46FBCA549C98F764392EDF18
- No deployment or runtime launch occurred during baseline qualification.
- Current issue: durable mission checkpoint.
- Next action: validate, commit, publish, and remote-verify these records, then
  begin Issue 1 source/callback investigation.

## 2026-08-19 Issue 1 source-qualified candidate

- Branch/source parent: `codex/gunslinger-overnight-bugfixes` at
  `879ffe152a4ccfbfe42679055f7c392e5d0f1669`; version remains 0.0.87.
- Diagnosis: Acadamae armed an eligible `UnitUseAbility`, but its completion
  lookup depended on the thread-local `OnAction` stack. A native
  `RuleCastSpell.OnTrigger` outside that stack could not consume the armed
  invocation, producing no save or fatigue. Terminal Harmony targets also did
  not freeze their exact overloads, and the old evidence exposed only DC/pass.
- Repair candidate: bind each concrete `RuleCastSpell` constructed during the
  eligible command to the same tracker entry; consume that exact rule once on
  successful completion; consume failed/canceled commands without a save; use
  explicit `RulebookEventContext` and `OnEnded(bool)` patch signatures.
- The native Fortitude path remains `RuleSavingThrow` at DC 15 + spell level.
  Failed saves still add the canonical Fatigued blueprint with null spell
  context and call native `MakePermanent()`; successful saves add nothing.
- One completed accelerated cast now records and publishes caster, spell,
  natural d20, real Fortitude modifier/total, DC, success/failure, and final
  fatigue disposition. Notification failure is exception-contained and cannot
  change spell or fatigue mechanics.
- The guarded scenario now drives real `UnitUseAbility` commands for a forced
  natural-20 success and forced natural-1 failure, in addition to OFF,
  ineligible, duplicate, cancellation, context lifetime, Cord, and rest paths.
- First `test-domain.ps1` compiled and ran all 1,150 tests; one new
  source-contract count expected two failures instead of the strengthened
  scenario's three. The assertion was corrected without production change.
- Second `test-domain.ps1 -Configuration Release`: 1,150/1,150 PASS.
- `Build-Local.ps1`: repository validation PASS; 1,150/1,150 PASS; clean
  exact-reference Release PASS; focused icon PASS; build-output PASS;
  SoundBank PASS; deterministic package and strict package validation PASS.
- Candidate local-runtime package SHA-256:
  `538FE2A912B07990B12DC20C89D379ED8C3878C36FF41A5F46A5A7A3D8556B7B`.
- Candidate DLL SHA-256:
  `E2CCBCE48C95C2183D37442F3D4E334F6EEF151192C6219CA15EFBE19ADB5116`.
- Runtime has not yet run. Next action: diff/staging safety audit, commit and
  approved-helper publish, exact remote verification, then guarded
  `disposable-acadamae-graduate` qualification on the immutable SHA.

## 2026-08-20 Issue 1 first runtime failure and fixture repair

- Dedicated repair commit `d691d508c43f3c048f28860389a6146186c11448`
  was published; HEAD/local/origin equality was exact before launch.
- Guarded Steam run ID
  `20260820T0358157167579Z-246801cb00f44e4a80f6e69e4dffa28c`
  loaded exact source `d691d508...`, version 0.0.87, then returned `ERROR`.
- Exact failure: `ExecuteAcadamaeNativeCommand` required a detached
  `ChargenUnit` command to reach `Success` after one animation tick, but its
  result remained `None`. No scenario assertion ran, so this is not mechanical
  PASS evidence and is not recorded as an Acadamae production failure.
- Repository precedent establishes that the one-tick native command driver
  requires a caster in an actually loaded area. This scenario is intentionally
  save-free and its detached animation/controller clock cannot advance that
  boundary.
- Materially different safe strategy: invoke the exact protected
  `UnitUseAbility.OnAction()` method through reflection. This retains the real
  Harmony prefix/postfix, native `RuleCastSpell` constructor, `OnTrigger`, and
  ability delivery while avoiding a fabricated animation clock and avoiding
  manual tracker/RuleCast calls.
- Corrected fixture gates: `test-domain.ps1 -Configuration Release` PASS
  1,150/1,150; `Build-Local.ps1` PASS repository, all tests, clean exact-
  reference Release, output, SoundBank, package, and strict package validation.
- New package/DLL SHA-256:
  `04E72E53DF1D8F58CEB9C60FDFD23C164E2EDE25991F02458A44F542843D1B9D` /
  `785DCB24EA3E6B4A91AA9867F9DAAAF374872C71A9857293F0573F1D277173C0`.
- Next action: commit/publish this issue-scoped runtime fixture correction,
  verify remote equality, and rerun the guarded scenario.

## 2026-08-20 Issue 1 second runtime failure and narrowed boundary

- Published fixture checkpoint:
  `b2b8d6edcfa16d0fdb72ea7b1e8ecb2cfe50406c`; exact remote equality verified.
- Guarded run ID
  `20260820T0404361106448Z-a262364c94774a8d9e444d73f7c32e10`
  loaded that exact commit/version and returned `ERROR` before assertions.
- Exact failure: reflection reached the native protected action, but the
  installed patched `UnitUseAbility.OnAction_Patch3` threw
  `NullReferenceException` for the detached `ChargenUnit`. This independently
  confirms the save-free fixture cannot supply the loaded-area command context
  required by the composed engine/mod action chain.
- Strategy change: do not attempt another detached animation/action call. The
  scenario now reproduces the rejected production ordering directly: begin the
  exact command scope, construct the native `RuleCastSpell` so the production
  Harmony constructor patch attaches it, end command scope, then trigger that
  exact rule. This proves the new delayed identity retention and native save/
  fatigue consequence without claiming loaded-area animation execution.
- Actual animation-driven command execution and area-transition persistence
  remain explicit consolidated human checks; they are not silently passed.
- Narrowed fixture passes 1,150/1,150 and the complete `Build-Local.ps1`
  repository/Release/output/SoundBank/package/strict-package pipeline.
- Candidate package/DLL SHA-256:
  `134184B1A08FC73153780A711CEF1A309DE311BD6D2AFBC2BE36B00DA7F604B3` /
  `3B5AEB2122E2F736340A7F504DE56F50BED9314109769502276DFC764D3A07FD`.
- Next action: publish this issue-scoped evidence-mode checkpoint and run the
  guarded delayed-terminal scenario.

## 2026-08-20 Issue 1 delayed-terminal live evidence and roll repair

- Published evidence-mode commit:
  `0708d9d7683bf8646a23efad06631cdddf2e473b`; remote equality verified.
- Guarded run ID
  `20260820T0410154111888Z-395ce15edb3b49a19d1f343d0604f369`
  completed without exception and returned FAIL with 11/13 assertions PASS.
- Passed live boundaries: prepared Summoning identity, default-OFF mode, OFF
  native full-round/no-save behavior, Standard action parity, subsequent
  native failed-save fatigue, cancellation, command snapshot, permanent
  context-independent fatigue, native rest removal, view lifecycle, Cord
  substitution, and cleanup.
- The delayed exact rules were consumed: completed count advanced to one and
  then two, diagnostics published, and failure applied permanent fatigue. The
  two failures were diagnostic expectations: success recorded d20 `-80`/total
  `20`; failure recorded d20 `101`/total `1`.
- Exact installed `RuleSavingThrow` metadata and getter IL resolved the cause.
  `BaseRollResult` computes implicit `D20` plus `StatValue`; `RollResult`
  returns that base total plus `SuccessBonus` only when required. The forced
  completion postfix wrote the desired natural roll into `BaseRollResult`, so
  its setter rewrote the stored d20 to natural minus Fortitude.
- Repair: the existing `RuleRollD20.PreRollDice` hook remains the sole forced
  natural-roll setter; the completion postfix only retains natural-20
  `AutoPass`. Diagnostics now record native `D20.Value`, exact `StatValue`,
  conditional bonus, and final `RollResult` under truthful labels.
- Gates: 1,150/1,150 PASS and full `Build-Local.ps1` repository/Release/output/
  SoundBank/package/strict-package PASS.
- Candidate package/DLL SHA-256:
  `3A3EEDB3CBB88BF0BFF6AB37A3717B102342F7EEC0A45BA6D543CFCCAECEB06D` /
  `EC897F4F87166A88FB8A196BA2EB206430D2E6D25364658D03682CE1724AF123`.
- Next action: commit/publish the native-roll diagnostic repair and rerun the
  guarded delayed-terminal scenario.

## 2026-08-20 Issue 1 final automated qualification

- Published production diagnostic repair `1963a80eb3f49cffa6324df6d43450ef9e3fb05f`.
  Run `20260820T0417236469478Z-8e69f69546804934a84ea4d184d7a05d`
  passed 11/13 and proved truthful native diagnostics: success d20 16 +
  Fortitude 100 = 116; failure d20 8 + Fortitude -100 = -92. Only the
  deterministic natural-roll expectations remained stale.
- Published seed experiment `928fab624ab4cdd6e59a9b98d40d95783c708713`.
  Run `20260820T0423312051777Z-775c65b925a240ec9590c0ab5d46913c`
  again passed 12/14 mechanical assertions but proved Unity RNG seeding is not
  the native saving-throw source.
- Installed behavior showed this saving throw does not enter the existing
  `RuleRollD20.PreRollDice` test hook. The final guarded-only completion hook
  writes base total as forced natural plus the real native `StatValue`; it does
  not alter unarmed production saves.
- Published final test-control commit
  `f807eb1cc3dabf9dc66acaa2b773c029a72dc942`; remote equality verified.
- Guarded Steam run
  `20260820T0428503321600Z-d97a49371e1949c89f3de25aac1c6eff`
  passed 14/14 with exact natural 20 success, natural 1 failure, one save per
  completed eligible cast, permanent context-independent fatigue, native rest
  removal, OFF/cancel/snapshot controls, Cord substitution, and cleanup.
- Runtime candidate package/DLL SHA-256:
  `B6E6F409C5B78C16EDBD98A35E349DCD9F6412C08659A6D1300712DA838996CF` /
  `788E98066FD614F78C3CE42B5ADC680BED45F4AF5C3B4A54AF956E257A1F6DDF`.
- Issue 1 is human-gated only for a loaded-area animation-driven command, area
  transition persistence, and visual combat-log/Cord presentation. Next:
  diagnose Issue 2 Focused Aim transactionality.
