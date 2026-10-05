# Gunslinger content followup — 2026-10-04

Status: mission in progress. Research, firearm descriptions and Model D are qualified. Whiteout and final
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

## Model D checkpoint

Status: PASS. Firearm checkpoint commit is
`3cbeb4c6766eb7a500a9fbbc58d0d1e28768d5aa`; its approved-wrapper push matched.
Model D uses a dedicated `OlegFirearmVendorBlueprints` publication independent
of the Eastern/spear/BV/FC/BTSL gates. The existing exact-reference transaction
now normalizes owned stock before assignment; wrong quantities, duplicate owned
rows and retired rows are replaced atomically. Native/unknown foreign rows retain
reference identity and order. Capital includes all generic Eastern/spear identities
in cleanup even when those modules are disabled. No purchased item is inspected
or removed. Materialized merchant inventories are native persisted state; no new
refill or destructive item-instance migration is introduced.

| Surface | Before | After |
|---|---|---|
| Oleg Gunslinger | no firearm/supply rows | Pistol/Musket/Blunderbuss 1 each; powder/ball 50 each |
| Capital Gunslinger | mundane 3 + plus-one 3, each 1; powder/ball/cartridge 200 each; kit 1 | plus-one 3, each 1; powder/ball/cartridge 200 each; kit 1 |
| Bokken | powder/ball/cartridge 100 each; kit 1 | powder/ball/cartridge 100 each; no kit |
| Eastern generics | capital 12; Oleg 6; Dire Narlmarches/Pitax 12 each; quantity 1 | capital 0; all regional rows/quantities unchanged |
| Spear generics | capital 6; Oleg 4; Dire Narlmarches/Pitax 6 each; quantity 1 | capital 0; all regional rows/quantities unchanged |
| BTSL | Honest Guy six firearm equipment rows, one each; Xelliren powder/ball/cartridge 200 each and kit 1; Eastern 12 and spear 6 on each Honest Guy | unchanged |
| Better Vendors | 50 catalog rows; exact Military I/III/V/VII/IX rank schedule and quantities/ledger | unchanged |
| Skeletal Salesman | Roadwarden and Dead Reckoning, one each on existing generated C3/C4 paths | unchanged |
| Named fixed loot | existing five firearm placements, 18 Eastern and six spear named placements, quantity 1 each | unchanged |

These are fixed stock entries that can deplete. Native regeneration is not
guaranteed. No renewable or infinite supply is claimed. Existing crafting and all
prices, GUIDs, enchantments, recipes, initial equipment and loot paths are retained.

The imported before-audit had additional factual errors: 4 rather than 6 capital
spear rows, invalid shorthand/localization symbols, two missing spear variants,
and omitted World-Tree Severer. CSV is corrected to **65 rows**, with all
single-item symbols checked against the fixed registry. The explicitly grouped
progression row is not an item identity. Capital concentration is **28 to 7**.

Validation:

- `scripts/Invoke-KmgGate.ps1 -Level Focused -Filter 'model-d;paper-cartridge;unified-repair'`:
  **34/34 PASS**, including 26 new Model D cases; actual registered count **2,005**.
- `scripts/Test-RuntimeScenarioPreflight.ps1`: **475 PASS** plus teleport metadata.
- `scripts/Invoke-KmgGate.ps1 -Level Sprint`: full repository wrapper,
  **2,005/2,005 domain tests**, clean Release, deterministic package and strict
  standalone package PASS. Logs: `artifacts/mission/vendor-gate.log` and
  `artifacts/mission/vendor-runtime-batch.log` (local, not committed).
- Guarded `observe-model-d-vendors`, same arguments/lease/deployment discipline
  as the firearm checkpoint: **26/26 runtime assertions PASS**, fresh Steam
  App 640820 launch, no save load/input/inventory mutation, automatic exit.
  Evidence: `20261005T0522299980364Z-observe-model-d-vendors`.
- DLL `ace366cd36cdd625a6d052b7d23ebf043b2f7710484b83b0ca4ab2580f3b85fa`;
  MVID `15c79d68-b35b-4691-8990-8375b5ba44cc`;
  package `feb551e65183ef5082ece3b9d90e1cc2c7df0ba3d61facf2f59567703ea7f3df`;
  source fingerprint `d60607ea0dd8962c66a2206c496eb218251a6b9a1c2be5b13180555c30489eb7`.
- Exact live backup `20261005T0522221476379Z` restored and verified; own shared
  lease released. Batch record: `PASS`, `restored=true`.

