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
