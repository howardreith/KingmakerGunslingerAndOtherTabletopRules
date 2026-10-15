# Claude takeover handoff — Sprint 24 theropod dinosaurs

Date: 2026-10-14

Machine: `HOWARD-LAPTOP` only

State: **PAUSED BY OWNER; IN PROGRESS; NOT BLOCKED; NOT RELEASED**

Mission boundary: finish Sprint 24, then stop. Sprint 25 was not started.

This document is a resume map, not a replacement for the owner mission or the
current instruction chain. Read the repository's `AGENTS.md`,
`docs/VALIDATION-POLICY.md`, the applicable guarded-runtime and save procedures,
and the current art instructions before taking action. The current policy
overrides stale chronology later in older handoff text.

## Non-negotiable scope and safety

- Work only in `howardreith/KingmakerGunslingerAndOtherTabletopRules` on
  `HOWARD-LAPTOP`. Do not use or launch anything on DATA.
- Continue only Deinonychus and Tyrannosaurus. Do not begin Sprint 25,
  Brachiosaurus, Elephant changes, companions, polymorph forms, mounts,
  expanded-summoning feats, or another repository.
- Never launch `Kingmaker.exe` directly. Use Steam App ID 640820 and the
  repository-owned guarded wrappers, leases, deployment receipts and restores.
- `KMG_AUTOMATION_BASELINE` is immutable. Only the authorized disposable world
  and `KMG_AUTOMATION_WORKING` may be mutated by their exact procedures.
- Never edit a save manually. Do not make account, entitlement, purchase,
  credential, cloud-sync, or protected-save decisions.
- Do not force-push, rewrite history, reset/clean, commit to master directly, or
  bypass the push wrapper. Preserve all failed evidence.
- Private Kingmaker assemblies, bind data and donor captures remain local. Do
  not redistribute native meshes, textures, animations, assemblies or bundles.
- The only accepted Sprint 24 limitations are
  `ACTIVE_SUMMON_GRAPPLES_RESET_SAFELY_ON_RELOAD` and
  `SWALLOW_WHOLE_INTERIOR_AC_HP_UNMODELED`, at their already-proven boundaries.
  There is no new waiver.
- `HumanReview` remains `NOT_PERFORMED_NONBLOCKING` unless the owner actually
  reviews the art.

## Repository and exact identities

| Item | Exact state at handoff |
| --- | --- |
| Product branch | `codex/expanded-summoning-sprint24-deinonychus-tyrannosaurus` |
| Draft product PR | `#45`, directly against `master` |
| Documentation-only final-records branch | `codex/expanded-summoning-sprint24-release-records` — not created yet |
| Released base seen at intake | `e825411a8f920fdb9447f7ad48d3f7d0267e6e32` |
| Stable baseline | `v0.0.153` |
| Expected next patch | `v0.0.154`, but recheck before freezing it |
| Last product/tested source | `fb7e471a9877fb68cc0cb26feadc04615882a1b2` |
| Reporting parent before this handoff | `83c9c9ea4a7c6d6e6f7c37538316e01344040f40` |
| Push-policy SHA-256 | `9354658937f2e679d3b77493a8bc56eed2395a53b8da3614b4e2cd5a05246542` |

The commit containing this document cannot name itself. After fetching, resolve
the current local and remote product-branch tip and require them to match. The
only expected changes after `fb7e471a` are reporting documents. Do not discard a
newer valid descendant if another owner-authorized change has landed; review it.

The external push policy contains exactly the two owner-authorized Sprint 24
branches. Do not add a wildcard or alter another push rule. Push checkpoints
with:

```powershell
powershell.exe -NoProfile -ExecutionPolicy Bypass -File C:/Dev/KingmakerGunslingerLab/codex-policy/Push-KingmakerGunslinger.ps1
```

## Frozen hidden candidate

The exact hidden candidate is local and ignored, not a release asset:

`artifacts/local-runtime/0.0.153/KingmakerGunslinger-0.0.153-local-runtime.zip`