Meaningful failures and repairs: legacy source guards expected removed stock or
inlined ownership code; they now check the Model D contract and shared normalizer.
A Windows text rewrite temporarily broke an LF-specific source assertion; canonical
UTF-8/LF was restored, with no assertion weakened. The icon guard correctly
rejected bootstrap wiring changes; automatic approval review rejected an overly
broad generated hunk proposal. A read-only proof established exactly five literal
vendor-only substitutions whose reversal equals the prior complete source; that
narrow ledger update was then accepted. Original icon baseline hashes, assignments,
artwork and all earlier integrity checks remain enforced. The validator import
created one local Python cache; that exact generated file was removed before the
passing gate. No generated or proprietary artifacts enter the commit.

## Whiteout policy/observation checkpoint

Status: PARTIAL-SAFE-FOUNDATION; further bounded engine investigation remains in
progress. No marker/buff blueprint, attack patch, public setting, icon, trait/feat
selection, automatic racial grant or player acquisition path exists. New code is
pure policy, per-owner state/replay models, and guarded read-only observation.

The 34 focused policy cases cover Rain/Snow at Light through Storm, inactive
clear/Normal/unmarked/Fog/VFX cases, 1/10/11/100 boundaries, independent exhaustive
28% stacking, ordinary-miss short circuit, Seeking/IgnoreConcealment, both attack
kinds, replay protection, independent unit state and lifecycle cleanup. No source
or domain-test result is represented as an installed combat patch qualification.

`Invoke-KmgGate.ps1 -Level Focused -Filter whiteout-policy`: 34/34 PASS, actual
registered count **2,039**. `Invoke-KmgGate.ps1 -Level Sprint`: repository wrapper,
2,039/2,039 full suite, clean Release and deterministic/strict package PASS.
`Test-RuntimeScenarioPreflight.ps1`: **480 PASS**. The new positive preflight
initially lacked required per-stage timeouts; it now supplies the same existing
working-save timeout contract, retaining all negative save/parameter/exit cases.

Guarded `observe-whiteout-weather -ExpectedVersion 0.0.141 -SaveName
KMG_AUTOMATION_WORKING -TimeoutSeconds 900 -CompletionTimeoutSeconds 600
-ExitAfterCompletion:$true` (exact artifact reuse, owned lease, dirty-source
fingerprint and deployment receipt) produced:

| Run evidence directory | Result | Exact finding |
|---|---|---|
| `20261005T0536125560594Z-observe-whiteout-weather` | FAIL | 5/6 assertions passed; the observer incorrectly required a weather-change notification while actual intensity stayed Clear; all restoration/write checks passed |
| `20261005T0540413348351Z-observe-whiteout-weather` | PASS | 6/6; two native area unload/load pairs; zero weather-change notifications recorded without fabricating events |

Both runs observed mansion `2849fdde28fe50f4d935bf2cf3405051`: indoor true,
Normal/Clear; Oleg `ead426a6c23d39548a670ee515d77df4`: indoor false, visual
Rain but CurrentWeather=ActualWeather=Clear and intensity 0. Returning to the
mansion restored Normal/Clear. The proposed marker/weather predicate was false
throughout. No active precipitation or combat effect has been qualified.

The exact native attack-stage signature exists:
`RuleAttackRoll.TryOvercomeTargetConcealmentAndMissChance(): bool`, private
instance, zero parameters, IL length 149, native MVID
`07fa1e4d-8618-41b3-9b8d-faa17d3b26f7`. This is a potential narrow patch boundary,
not a missing-method blocker or proof that a patch is safe.

PASS assembly: DLL `6a09eea3944a444a1007fc9c50a8297f74332e505ff8a5e97072ca9595681495`,
MVID `da93a2fc-a3ba-4ed8-a2b7-337ec4769e00`, package
`53eed3d5dc83c54cb4343b8b8f9a6e3ad39cda41e6d5a3fe37ecb524b39c2144`, source
fingerprint `4b7cdadb234d7497b3495c6bd0c6faa130073a181e37b658439a29f9c1773311`.
Both independent Steam runs auto-exited, observed zero save writes and verified
exact restoration of their own live-mod backups. Successful repeat backup:
`20261005T0540340565882Z`; owned lease released.

Remaining investigation: native scene projection has an explicit
`BlueprintAreaPart.OverrideWeather` branch even for indoor parts. Read-only
registered area metadata can narrow whether there are authored precipitation
cases suitable for further observation. The working save's Clear weather cannot
by itself qualify an active-precipitation indoor decision or a new combat patch.
