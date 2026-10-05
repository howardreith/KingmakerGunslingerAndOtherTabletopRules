# Gunslinger content followup — 2026-10-04

Status: mission in progress. This checkpoint qualifies corrected research and
baseline reproducibility metadata only. Later mandatory slices remain pending.

## Repository and isolation

Repository: `howardreith/KingmakerGunslingerAndOtherTabletopRules`.
Exact fixed starting SHA: `5482db429bd3c4009a031aa733a3091edfa5fe5e`.
Branch: `codex/gunslinger-content-followup-2026-10-04`.
Worktree: `C:/Dev/KingmakerGunslingerLab/worktrees/gunslinger-content-followup-2026-10-04`.
The exact Phase 2A ref and weekend ref were fetched without pulling/rebasing.
The exact base exists and equals the fetched Phase 2A head; local master is its
ancestor. Initial local/remote master: `2ce70e4e7e9d3c97ca1008ab05a341e758f5cf5a`.
Initial remote Phase 2B: `cdec0180b948dea0c41f4d6b09d97b37f5c478ad`.
The main worktree remains on the weekend branch. No foreign worktree was edited.
No merge, tag, release, PR, version bump or master/main write is authorized.
Final HEAD and remote push verification will be recorded at closeout.

## Baseline qualification and meaningful failures

The fresh fixed-base checkout had two stale current-file hashes in the icon
catalog. The working bytes exactly matched `git show` for both targets:

- `blueprints/blueprints.json`: `8d1893c096e94d4c5130a7e817b5187218b269f731fb3a4a8556ef041e659207`.
- `assets-source/original-icons/expanded-summoning/icon-manifest.json`:
  `da7bf57584e1fcdcbd0d65c9e2dece48fa9d5e86c588ee8898c16035abb97c51`.

Only those two current-file pins were reconciled. No artwork, consumers,
publication flags, protected assignments or historical published-registry hash
changed. Existing icon corruption fixtures remain in the validator.

The fresh worktree also required ignored `GamePath.props` with both
`KingmakerInstallDir` and the test project's `KingmakerManagedDir`. The existing
setup script creates only the former; the latter was added locally. The first
two domain compilation attempts failed for missing Newtonsoft references.
After that repair, the complete suite discovered **1,975 registered tests**, with
two failures because `artifacts/inspection/bodyguard-native/Assembly-CSharp.il`
was absent. The installed SDK's `ildasm.exe` generated that required ignored
artifact from the installed game's `Assembly-CSharp.dll`. No software was
installed, test skipped, assertion weakened, or proprietary material committed.

`powershell.exe -NoProfile -ExecutionPolicy Bypass -File scripts/Invoke-KmgGate.ps1 -Level Sprint`
then passed the complete repository wrapper, all **1,975 tests**, exact-reference
clean Release build, output checks, deterministic package creation and strict
standalone package validation. Log: `artifacts/mission/research-gate.log` (local).
Version remains **0.0.141**. This documentation/metadata checkpoint launches no
game, per the runtime policy; it makes no gameplay qualification claim.

## Initial port ledger

- `bc83e0044`: substantive trait research imported and rewritten with required
  Fiery Glare adaptation, Stoic same-effect prerequisite/acceptance table,
  Earthsense current-engine omission, and FC conditional offering contract.
- `ee88a6361`: engine findings retained; owner decisions replace open proposals;
  unpublished status, 10%/28% ordering, Seeking bypass and indoor prerequisites
  are explicit. No runtime implementation claim yet.
- `7e7cee271`: 62-row historical before-ledger retained; transposed CSV fields
  corrected; finite stock and Model C concentration claims corrected; exact
  Model D selected and documented. Vendor implementation remains pending.
- `dea494233`: inspected; substantive source/test port pending.
- Weekend handoff, interim ledgers, tracked `.zcodeignore` ignore change,
  stale version/count assertions and old runtime qualification claims omitted.

## Pending mandatory work

Firearm description port and resolved-string runtime checks; Model D source,
normalization tests and live table observation; strongest safe unpublished
Whiteout policy/observation/mechanics foundation; final integrated gates,
runtime/restoration, audits and pushed clean closeout.

HumanTooltipReview: **NOT_PERFORMED**. Icon assignments are protected-existing;
no new visible consumer or icon is introduced by this checkpoint.
