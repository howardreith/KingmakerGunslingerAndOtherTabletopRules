# Z-FIREARM-MAINTENANCE — Mission State

**Mission ID:** `Z-FIREARM-MAINTENANCE`

**Last updated:** 2026-09-12 (P0 session 1)

## Mission status

`IN_PROGRESS` — phase **P0 (baseline and durable setup)**

First incomplete acceptance IDs: all of F01–F06, R01–R10, A01–A08, W01–W02,
C01–C06, Q01–Q03 (`NOT RUN`; see `docs/FIREARM-MAINTENANCE-ACCEPTANCE.md`).

## Identity snapshot

| Item | Value |
| --- | --- |
| Host | Windows 10 (10.0.19045 x64), Git Bash |
| Lab root | `C:\Dev\KingmakerGunslingerLab` |
| Worktree | `C:\Dev\KingmakerGunslingerLab\worktrees\firearm-maintenance` (isolated; main checkout left on `master`) |
| Branch | `codex/z-firearm-maintenance-rest-safety` |
| Base SHA | `71af37acc1dc7548fecb067bf753c11d6b90f893` (= `origin/master`, verified in sync after fetch) |
| Current HEAD | `71af37acc1dc7548fecb067bf753c11d6b90f893` (no mission commits yet) |
| Last qualified/pushed commit | none on this branch yet |
| Remote verification | branch not yet pushed; origin/master == base SHA confirmed |
| Dirty files | mission records only (this file, JOURNAL, MISSION copy, two docs) |
| Installed mod | `...\Pathfinder Kingmaker\Mods\KingmakerGunslinger` Info.json `0.0.126` (matches master release record) |
| Installed DLL SHA-256 | NOT VERIFIED yet (record during baseline slice) |
| Candidate version | NOT BUILT (expected next version 0.0.127 — confirm no later release before allocating) |
| Build env | dotnet 8.0.424 SDK, Python 3.14.7, MSBuild via vswhere fallback, reference bundle present at lab `private\extracted-references\KingmakerGunslinger-private-build-references`, .NET 4.7 ref assemblies assumed present (verified by Build-Local on first run) |
| Worktree script support | verified: `Get-KmgRepositoryRoot` resolves relative to script dir; push wrapper takes `-RepositoryRoot`; reference bundle walk-up reaches lab `private\` from worktrees\ |

Other work preserved: main checkout clean on `master`; unrelated worktree
`worktrees\share-transmutation-instant` (branch `codex/share-transmutation-instant`,
clean at `636d70bf`) untouched.

## Implementation decisions so far

None yet — investigation phase. Mission source map (section 5) verified: all 16
listed files exist at the stated paths under `src/KingmakerGunslinger/`.

## Results

| Gate | Result | Evidence |
| --- | --- | --- |
| Identity verification | PASS | journal 2026-09-12 entry 1 |
| Domain/build/package/runtime | NOT RUN on any candidate (no source changes yet) | — |

## Active processes / fixtures / cleanup

- None. No runtime launches, no installs touched, no leases held, normal install
  is the untouched 0.0.126 release (backup/restore not yet needed).

## Blockers

None.

## Next concrete actions

1. Trace field-repair path: read `Recovery/RepairTestMusketRuntime.cs`,
   `Recovery/RepairTestMusketAbilityLogic.cs`,
   `Recovery/FirearmRepairTransactionService.cs`,
   `Recovery/FirearmItemRepairStateStore.cs`,
   `Actions/FirearmActionPolicy.cs`, `Actions/ExactEquippedFirearmResolver.cs`;
   record current Wrecked-field-repair behavior and exact eligibility checks in
   the contract doc.
2. Trace rest boundary: read `Gunsmithing/CraftingRestResetPatch.cs`, find
   native `RestController`/rest-completion call sites in the installed
   assemblies' usage within the repo, record candidate integration points.
3. Trace misfire/attack continuation: read `Misfires/FirearmMisfireRuntime.cs`,
   `Firing/FreeActionFullAttackReloadPatch.cs`,
   `Reloading/FullAttackAutoReloadPolicy.cs`,
   `Firing/EmptyFirearmAttackCommandPatch.cs`,
   `Firing/FirearmDischargeRuntime.cs`; record current auto-continuation
   behavior and the native command lifecycle used.

Update this file after every coherent slice. Keep it an index; details live in
the journal and acceptance matrix.
