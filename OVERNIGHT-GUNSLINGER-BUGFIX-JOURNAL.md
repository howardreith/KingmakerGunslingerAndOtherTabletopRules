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
