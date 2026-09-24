# Favored Class Integration — Implementation Report

## PARTIAL — NOT RELEASE QUALIFIED

Every adopted row and every required infrastructure item is implemented.
Each passes its domain tests and guarded native runs on the final local
candidate; the exceptions are G08 and G20, whose optional races have no
playable provider. The candidate is **not** release qualified:
- Four scenario families were not run (E11, E16, L02, L03); E09 is out of
  scope; eleven are only partly observed.
- Owner decisions are still open (section 6).
- 127 visible choices have no art yet.

Nothing was pushed, merged, tagged or published.

## 1. Branch, commits, package and loaded identity

| Item | Value |
| --- | --- |
| Worktree | `C:\Dev\KingmakerGunslingerLab\worktrees\favored-class-integration` |
| Branch | `claude/favored-class-integration` (local only) |
| Baseline | `996105ed9e72259a220be56e2a74e0cc18e5ef47` (master, 0.0.138 release record) |
| Final code commit (candidate) | `ef250f877cda322f75b499b0dec8fbe155e14ce2` |
| Mission commits | 25 code/record commits `7cb2cab92`…`ef250f877`, plus the evidence-record commit that adds this report |
| Package | `artifacts/packages/KingmakerGunslinger-0.0.139-favored-class-integration.zip`, SHA-256 `5947ea172b1a2547e8575007789f242d728818abb35413445a249444195600ea` (not committed) |
| Loaded DLL | `KingmakerGunslinger.dll` 0.0.139, SHA-256 `fe451f35dff61c26bb96fd9ff9d8c7eb23bffe177788899e74a57662e10de8bf`, MVID `2c9d8c5f-c424-4179-91cd-72ca85438c54` (deployment `20260924T0601504282380Z`) |
| Domain suite | 1,801 deterministic cases, all PASS; repository validation PASS; clean Release build PASS |

Commits since the baseline (oldest first): `7cb2cab92` start 0.0.139;
`68e6d9a8a` `5fd8ef951` `c6c748f6d` `2cccec68b` Phase 1 host adapter and grit
slice; `fc9c7cf7a` `b31d563f2` records; `fba7516d3` `cf8a66d21` `732cddae3`
`57116e057` Gunslinger counters, initiative timing (D3 fix) and persistence;
`9ee86bfbe` `11b488be3` `d9d09b8b8` simple geniekin counters; `f2db844c2`
settings file; `4c9409ba4` `4aab2eda8` probe fixes and records; `cc0168d89`
Mostly Human and the scoped host ancestry bridge; `94cb93919` O06–O08;
`fece33038` host-state follows the settings profile; `07c21f29b` I08/S06;
`9b3db78bf` I06/S04; `76e0a1f11` revelation-walk rule-reaction fix;
`6db62e57d` O01; `ef250f877` icon dispositions, target manifest, hook guards.

## 2. Rows and counts

- Catalog: 54 table appearances, 53 distinct options (46 Paizo, 7 Jon Brazer
  Enterprises), 22 canonical effects.
- Scheduled rows: 30. First-party faithful 22 (G01 G02 G04 G05 G06 G07 G08 G10
  G11 G14, I01 I06 I08, O01 O05 O06 O07 O08, S04 S06, U02 U04); adaptations 3
  (I05 I07 O04); optional third-party 5 (G16 G17 G18 G20 G21, profile OFF by
  default). All 30 are NATIVE TESTED or better, except G08 and G20, which are
  CODE COMPLETE with their routes EXPECTED PROVIDER ABSENCE (no playable Goblin
  or Orc race). G02/G04/G07 are SAVE TESTED. Alias: I04 = G11.
- Not published: 23 (P: G09 G12 G13 G22; D: G03 G15 G19 S03 S08 U06 U07; X: I02
  I03 O02 O03 S01 S02 S05 S07 U01 U03 U05 U08); none appears as a placeholder.
- Owned identities: 170 favored-class leaves and helpers
  (`KMG.FavoredClass.*`: 52 revelation targets × 2, 15 performance targets, 4
  bloodline powers × 2, 3 helpers and the Gunslinger/geniekin counters) and 13
  Mostly Human identities; ledger total 2,139 (2,137 active, 2 reserved).
- Targets: `docs/FAVORED-CLASS-TARGET-MANIFEST.md`. I06/S04: 52 revelations, all
  scoped with every audited family (120 rank reads, 40 resources, 118 parameter
  abilities on the final scope). O01: 15 performances (17 areas).

## 3. Profiles and settings

`Mods\KingmakerGunslinger\FavoredClassIntegration.json` (optional,
restart-required, schema 1, never written by KMG): `integration`, `firstParty`,
`adaptations`, `thirdParty`, `mostlyHuman`. Absent or invalid means the charter
defaults: everything ON except `thirdParty`; an invalid file is reported.
Natively observed on `07c21f29b`: third-party ON, Mostly Human OFF, integration
OFF (mechanics suppressed, identities still registered) and an invalid file.
Third-party menus were observed on `9b3db78bf`. The settings file was restored
to absent after every profile run, verified by a direct file check. The owner's
install never had this file.

