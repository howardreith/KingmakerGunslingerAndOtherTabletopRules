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