| Identity | Value |
| --- | --- |
| Product commit | `fb7e471a9877fb68cc0cb26feadc04615882a1b2` |
| Source-state SHA-256 | `c88839e925fda17df86b273c7c4088baa69a9a716d3f36db75f9affd8aaa0474` |
| Local-runtime ZIP SHA-256 | `333a8c8772f089fd5b1e2d47bcfcc92c53f375360c120944fdf7b3cff0c82051` |
| Compile-candidate ZIP SHA-256 | `750089752b4b37b5e7e5b0c5fb2761eb705d8b67463321d2c0fc3c7d94f9cad5` |
| DLL SHA-256 | `24af1a3fb3b88410b26656ad2d457edf245b24012858c3ac26e6d316f206c867` |
| DLL MVID | `cd939d5b-6678-4c1f-8e26-ad5c761992d7` |
| Build-local manifest | the ZIP path plus `.build-local.json` |

Its fresh `Invoke-KmgGate.ps1 -Level Sprint` run passed repository validation,
2,607/2,607 domain tests in 94.3 seconds, exact compilation, packaging and
strict validation in 192.1 seconds total. It did not deploy or launch the game.

Do not modify the build-local sidecar, rename the ZIP, restamp its DLL, or call
a reporting-only rebuild the same artifact. If any product/package input has
changed since `fb7e471a`, this artifact is stale and a new coherent candidate
must be qualified normally.

## What is already green

The detailed source, requirements, routes, failures and proof paths are in:

- `AUTONOMOUS-RESUME.md` — read the top stopping checkpoint first;
- `docs/SPRINT24-COVERAGE.md` — authority and requirement-to-proof map;
- `planning/SPRINT24-THEROPOD-DINOSAURS-LEDGER.json` — machine-readable ledger.

Fresh native results already retained as PASS include:

- `rtwp-foundation`: 26/26, both adult profiles and all 18 hidden real-parent
  routes;
- Deinonychus RTWP: 10/10;
- Deinonychus turn based: 11/11;
- Tyrannosaurus RTWP: 10/10;
- Tyrannosaurus turn based: 11/11;
- final hidden visual lifecycle: 26/26, including original bodies, contacts,
  movement/crowding, failure rollback, ownership and teardown.

Mechanically, Deinonychus has exactly two primary talons, one primary bite and
one secondary foreclaws roll. Its legal charge uses native Pounce once; ordinary
single attacks, illegal/aborted charges and AoOs do not become free routines.
Tyrannosaurus has exactly one 4d6 bite with signed twice-Strength Powerful Bite,
19–20 threat, printed critical feats, bite-only Grab, later-turn Swallow Whole,
and its own once-per-round 2d8+11 interior packet. Same-size Grab and one-size-
smaller swallow are distinct. No invented claws, Pounce, Powerful Charge,
automatic confirmation or immediate swallow exists.

The deterministic original-art review passed 20 offline frames after retaining
and correcting a jaw-pivot failure and a neck/tail-transition failure. Blender
rendering took 181.63 seconds; the sorted review-manifest SHA-256 is
`504ea9cfaed5723e74b3b67b81ef43155baf71877510de80bbc9de3b950331ea`.

Both creature keys remain suppressed. Current public totals are still 1,075
generated placements plus 29 native wrappers, or 1,104 visible choices. The
qualified public target is 1,093 generated placements plus 29 wrappers, or
1,122 visible choices. Identities are already append-only; publication must not
reallocate them.

## The one unfinished hidden native phase

`shared-controls` is not green. The last guarded run used the preceding broad
AI-controller strategy. Four unaffected-consumer rows passed—released Leopard,
released Grab/Purple Worm swallow, and Triceratops +7/+9 Strength controls—but
execution stopped fail-closed because the controller candidate set contained
nine protected party actors.

- Evidence directory:
  `C:/Dev/KingmakerGunslingerLab/runtime-evidence/20261014T1338497545977Z-disposable-expanded-summoning-sprint24-native-review`
- Run ID: `20261014T1338497858128Z-db9c9e1da2754e1398037313c3b5a76b`
- Runtime duration: 90.379 seconds
- Transaction: `compat-20261014T133710Z-3cf6f796211f`
- Restored at: `2026-10-14T13:40:41.1768330Z`
- Restoration verified: true

The current product source fixes only that observer seam in
`src/KingmakerGunslinger/RuntimeTesting/RuntimeTestRunner.ExpandedSummoningSprint24Review.cs`.
It records the ambient eligible set but invokes Owlcat's exact per-unit sequence
only for the disposable Deinonychus:

`DecisionContext.InitGroup(owner.Group) -> AiBrainController.TickBrain(owner) -> DecisionContext.ReleaseGroup`