## 4. Tests run and NOT-RUN gates

Final candidate runs (all through the guarded Steam App ID 640820 launcher):

| Scenario | Run (final candidate `ef250f877`) |
| --- | --- |
| `observe-favored-class-contract` | 20260924T0603184328859Z PASS |
| `observe-favored-class-host-state` | 20260924T0604015343603Z PASS |
| `disposable-favored-class-grit` | 20260924T0604457833407Z PASS |
| `disposable-favored-class-gunslinger-menus` | 20260924T0605443253994Z PASS |
| `disposable-favored-class-gunslinger-mechanics` | 20260924T0606461797306Z PASS |
| `disposable-favored-class-initiative-timing` | 20260924T0607291357025Z PASS |
| `disposable-favored-class-elemental-core` | 20260924T0608135881312Z PASS |
| `disposable-favored-class-mostly-human` | 20260924T0609003356634Z PASS |
| `disposable-favored-class-elemental-advanced` | 20260924T0609525254026Z PASS |
| `disposable-favored-class-oracle-revelations` | 20260924T0601519001416Z PASS |
| `disposable-favored-class-performance-range` | 20260924T0602359332310Z PASS |
| L01 fresh-process persistence (`Invoke-WordOfRecallFavoredClassPersistence.ps1`) | 20260924T0612102884372Z (prepare) + 20260924T0613092420246Z (verify) PASS; transaction word-of-recall-fcb-persistence-20260924T0612076008434Z_556ce961a4974babba75ab6c3145bb78 (settings and complete Mods tree restored) |
| `working-save-smoke` (KMG_AUTOMATION_WORKING, 11/11) | 20260924T0615397121565Z PASS |

Scenario families (`FAVORED-CLASS-COVERAGE.json`): NATIVE TESTED 38, SAVE TESTED 1, PARTIAL 11, DOMAIN TESTED 6, CODE COMPLETE 3, NOT RUN 4, OUT OF SCOPE 1 (64 total).
NOT RUN: E11 (multiclass flow), E16 (ancestry change with fractions), L02
(death/resurrection, area transition, polymorph) and L03 (full respec).
OUT OF SCOPE: E09. PARTIAL (see the JSON notes): E13 E15 M07 M10 M12 M13 M14
M16 M24 M26 L07. DOMAIN TESTED only: H02 H04 H05 E08 E10 L08. CODE COMPLETE
only: M27, L06 (a missing dependency in a saved build is not observed
natively) and L10. Also NOT RUN: the CotW-only and
CotW + Favored Class compatibility profiles (B2, missing fixture), and
fresh-process persistence for the geniekin, aura, pet, power, revelation and
performance counters (L01 covers the Gunslinger counters). The pre-existing
elemental character-creation qualification scenarios were not re-run with the
new Mostly Human Heritage-phase choice; neither was the icon census visual
scenario.

## 5. Deviations and restoration state

- I06/S04 scaling policy: abilities that a revelation gains at a level gate are
  scaled once the revelation actually grants them. The gate itself never moves.
  Five targets have parameter (C) read points beyond the audit's families:
  Aging Touch, Time Hop, Erosion Touch, Form of the Dragon, and Raise the Dead's
  15th-level abilities.
- O01: the performance's shared range text and visual ring are unchanged. Only
  the membership radius of the bard's own area widens.
- Icons: 127 visible choices show Kingmaker's null-icon placeholder (catalog:
  original-required, pending).
- Shared installation: another session (the KingmakerBuffPlanner lab) staged
  its own Mods tree (including KMG 0.0.133) at 05:45:37Z. That was while this
  mission's build-13 candidate was deployed. It restored the tree at about
  05:47:27Z, byte-verified against the build-13 deployment. This mission never
  touched that session's files or process. It resumed only after 90 seconds of
  quiet with no foreign sentinel.
- Restoration: VERIFIED 2026-09-24T06:18Z. `Mods\KingmakerGunslinger` was restored from `runtime-backups/live-mod/20260924T0027014050156Z` with `scripts\Restore-Live-Mod.ps1`. The DLL (`c6cccdac…`, 0.0.136), `Info.json` (`f66de05d…`) and `FeatureModules.json` (`6e24b278…`) match the pre-mission bytes, and all 238 files are identical to the backup (path and SHA-256). `FavoredClassIntegration.json` is absent, and the Favored Class, Call of the Wild and Races Unleashed settings files are unchanged. No compatibility lock is held and Kingmaker is not running.

## 6. Owner decisions

See `FAVORED-CLASS-BLOCKERS.md` (OD-1 … OD-10, D1, D2, B1, B2).
