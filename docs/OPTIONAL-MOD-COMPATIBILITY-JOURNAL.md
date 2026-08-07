# Optional-mod compatibility journal

## 2026-08-07 - Mission start and unchanged baseline

- Repository root: `C:\Dev\KingmakerGunslingerLab\repo\KingmakerGunslinger`.
- Starting branch/commit/version: clean `master`,
  `d03dfe9eae65f5cd1395df7337f21dfdb4357661`, `0.0.71`.
- Isolated branch/worktree: `codex/postbase-archetypes-compatibility` at
  `.worktrees/postbase-archetypes-compatibility`.
- Repository validation: PASS.
- Complete dependency-free domain/reflection suite: 911/911 PASS. The first
  sandboxed run failed only because the sandbox denied the audio fixture's
  atomic `File.Replace`; the identical approved elevated run passed.
- Exact-reference Release, build-output, SoundBank, package creation, and strict
  package validation: PASS. No deployment occurred.
- Package SHA-256:
  `1815C6A37C935A61223D026E03A8E6D50A0D949066CD41F9D2A17479D9197CC2`.
- DLL SHA-256:
  `F879904D51DDAA0B226375048EF0C7983F44158B8441EC1EC4616C00CB204BEB`.
- AssetBundle SHA-256:
  `F52CBC5B2937EE2400D882A7E02CD45272E6A6EB244A7324E78920F265971A0B`.
- SoundBank SHA-256:
  `0E9F88C562F4F937A8941ACE0F241BB31A7ED56B46FBCA549C98F764392EDF18`.
- Stale-state finding: the prior active resume incorrectly summarized the
  attach-slot A/B result. Per the current work-order authority, attach-slot
  Experiment A still left held long guns invisible and the later isolated
  holster-patch experiment restored them; minor clipping is accepted.
  Historical entries remain untouched, including their conflicting summary.
- Next action: inventory every immediate child of the authorized examples root
  without mutation, then implement the committed catalog/schema and fixtures.

## 2026-08-07 - Reference inventory checkpoint

- Read-only run ID: `20260807T1740349239015Z`; 12 immediate children fully
  inventoried with bounded `.git` metadata/object exclusion recorded.
- Loadable primary roots: ArmsArmor 1.0.10, CallOfTheWild 1.14.4c-2.1, and
  ToggleCustomSoundpacks 1.0.1. CraftMagicItems 1.10.0 is source-only.
- Five `KAZ_*` folders proved to be small loadable UMM equipment mods rather
  than raw assets. They are retained as a grouped extension and not promoted to
  a required primary profile without separate manifest authority.
- CallOfTheWild compiled/source versions differ (1.14.4c-2.1 versus 1.14.5), so
  no exact source-twin claim is made. ArmsArmor and Toggle identities/versions
  align but byte equivalence remains unproven.
- Eddic Respec and Bag of Tricks are absent:
  `UNAVAILABLE-LOCAL-REFERENCE`.
- Inventory fixtures PASS: source-only classification, invalid-loadable
  classification, and canonical approved-root escape rejection.
- Version surfaces advanced together to 0.0.72 with a version-aware validator.
- Full 0.0.72 checkpoint gate: repository validation PASS, 911/911 PASS,
  exact-reference Release PASS, build-output/SoundBank/strict package PASS.
  Package SHA-256
  `43D48259B890F7F600DF6E2FFC1B5D142ED0948FF1A1BD4FE1F0181E9779B006`;
  DLL SHA-256
  `BD1BD66C690A4689A2125CE4D6CC8ED3CFC36962ADB222A131F1BDA856FA0339`.
- Next action: run full 0.0.72 source/build/package gates, commit/publish this
  checkpoint, then implement the static GUID/Harmony/bootstrap audit.
- Commit: `274d4d7` (`chore(compat): establish mission and reference inventory`).
- Required push helper: FAIL before network activity; branch is not on its
  external allowlist. No alternative push was attempted. Safe local work
  continues; publication status is blocked.

## 2026-08-07 - Static audit and profile manifest checkpoint

- Added deterministic standard-library lexical audit plus PowerShell wrapper
  and behavior fixture. Fixture proves exact GUID collision classification and
  shared Harmony-target reporting.
- Real audit: five trees, zero cross-owner project-definition GUID collisions,
  three shared Harmony targets. Curated details are in the forensics report.
