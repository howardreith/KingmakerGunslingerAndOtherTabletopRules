# Native UI review packet — icon overhaul candidate (rev. 2, owner-usable)

One compact review for the owner's final native-UI acceptance. It concerns
**final UI integration on the candidate below**, not a renewed vote on the
approved paintings (those remain closed; hashes re-verified no-write on
2026-09-18). Read with [ICON-EVIDENCE-LEDGER.json](ICON-EVIDENCE-LEDGER.json).

## Open the picture review directly

**`C:\Dev\KingmakerGunslingerLab\icon-recovery\native-review-gallery-20260918\NATIVE-REVIEW-GALLERY.html`**
— a local gallery of 12 curated captures (per race: one wide creator-Total
context, the first racial action row, the pinned Fight Defensively control
row). Every image link is verified present and its SHA-256 matches the source
run's recorded capture hash (`manifest.json` beside it lists file, stage, run
ID and both hashes). Transitional/duplicate captures are excluded by design.
The gallery and images are machine-local review evidence, deliberately not in
Git. Class label on every image: **portrait-strip native-widget binding
(fixture-reparented rows) — not ordinary menu flow**.

## Exact candidate

| Item | Value |
|---|---|
| Package (local path) | `C:\Dev\KingmakerGunslingerLab\worktrees\icon-art-overhaul-v2\artifacts\local-runtime\0.0.129\KingmakerGunslinger-0.0.129-local-runtime.zip` |
| Package SHA-256 | `2fca98bc2b1abecc69985f605b11d4e8d63c707869bd55cef3a6277e9edc77cd` |
| DLL SHA-256 / MVID | see `runtime-evidence/deployments/20260918T1413202579998Z/deployment.json` (reconciled below) |
| Deployments of this exact package | `20260918T1337006300882Z` (qualification matrix) and `20260918T1413202579998Z` (documentation-stabilized rebuild; byte-identical package, manifest matching the final tree). Same artifact, two deployment records. |
| Restoration procedure | `scripts\Restore-Live-Mod.ps1 -BackupDirectory <session pre-test snapshot>` from the icon worktree; verified 138/138 inventory identical after the matrix |

## What is already proven vs what needs you

| Surface | Status | Evidence |
|---|---|---|
| 39 racial consumers: native-widget binding + rendered control + restoration | PASS | corrected fixture, four race runs + smoke on this package (see ledger run IDs) |
| Higher firearm roots: selection/data/cancellation/cleanup | PASS (cancellation snapshot-proven on the re-qualified artifact) | `disposable-firearm-higher-feat-roots` runs (ledger) |
| Higher-feat **screen** rendering; ordinary racial action flow; activatable; held-touch | **PENDING — you** | [SUPERVISED-NATIVE-FLOW-CHECKLIST.md](SUPERVISED-NATIVE-FLOW-CHECKLIST.md) rev. 2 (catalog-verified expected identities; state-changing steps marked) |
| Wakizashi + P/M/B monogram in the actual weapon selector at your verified settings | **PENDING — you** | checklist item 6 |
| Saved-parameter disk round trip | NOT RUN — authorization/input required | [SAVED-PARAMETER-AUTHORIZATION-REQUEST.md](SAVED-PARAMETER-AUTHORIZATION-REQUEST.md) rev. 3 |

## Notes for the review

- Prior Weapon Focus/Rapid Reload and earlier run captures remain evidence for
  their recorded artifacts only; none are relabeled as this candidate's.
- Rows needing you are marked pending with exact steps; no renewed approval of
  unchanged paintings is requested. Final review concerns UI integration only.
