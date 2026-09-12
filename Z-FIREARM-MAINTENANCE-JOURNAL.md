# Z-FIREARM-MAINTENANCE — Journal (append-only)

Format: `YYYY-MM-DD #N — topic`. Concise entries; evidence paths under
`C:\Dev\KingmakerGunslingerLab\runtime-evidence\` when runtime work starts.

---

2026-09-12 #1 — Mission start, identity verification, worktree setup (P0)

- Read mission (owner download copy, 339 lines; copied verbatim to
  `Z-FIREARM-MAINTENANCE-MISSION.md` in the worktree), root `AGENTS.md`.
- Main checkout `C:\Dev\KingmakerGunslingerLab\repo\KingmakerGunslinger`:
  clean on `master` @ `71af37ac`; `git fetch` confirmed `origin/master` ==
  `71af37acc1dc7548fecb067bf753c11d6b90f893` (no drift).
- Existing unrelated worktree `worktrees\share-transmutation-instant`
  (branch `codex/share-transmutation-instant`, clean @ `636d70bf`) — preserved,
  untouched.
- Created isolated worktree `worktrees\firearm-maintenance` with new branch
  `codex/z-firearm-maintenance-rest-safety` from `master` @ `71af37ac`
  (mission-suggested branch name).
- Verified worktree compatibility of the toolchain up front:
  - `scripts/common.ps1` `Get-KmgRepositoryRoot` resolves relative to the
    script directory → worktree root. ✔
  - `codex-policy/Push-KingmakerGunslinger.ps1` takes `-RepositoryRoot`, runs
    `git -C`, requires `codex/*` branch + clean tree + approved origin URL +
    fast-forward. No merge/tag/release behavior. ✔
  - `scripts/Build-Local.ps1` walks up ≤6 parents from repo root to find
    `private\extracted-references\KingmakerGunslinger-private-build-references`;
    lab root has it → reachable from `worktrees\firearm-maintenance`. ✔
    NOTE: Build-Local currently pins active version `0.0.126`; candidate
    version allocation later must update that pin as part of the established
    workflow (check how prior missions did it, e.g. 0.0.125→0.0.126).
  - `scripts/Deploy-Local.ps1` / `Backup-Live-Mod.ps1` hard-pin the exact live
    mod dir and backup root; deployment is backup-first and reversible. ✔
- Installed mod: `Info.json` version `0.0.126`, matches master release record.
  Game install at `C:\Program Files (x86)\Steam\steamapps\common\Pathfinder Kingmaker`.
- Build tools present: dotnet SDK 8.0.424, Python 3.14.7; MSBuild resolved via
  vswhere fallback at build time (same as prior missions).
- Mission source map verification: all 16 files listed in mission §5 exist at
  the stated paths under `src/KingmakerGunslinger/`. No renames needed in the
  initial contract source map.
- Created durable records: MISSION (copy), STATE, JOURNAL (this file),
  `docs/FIREARM-MAINTENANCE-CONTRACT.md` (initial),
  `docs/FIREARM-MAINTENANCE-ACCEPTANCE.md` (all IDs, NOT RUN).
- No source changes, no builds, no runtime launches this slice.

Strategy note: P1 (interruption) precedes P3 (repair restriction) per mission
§6 ordering, to avoid shipping an intermediate candidate where accidental
automation has harsher consequences.
