# Gunslinger content followup — 2026-10-04

Status: mission in progress. Research and firearm descriptions are qualified. Model D, Whiteout and final
integrated qualification remain in progress.

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
- `dea494233`: substantive description source and four focused tests ported;
  current project/test registration reconciled; actual registered localization
  getters separately qualified by a new guarded scenario.
- Weekend handoff, interim ledgers, tracked `.zcodeignore` ignore change,
  stale version/count assertions and old runtime qualification claims omitted.

## Pending mandatory work

Firearm description port and resolved-string runtime checks; Model D source,
normalization tests and live table observation; strongest safe unpublished
Whiteout policy/observation/mechanics foundation; final integrated gates,
runtime/restoration, audits and pushed clean closeout.

HumanTooltipReview: **NOT_PERFORMED**. Icon assignments are protected-existing;
no new visible consumer or icon is introduced by this checkpoint.

## Firearm description checkpoint

Status: PASS; HumanTooltipReview: NOT_PERFORMED. Item names, prices, identities,
enchantments, acquisition and localization keys are unchanged. Shared Reliable
and Seeking clauses retain their complete limits; Scatter Shot is separated
from lead-ball attack penetration prose. No artwork or icon consumer changed.

Commands and evidence:

- `scripts/Invoke-KmgGate.ps1 -Level Focused -Filter 'firearm-descriptions;midgame'`:
  10/10 selected tests PASS; 1,979 registered.
- `scripts/Invoke-KmgGate.ps1 -Level Sprint`: repository validation, complete
  **1,979/1,979 domain suite**, clean Release build, deterministic package and
  strict standalone package PASS. Local logs: `artifacts/mission/firearm-gate.log`
  and `artifacts/mission/firearm-runtime-batch.log`.
- `scripts/Test-RuntimeScenarioPreflight.ps1`: 475 PASS. Its first run exposed
  stale positive version literals and five missing Phase 2A scenario entries;
  positives now derive the active Info version, negative/legacy tests remain.
- `scripts/Invoke-KingmakerRuntimeTest.ps1 -Scenario observe-firearm-descriptions
  -ExpectedVersion 0.0.141 -ExitAfterCompletion:$true -AllowDirtyGit
  -ReuseInstalledArtifact -PackagePath <qualified-package>
  -DeploymentManifestPath <verified-deployment> -RuntimeLease <owned-lease>
  -Confirm:$false`: **PASS, 8/8 assertions**, fresh Steam App 640820 process,
  no save loaded, no UI input, automatic exit. Evidence directory (under the
  authorized runtime-evidence root):
  `20261005T0454491229974Z-observe-firearm-descriptions`.
- The structured result contains the full actual `BlueprintItemWeapon.Description`
  strings for Pistol, Blunderbuss and The Last Word, exact equality and spacing
  checks, lead-ball-only penetration and both complete property clauses once.
- Qualified DLL SHA256:
  `386675e6017377dc3608561aabb9e57f373366ef964d8efa632e9969983ec8a4`;
  MVID `63b1456f-402d-48cb-b18f-d0ba148ada44`.
  Package SHA256: `080e1faefb4fcbcc3c705c856f5fbc004c4f8f7e3bbdb417fcf9eeb8e832fa4f`.
  Candidate source fingerprint:
  `83ebff022a02dc422d4c6684bcc6170c7660dbd24a2e835464150c5faf344e1a`.
  Embedded parent commit is `f1155cb158188a1264f8b53ac41a64f514fe099a`;
  source was qualified before commit, not misrepresented as clean parent source.
- `Restore-Live-Mod.ps1` verified exact restoration from the batch's own
  `20261005T0454403947334Z` backup; shared lease released. Batch JSON says
  `status=PASS`, `restored=true`.

Runtime guard correctly rejected the elevated tool process before launch.
A hidden PowerShell launched through the existing desktop Shell application
was token-checked as the normal user and ran the unchanged guarded scripts.
No process was killed, OS setting changed, or runtime guard bypassed.