It uses an ephemeral Default-mode token because the guarded fixture runs below
Dialog mode, restores mode/time/pause exactly, never ticks protected party
actors, never issues a fixture attack, and requires a native AI-owned
`UnitAttack`, actual weapon rolls and at least 0.25 m of relative approach. This
correction passed the named focused gate and the complete Sprint gate, but it
has **never been run natively**. Do not report it as PASS until the guarded
result says so.

## Tomorrow's intake

Before any launch:

1. Read the instruction files named above plus the relevant sections of
   `docs/WIN10-AUTONOMOUS-RUNTIME-TESTING.md` and the current save-observation
   procedure.
2. Confirm the host is `HOWARD-LAPTOP`, the repository/worktree is clean, no
   unfinished Git operation exists, and PR #45 still targets master.
3. Fetch origin and tags. Verify current `origin/master`, Latest stable release,
   tag availability and any intervening descendant. Do not assume the intake
   master or `v0.0.154` is still current/free.
4. Verify no Kingmaker process, conflicting publisher/source owner, active
   runtime transaction, compatibility lock, deployment staging or publisher
   checkout exists.
5. Take a fresh machine snapshot. The last measured cross-check was 968 Mods
   entries, installed KMG 0.0.117 with 136 files, 83 settings and 118 saves, but
   tomorrow's actual state is restoration authority.
6. Verify the exact ZIP, sidecar, DLL and source identities above. Also require
   this command to show no changed product/package input between the tested
   source and the reporting head:

```powershell
git diff --exit-code fb7e471a9877fb68cc0cb26feadc04615882a1b2..HEAD -- src/KingmakerGunslinger assets blueprints Info.json Directory.Build.props
```

If the product-path comparison is not empty, stop treating the old ZIP as the
current candidate. Review and qualify the descendant; do not hide the mismatch.

## Next action: one exact shared-controls replay

The build manifest deliberately binds the ZIP to commit `fb7e471a`, while the
product branch now has documentation-only descendants. The runtime wrapper
correctly rejects a commit mismatch. Do not edit the manifest or rebuild only to
stamp the reporting commits.

With a clean worktree and no competing source owner, temporarily detach the
primary checkout at the tested product commit, run the exact artifact, and then
return to the product branch. This changes no ref and rewrites no history:

```powershell
$reportingBranch = 'codex/expanded-summoning-sprint24-deinonychus-tyrannosaurus'
$testedSource = 'fb7e471a9877fb68cc0cb26feadc04615882a1b2'
$package = (Resolve-Path 'artifacts/local-runtime/0.0.153/KingmakerGunslinger-0.0.153-local-runtime.zip').Path

git status --porcelain
git switch --detach $testedSource

.\scripts\compatibility\Invoke-KingmakerCompatibilityProfile.ps1 `
  -ProfileId gunslinger-only `
  -Scenario disposable-expanded-summoning-sprint24-native-review `
  -Parameters @{ reviewPhase = 'shared-controls' } `
  -RuntimeTimeoutSeconds 900 `
  -ReuseInstalledArtifact `
  -PackagePath $package `
  -Confirm:$false