- Added eight required stable profiles. Craft Magic Items and its combined
  profile are `STATIC-AUDITED-ONLY`; absent extensions remain explicit. The
  high-risk primary combined profile includes the three compiled primary roots;
  the all-loadable extension additionally names all five proven KAZ UMM IDs.
- No runtime compatibility claim is made. Next engineering phase is exact
  profile resolution followed by transactional staging fixtures.
- Full checkpoint gate: repository validation PASS, 911/911 PASS,
  exact-reference Release/build-output/SoundBank/strict package PASS. Package
  SHA-256 `AC9DB772CDA6C228B1CEEA2AC13CE7DDF73BDD2C85A87D0EFE57D827CB4BECAF`;
  DLL SHA-256
  `8891F0709E96282908BB26898BB51D852F61D8497FD859E4B74E8F0AA6857EFA`.

## 2026-08-07 - Publication resumed and transaction core

- Verified clean required worktree/branch at exact `9da61a4`; the updated
  approved helper published successfully and origin matched the local commit.
- Current user authority supersedes the earlier KAZ staging interpretation:
  `KAZ_*` stays an asset-reference group, `runtimeStagingAllowed=false`, and no
  KAZ key or UMM ID appears in the all-loadable runtime profile.
- All eight dry runs PASS without Mods mutation. Six are runtime-capable;
  Craft Magic Items and its Call-of-the-Wild overlay remain static-only.
- Added a committed-profile-only resolver with exact UMM/version/assembly/MVID,
  whole-tree source manifests, intended destination, load order, warnings, and
  runtime-capability reporting.
- Added transaction enter/restore/recovery primitives. Public entry accepts only
  a committed profile ID and exact lab state root. The original Mods directory
  is renamed, never merged; a fresh staged directory carries a sentinel bound
  to an immutable hashed ownership record.
- Filesystem integration tests PASS for normal restoration, copy/profile
  failure, simulated launch failure, destination collision, unresolved prior
  transaction, interruption recovery, staged extra-file detection, original
  hash mismatch preservation, original Mods absent, managed SoundBank
  present/absent, running-process refusal, duplicate restore, spaces in paths,
  and sentinel hash mismatch.
- No real Kingmaker installation, Mods directory, Steam process, save, or
  SoundBank was touched by these fixture tests.

## 2026-08-07 - Guarded runtime observer source checkpoint

- Added `observe-optional-mod-compatibility` to the existing guarded request
  parser/runner. Only the six committed runtime-capable profile IDs are
  accepted; source-only, unavailable, traversal, extra-parameter, and arbitrary
  values fail closed in the focused fixture.
- Exact installed contracts are used: UMM 0.32.4 private `modEntries` for
  ordered entry identities/state and Harmony12 1.2.0.1
  `GetPatchedMethods`/`GetPatchInfo` for owner, role, priority, before/after,
  and order evidence.
- Runtime assertions cover isolated UMM identity, assembly MVID/SHA-256,
  singular base class and 20-level progression, exact Mysterious Stranger
  registration/replacement rows/Charisma binding, five production firearm
  pairs, non-faulted Wwise state, singular Gunslinger patch installation, and
  the observer's save-free boundary.
- Focused observer fixture PASS. Full gate PASS: repository validation,
  911/911 tests, exact-reference Release, SoundBank, and strict package.
  Candidate package SHA-256
  `08DA0786E37F1AC4EC97A0166DEC0FEDBAEC7FE9B6E0168A8984EEA62D12DB22`;
  DLL SHA-256
  `026B7215DCAC0AA1923B11F5A1E79101D0C70872BD462813FC9703C11918F698`.
- No real Mods mutation or Kingmaker launch occurred at this checkpoint.

## 2026-08-07 - First real transaction, runner timing repair

- Transaction `compat-20260807T191144Z-9ce245d1f232` staged only Gunslinger.
  Guarded `mod-load-smoke` run
  `20260807T1912134251961Z-379f7fd088d945fca5a7e663ed6c1262`
  PASS with embedded commit `3fbd5ae`.
- Exact original Mods and managed SoundBank restoration verified `True` at
  `2026-08-07T19:13:16.1390728Z`. Staged mutation was observed and safely
  discarded only after restoration verification.
- The second requested fresh process was correctly refused because the first
  Kingmaker process had committed its result but had not completed automatic
  exit. Root cause is wrapper sequencing, not product compatibility. The
  wrapper now waits boundedly for process exit between scenarios as well as in
  `finally`. No process was killed.