git switch $reportingBranch
```

Do not use `-AllowDirtyGit`. Do not commit while detached. If the guarded call
throws, still verify its transaction/restoration and return to the reporting
branch after evidence inspection. A PASS requires every row, especially
`deinonychus-genuine-ai-fallback`, `sprint24-execution`, fixture restoration and
profile restoration. Record the exact run ID, evidence path, result SHA-256,
loaded commit/package/DLL/MVID, duration and transaction restore.

If it fails, inspect raw evidence and change strategy before another launch.
Never rerun an unchanged failed postcondition. Make any correction on the
product branch, run the named focused gate, commit/push, then run one new full
Sprint gate for the coherent changed candidate before replaying the affected
native phase.

## Hidden-candidate persistence still required

After shared-controls passes on the exact artifact, remain at its exact source
while running the targeted installed-stack persistence cycles. These are the
only authorized save writes and use `KMG_AUTOMATION_WORKING`; never substitute
the baseline save or a stripped profile.

Define the exact arguments in PowerShell:

```powershell
$persist = @{
  SaveName = 'KMG_AUTOMATION_WORKING'
  PackagePath = $package
  ExpectedVersion = '0.0.153'
  PersistenceScope = 'theropods'
  TimeoutSeconds = 900
  Confirm = $false
}
```

Cycle A proves enabled prepare, cold load/cleanup and fresh absence:

```powershell
.\scripts\Invoke-Sprint22InstalledStack.ps1 -Scenario working-save-expanded-summoning-prepare @persist
.\scripts\Invoke-Sprint22InstalledStack.ps1 -Scenario working-save-expanded-summoning-verify-cleanup @persist
.\scripts\Invoke-Sprint22InstalledStack.ps1 -Scenario working-save-expanded-summoning-verify-absent @persist
```

Cycle B proves module-OFF cold load/cleanup, then fresh absence after re-enable:

```powershell
.\scripts\Invoke-Sprint22InstalledStack.ps1 -Scenario working-save-expanded-summoning-prepare @persist
.\scripts\Invoke-Sprint22InstalledStack.ps1 -Scenario working-save-expanded-summoning-verify-cleanup -ExpandedSummoningDisabled @persist
.\scripts\Invoke-Sprint22InstalledStack.ps1 -Scenario working-save-expanded-summoning-verify-absent @persist
```

Each call must use a fresh guarded process and restore the actual installed
eleven-mod stack and game settings exactly. The evidence must distinguish the
accepted reset of an active mouth hold from preservation of already-swallowed
native state and Bleeding Critical state. It must prove one Tyrannosaurus
2d8+11 packet, one native 2d6 bleed tick, healing removal, exact receipt-owned
cleanup, no orphan/timer, and final absence. If a stage fails after prepare,
preserve the failure and use the sanctioned cleanup scenario; do not edit the
save.

After both cycles, return to the product branch and record the results in the
three active Sprint 24 records. Keep the keys suppressed until every hidden
requirement above is green. Commit and push the coherent evidence checkpoint.

## Publication candidate after hidden qualification

Only after shared-controls and both persistence cycles pass:

1. Fetch/review current master and integrate any valid descendant normally.
   Recheck the next available stable patch.
2. Remove **only** `deinonychus` and `tyrannosaurus` together from
   `SummonVisibilityCatalog.SuppressedCreatureKeys`. A half-published pair is
   invalid. Do not change GUIDs, keys, template children or ordering.
3. Centralize the final counts: 107 project creatures; Monster 95 creatures /
   543 placements; Nature's Ally 95 / 550; 1,093 generated placements; 29
   wrappers; 1,122 visible choices; zero suppressed logical placements.
4. Use Sprint 23 publication commit `01238de87aeb1cf67a6766481374eda5ba2eef41`
   as a pattern for affected tests and records, not as authority for current
   totals. Do not scatter new pins through unrelated history.
5. Update the complete version/release/validator surface to the next free patch
   and add its release notes before the final freeze. Expected is `0.0.154`.
6. The qualified-candidate publisher currently has explicit Sprint 22 and
   Sprint 23 charge-rounding contracts but no Sprint 24 selector. Extend that
   existing path narrowly, with tests, to select this branch/PR/ledger and
   require the Sprint 24 artifact, proof, restoration, zero-waiver and
   1093/29/1122 roster contract. Do not create an alternate publisher.
7. Run named focused production compilation/tests for Sprint 24 and the changed
   publication/count surface. Commit and push the coherent public candidate.
8. Run exactly one fresh `.\scripts\Invoke-KmgGate.ps1 -Level Sprint` for
   the frozen public, versioned source. Capture commit, source fingerprint,
   package, DLL, MVID, asset identities and complete-suite result.

Suppression removal and version/publisher changes create a new artifact. Do not
reuse the hidden ZIP as the public/release ZIP.

## Affected public native gates

Use the exact final public ZIP across fresh restored transactions. Follow the
current wrapper signatures rather than copying stale flags. At minimum, mirror
the released Sprint 23 public pattern with fresh Sprint 24 evidence:

- `observe-expanded-summoning-inventory` under
  `gunslinger-high-risk-combined`, proving all current totals and ownership;
- `disposable-expanded-summoning-sprint24-native-review` with
  `reviewPhase = 'rtwp-foundation'` under the same high-risk profile, proving
  every one of the 18 now-public roots via real parent spell paths, quantities,
  slots, duration and Monster templates;
- any affected module/attachment check required by the actual publication diff.

Retain the exact hidden mechanics/art/persistence evidence only after an
explicit unchanged-dependency audit. Do not replay all old roots or unrelated
matrices merely because two keys became visible. Conversely, rerun any observer
or shared semantic actually changed during publication.

Do not place documentation-only commits between the final product freeze and
its native runs unless using the exact tested-source procedure above. Avoid
creating another commit/package mismatch unnecessarily.

## Merge, exact promotion and stable release

When the final public artifact and mandatory gates are green:

1. Update the qualification ledger to `QUALIFIED`/release-eligible with exact
   proof files and no unresolved mandatory behavior.
2. Mark PR #45 ready and merge it normally into current master.
3. Promote the exact qualified public ZIP through the existing
   `Publish-Release.ps1 -ReuseQualifiedCandidate` path after the Sprint 24
   contract is implemented. Do not rebuild, restamp or rename untested bytes.
4. Publish the next free stable annotated tag and GitHub release, expected
   `v0.0.154`, non-draft, non-prerelease and Latest, with exactly the installable
   UMM ZIP, `SHA256SUMS.txt` and `release-manifest.json`.
5. Independently download all three assets and verify GitHub digests, ZIP
   strict validation, DLL SHA/MVID, manifest provenance, annotated tag object
   and peeled merge target.
6. Restore and audit the actual human installation, settings, working fixture,
   protected saves, references and sound bank. Require no owned process, lease,
   lock, staging or permanent development deployment.
7. Create `codex/expanded-summoning-sprint24-release-records` from current
   released master. Make only documentation/JSON/link/consistency updates, open
   and merge its documentation-only PR, and do not build another product.
8. Confirm clean synchronized worktrees and that master says `RELEASED/STOP`.
   Then stop before Sprint 25.

Authentication, entitlement, cloud conflict, protected-save ambiguity,
conflicting live ownership, proprietary/legal issues, unsafe restoration, or a
mandatory behavior with no safe scoped solution are genuine owner blockers.
Ordinary engineering, observer fixes, donor/art volume and remaining tests are
not blockers.

## Important file map

- `src/KingmakerGunslinger/RuntimeTesting/RuntimeTestRunner.ExpandedSummoningSprint24Review.cs`
  — final hidden mechanics/visual/shared-controls observer.
- `src/KingmakerGunslinger/RuntimeTesting/RuntimeTestRunner.ExpandedSummoningSprint24Persistence.cs`
  — held/swallowed/bleed persistence evidence.
- `src/KingmakerGunslinger/Summoning/TheropodRulesPolicy.cs` — centralized
  rules, append and hidden/public totals.
- `src/KingmakerGunslinger/Summoning/SummonVisibilityCatalog.cs` — the only two
  publication suppression keys.
- `tests/KingmakerGunslinger.DomainTests/ExpandedSummoningSprint24Tests.cs` —
  closed Sprint 24 contracts.
- `scripts/compatibility/Invoke-KingmakerCompatibilityProfile.ps1` — fresh
  isolated profile/native-review wrapper.
- `scripts/Invoke-Sprint22InstalledStack.ps1` — exact current installed-stack
  persistence wrapper despite its historical filename.
- `scripts/Publish-QualifiedCandidate.ps1`, `scripts/Publish-Release.ps1` and
  `scripts/QualifiedRelease.Common.ps1` — exact-candidate publication path to
  extend, not replace.
- `docs/SPRINT24-COVERAGE.md` and
  `planning/SPRINT24-THEROPOD-DINOSAURS-LEDGER.json` — current proof authority.

## Remaining checklist

- [ ] Fresh intake and ownership checks.
- [ ] Exact `fb7e471a` shared-controls native replay PASS.
- [ ] Enabled targeted persistence prepare/cleanup/absence PASS.
- [ ] Module-OFF targeted persistence cleanup and final absence PASS.
- [ ] Hidden qualification records committed and pushed.
- [ ] Current master/version/tag rechecked and integrated.
- [ ] Only the two Sprint 24 suppression keys removed; totals regenerated.
- [ ] Next stable version and existing publisher contract updated.
- [ ] Final public Sprint gate PASS on one frozen artifact.
- [ ] Fresh public high-risk inventory and all 18 public routes PASS.
- [ ] PR #45 ready and normally merged.
- [ ] Exact qualified bytes promoted and stable release independently verified.
- [ ] Human machine and authorized fixture restored exactly.
- [ ] Documentation-only final-records PR merged.
- [ ] Master synchronized at `RELEASED/STOP`.
- [ ] Sprint 25 was not started.
