# Expanded Summoning alternate sources - mission state

Status: **released as v0.0.157 (2026-10-16).** Before release: **targeted verification corrections implemented; native
command-fixture coverage passed; real-popup and natural-RTWP cases passed on
the working save; player-facing validation partially complete.** Runtime is
checked only for the scenarios in the tables below (details and the evidence
matrix in the doc's "Targeted verification pass"); the cases under
"Remaining" are not checked.

- This file is the resumable state for the alternate-summoning-sources mission
  only. It does not replace the Sprint 25 worker's resume narrative or any
  older handoff; older handoffs remain history.
- The design, route audit and maintainers' note are in
  [docs/ALTERNATE-SUMMONING-SOURCES.md](../docs/ALTERNATE-SUMMONING-SOURCES.md).
  The development-candidate route is in
  [docs/DEVELOPMENT-CANDIDATES.md](../docs/DEVELOPMENT-CANDIDATES.md).

## Intake

First intake 2026-10-16 (first checkpoint); refreshed at 13:13Z before the
first guarded launch and checked before every later one. Third-pass intake
at 14:54Z, rechecked at 16:00Z: PR #52 head `e9db3c18f2` (draft, no reviewer,
comment or review); `origin/master` `968b35495c` unchanged; PR #49 head
`a295dd8902` (draft; it moved during the pass; not imported or touched); the
installed Mods tree and settings as in the table below.

| Item | Observed value |
|---|---|
| Machine / lab root | Windows 10 Pro 10.0.19045; `C:/Dev/KingmakerGunslingerLab` |
| Main checkout | `repo/KingmakerGunslinger`, `master` at `607da683b1`, clean, not touched |
| Mission worktree / branch | `worktrees/expanded-summoning-alternate-sources`, `codex/expanded-summoning-alternate-sources` (pushed only through the approved wrapper) |
| Base | `origin/master` `968b35495c11d87a7e00b96f3d33bea3ce05929b`, unchanged at 13:39Z |
| Latest stable release | `v0.0.156` -> `607da683b1f6fd545dab90fd6f4121d155573142` |
| PR #52 | draft, no reviewer, no comments or reviews |
| PR #49 (Sprint 25) | draft; head `c75b3bf89f063b640508c3c8337ea3e78bbc22af` at 13:39Z; not imported, not touched |
| Installed game | Kingmaker 2.1.7b, Assembly-CSharp SHA-256 `3b6450ffec440e296e586f71c711b195aed144b28d53e1cbb29406d18fef5afb` |
| Installed Mods (human install) | 16 mods. KMG `Info.json` `0.0.140`, DLL SHA-256 `00ef67570d30e4ff9c54a2ad2027393660436d5724bcee2bb99e4c5718e966fd`; CotW `1.14.4c-2.1`, DLL `4ebf8e1ed3e66ffed72ea33ea325595629423dacd5bffa23e3c9109144b26915`; `FeatureModules.json` present. Names-and-sizes fingerprint of the Mods tree `9736b4f7c906b6f9` |
| Runtime state | no Kingmaker process; no `compatibility.lock`; newest earlier lease `Completed` |
| Private references | the default `private/extracted-references` bundle lacks `UnityEngine.PhysicsModule.dll` (required since Oct 6); the candidate builds used the documented `private/refresh-2026-10-15` bundle (archive SHA-256 `99a63aef727ff2111b70bcca31fbacc6a0cbc52e4f07d6bdb5426f955af94610`, matching its `.sha256`), which Build-Local verified file by file against the installed game |
| Reference mod root | the profile's default `C:\Dev\KingmakerGunslingerLab\examples` did not exist. It was created with a byte-exact copy of the installed `Mods\CallOfTheWild` (266 files, identical SHA-256 manifest, CotW `1.14.4c-2.1`). Lab-local, not committed. No other lab's copy was used |

## Delivered

First checkpoint (`9fa2605184`, `02dedaaaaf`):
- Seven adapter families with exact host identities (see the doc): native
  Monster Tactician; with CotW Summoner (Twinned shares it), Master
  Summoner, Occultist Arcanist, Naturalist, Feral Hunter, Preservationist.
- 3,746 adapter identities (UUIDv5, append-only) and the manifest-tool
  extension; an availability gate on every adapter child; menu-layout
  recognition for exact adapter roots; per-family transactional
  publication with refuse-on-foreign-change rollback; first-update
  lifecycle.

Corrections mission:

| Commit | What |
|---|---|
| `c77f971529` | Explicit per-family template contracts and a signature model; resolver rejects unknown execution templates, keeps unknown menu options; placeholders for blocked families; signature-based observer; blocked-family drill; save-free contract fixture; 6 contract domain tests |
| `4c5b721bdc` | Development-candidate route ([docs/DEVELOPMENT-CANDIDATES.md](../docs/DEVELOPMENT-CANDIDATES.md)); icon-catalog repin for the extended ledger and the `BlueprintBootstrap.IsFailed` accessor |
| `f05256c3a1` | Save-free native-area player-path fixture for Master Summoner and Occultist |
| `1a1864e870`, `67428817fb`, `7ed7dfcfbb`, `651dde310e` | Player-path fixture extended to every distinct family contract, and four fixture corrections found by guarded runs (see transactions) |
| `e9db3c18f2` | Documentation and state |

Third pass ("targeted verification corrections and player-path closure"):

| Commit | What |
|---|---|
| `3e2bf5caf7` | Composed-action contract (`AlternateSummoningComposition`) in the observer, with negative controls in the contract fixture and 6 domain tests; assisted-execution labels on every player-path cast; Master Summoner eidolon pool cases, Occultist lower-tier group, Preservationist extract III -> SNA IV |
| `c7216602b1` | Eidolon-return fixture correction (the engine's life tick after Summon Eidolon); new working-save scenario `disposable-alternate-summoning-natural` |
| `3c17b769b6` | Natural scenario at Master Summoner 13 with the tier VII Tyrannosaurus. Its message also claims the observer comment change, which is actually in the next commit |
| `b9c56e3da9` | Observer documentation names the supported contracts (comment only) |
| `3cc8bd3499` | Documentation, state and evidence matrix |
| `a320442acd` | Natural verdict: terminal state, pinned quantity bounds, explicit Tyrannosaurus cleanup, with domain tests |
| this commit | Evidence reconciliation and the corrected owner checklist |

## Candidates

Two launch routes were used, both Steam App ID 640820 with a runtime lease
and automatic exit:
- **Compatibility-profile transactions** (every profile scenario):
  `scripts/compatibility/Invoke-KingmakerCompatibilityProfile.ps1
  -ReuseInstalledArtifact -PackagePath <development package>
  -DevelopmentPurpose alternate-summoning-native`, with the profile's intake
  snapshot, staged Mods tree, deployment receipt and restoration. These
  scenarios are save-free: no save was selected, loaded or written.
- **Live development route** (only `disposable-alternate-summoning-natural`,
  third pass): `Deploy-Local.ps1 -DevelopmentPurpose` (backup first),
  `Invoke-KingmakerRuntimeTest.ps1 -ReuseInstalledArtifact -SaveName
  KMG_AUTOMATION_WORKING`, then `Restore-Live-Mod.ps1`. It loads the
  disposable working save read-only; no save file was written (the runner's
  write guard observed none). No human save was selected or touched.

Evidence directories are under `C:\Dev\KingmakerGunslingerLab\runtime-evidence`
and are not committed.

Candidates (all `0.0.156`, `candidateKind=development`, purpose
`alternate-summoning-native`, built by
`Build-Local.ps1 -ReferenceBundleDir <refresh-2026-10-15 bundle> -DevelopmentPurpose alternate-summoning-native -DevelopmentTestFilter '<focused filter + icon;elemental-character-trait.>'`):

| Commit | Package SHA-256 | DLL SHA-256 | DLL MVID | Selected tests |
|---|---|---|---|---|
| `4c5b721bdcdf` | `0142cb7b172867fc03971c93c9c37ce9ab517e8b58d179c42ffd89e3c2d4fb41` | `9e80d8acf578ab240eaa1d40046e4120eb09df1685cad62c746ca47dafd73081` | `fb6a7334-4130-4e66-8aca-ce6c9720c932` | 576 / 2,632, 0 failures |
| `f05256c3a17d` | `4fb6fc374410a3f1748712123017bb159dbc9c1cf12cef1fbdd363abac5948fc` | `eb762a0e2af8ccfa51be07a2c5bdf84b6e4af8d69a674909614e4349db567330` | `a07fc3e8-14ed-4d7e-bb51-df7d69c311e7` | 577 / 2,633, 0 failures |
| `1a1864e8702b` | `1355cf1972084aaa4f065283dfc05cb33d47bb9f5c23fdea1d7a165d0abc4b28` | `e71ae70537dadeff98897c0d28cd7e6343e6f9c4770663e95f82706947de8f50` | `a1fe32fa-1cc5-4346-8b1b-ac7530dea6a6` | 577 / 2,633, 0 failures |
| `67428817fb8d` | `9004ecf54e153d244de0850e8baaa9ca38f0ed371d29ddf62c62e52bc2755c19` | `c2bd18f8975160c900b74536f683cc061f72d2d3d2fcc3153481183a0747ffcc` | `62ee00e8-0f5d-4616-b3c8-5a62b08a98a4` | 577 / 2,633, 0 failures |
| `7ed7dfcfbb63` | `f684ca01cb450ba5db07a171bb299393b9834755eb8430e17b69797898cc2082` | `e11e8da23f103aa1c5abfd0955c543fb89476a964550dfd2fb9c9d76b718018f` | `467eb160-ac76-41a5-824e-80b919fe9c1b` | 577 / 2,633, 0 failures |
| `651dde310e63` (final of the corrections mission) | `7e76626cd5846444b7b23a11ef37c16955b5916fc340b0ea43de8f1ecdd4f677` | `1e5d53fe6f611912dc3d42afe811bf9e52dc1ea972e0d3c93f38c0a9935dfb0f` | `e49cb11e-6d1c-4770-b2dd-80f9fcd55530` | 577 / 2,633, 0 failures |
| `3e2bf5caf729` | `9915d67ef1e42922bcc726db3b3d24b8846b0bd942b29f8881d3f2e67dcfdb8e` | `d96792ced60f8728d1203e85ae187b7aed513805bffec65470d0e3c3dbe5d6ea` | `83279163-284d-4ad8-92a2-d40092295f7b` | 583 / 2,639, 0 failures |
| `c7216602b103` | `14b46f66877342d3a5129561a818dbf141e50510e41ce2cafb3d0963d07eed47` | `633d4f83692987ac857c68048be1e49d374a23619a7a0503f5f3b216f9336f88` | `be269bac-a383-4599-aa72-911dfd43f06c` | 583 / 2,639, 0 failures |
| `b9c56e3da9a9` | `72a6c5c64e7143d01e011328e000b3145b8489cb4f2f8ebee7ea45e5aa4d3193` | `d7b578871f839e0051d82a757c96b148d9185fc2c28023abbd3873e78c58d1b9` | `7d375849-9c13-482d-a12d-cc1dfb0877f3` | 583 / 2,639, 0 failures |
| **`a320442acdb2`** (final development candidate) | `2b398a7c3cf2dc5bcad2d17c0681fc3c54d6273a93be5c14932e81bf08454a02` | `d9921be87c7bc2626200ccf8927a6413ae2403657beda7cab63af14af83b8395` | `177607b6-ff79-4906-b16f-b73d586d6398` | 590 / 2,646, 0 failures |

Receipt SHA-256: `3e2bf5caf729` `896cb264ecb4975e6f953e937bbc34d4dd66938920d63d0a73b040f061239447`, `c7216602b103` `2f2b741f8d4f3190916be9f9161470c1195dc44ffaa7a1ee17101eaee30b516a`, `b9c56e3da9a9` `f08b3f6e48555de8b98541b84420d3116d5cafda7ce9621186017c1184ed3565`, `a320442acdb2` `8f09e23465d332ad101b55734c3596a1c7b5741fcbeaa47df2e2956a3e355a07`. The `651dde310e63` receipt (SHA-256 `6959f0cc9f0ca1f63df9910d18a2ac6deb8ce8dfbb84756b7f4179a19118d80e`) binds the 14 private reference hashes, the development validation (baseline `607da683b1f6`, 6,895 identities, 3,746 appended) and the selected-test log. Each build took about 70 s. Candidates and evidence stay under the gitignored `artifacts/` and the lab's `runtime-evidence`; nothing was installed permanently.

## Runtime transactions and restoration receipts

Every transaction in this table is a compatibility-profile transaction under
`C:\Dev\KingmakerGunslingerLab\compatibility-state`. Its `transaction.json`
holds the original Mods manifest (every file's SHA-256, length and timestamp;
1,057 files in the third pass) and says `Restored` with
`restorationVerified=true`: the restored tree was compared with that manifest
exactly. That comparison is the byte-level evidence. After each one I also
read the names-and-sizes fingerprint `9736b4f7c906b6f9` (file paths and sizes
only, a quick check that does not by itself prove unchanged bytes) and the KMG
DLL `00ef6757...966fd`, and saw no Kingmaker process and no lock. These
scenarios are save-free.

| Transaction | Profile | Candidate | Scenarios -> result | Created -> restored (UTC) |
|---|---|---|---|---|
| `compat-20261016T131613Z-18e0ec7a97ff` | `gunslinger-call-of-the-wild` | `4c5b721bdcdf` | observer PASS 9/9; blocked-family PASS 9/9; contract fixture PASS 12/12 | 13:16:26 -> 13:22:10 |
| `compat-20261016T132341Z-9c60ef607676` | `gunslinger-only` | `4c5b721bdcdf` | observer PASS 9/9 | 13:23:54 -> 13:25:46 |
| `compat-20261016T133028Z-90c37c3d7047` | `gunslinger-call-of-the-wild` | `f05256c3a17d` | player paths FAIL 4/5: fixture required the preserved tier-I cast in the menu (it is correctly withheld) | 13:30:42 -> 13:33:16 |
| `compat-20261016T133727Z-86faab1748c3` | `gunslinger-call-of-the-wild` | `1a1864e8702b` | player paths FAIL 11/15: no resource spent (fixture caster marked a cheater, which `AbilityResourceLogic.Spend` skips); Monster Tactician archetype lookup too narrow | 13:37:40 -> 13:40:13 |
| `compat-20261016T134406Z-6e8b22147249` | `gunslinger-call-of-the-wild` | `67428817fb8d` | player paths FAIL 18/20: base Summoner hit the fixture's null-archetype assumption; Preservationist extract not found | 13:44:19 -> 13:46:52 |
| `compat-20261016T134920Z-dc6f2b183039` | `gunslinger-call-of-the-wild` | `7ed7dfcfbb63` | player paths FAIL 19/20: the fixture's level-up visits skipped the native spellbook step, so the Alchemist had no formula book | 13:49:33 -> 13:52:06 |
| `compat-20261016T135403Z-9c3962df18c5` | `gunslinger-call-of-the-wild` | `651dde310e63` | player paths PASS 20/20 | 13:54:16 -> 13:56:49 |
| `compat-20261016T135711Z-dfc613566a70` | `gunslinger-call-of-the-wild`, Expanded Summoning OFF through the profile's feature-module parameters | `651dde310e63` | observer PASS 9/9 (every family `RegisteredModuleOff`, nothing exposed); feature-module settings PASS 47/47 | 13:57:24 -> 14:01:23 |
| `compat-20261016T152216Z-2309b39e72d3` | `gunslinger-call-of-the-wild` | `3e2bf5caf729` | observer PASS 9/9; contract fixture PASS 17/17; player paths FAIL 27/28 (`player-ms-eidolon-return`: the fixture omitted the life tick after Summon Eidolon) | 15:22:29 -> 15:28:07 |
| `compat-20261016T153610Z-e8eb945012ed` | `gunslinger-call-of-the-wild` | `c7216602b103` | observer PASS 9/9; blocked family PASS 9/9; contract fixture PASS 17/17; player paths PASS 28/28 | 15:36:23 -> 15:43:38 |
| `compat-20261016T155231Z-ebe9626d778a` | `gunslinger-only` | `b9c56e3da9a9` | observer PASS 9/9 | 15:52:44 -> 15:54:33 |
| `compat-20261016T155434Z-ea5fc4a94142` | `gunslinger-call-of-the-wild`, Expanded Summoning OFF | `b9c56e3da9a9` | observer PASS 9/9; feature-module settings PASS 47/47 | 15:54:48 -> 15:58:38 |

Live working-save runs (third pass). The reduced CotW profile cannot load the
working save, so these used the purpose-bound live route: `Deploy-Local.ps1
-DevelopmentPurpose alternate-summoning-native` (backup first, settings
preserved), `Invoke-KingmakerRuntimeTest.ps1 -Scenario
disposable-alternate-summoning-natural -SaveName KMG_AUTOMATION_WORKING
-ReuseInstalledArtifact -DevelopmentPurpose ...` (Steam App ID 640820,
runtime lease, automatic exit), then `Restore-Live-Mod.ps1` from that run's
backup, which writes and verifies only the `KingmakerGunslinger` folder.
`KMG_AUTOMATION_WORKING` was loaded read-only: no save write was observed.
Restoration evidence per run:
- `c7216602b103` and `b9c56e3da9a9`: the live `KingmakerGunslinger` folder was
  byte-compared (`diff -rq`) with its pre-run backup: identical, 254 files
  (`Info.json` 0.0.140, DLL `00ef6757...966fd`, `FeatureModules.json`
  `6e24b278...`). The rest of the Mods tree, which this route does not write,
  was checked only by the names-and-sizes fingerprint `9736b4f7c906b6f9`.
- `a320442acdb2`: a full content manifest of the Mods tree (1,057 files, SHA-256
  each) was captured before deployment and after restoration and is identical
  (manifest digest `eae0b32e7c8c26b256ca36972d27e48c91e7b9b68a89aad87942f4aa2501a993`;
  the manifest files are kept in this session's scratch area, not committed).
No game process or lock remained after any run.

| Evidence directory | Candidate | Result | Deployment / backup | Window (UTC) |
|---|---|---|---|---|
| `20261016T1544477038010Z-disposable-alternate-summoning-natural` | `c7216602b103` | PASS 8/8 under the earlier verdict (Master Summoner 5; tiers I and III; a Dire Rat cleaned up) | `deployments/20261016T1544457766822Z`; `runtime-backups/live-mod/20261016T1544375407470Z` | 15:44:37 -> 15:46:10 |
| `20261016T1550339254110Z-disposable-alternate-summoning-natural` | `b9c56e3da9a9` | PASS 11/11 under the earlier verdict (Master Summoner 13; tiers I, III and VII Tyrannosaurus; Tyrannosaurus cleaned up) | `runtime-backups/live-mod/20261016T1550239443522Z` | 15:50:23 -> 15:51:59 |
| `20261016T1740160951203Z-disposable-alternate-summoning-natural` | `a320442acdb2` | PASS 11/11 under the strengthened verdict (terminal state, pinned quantity bounds, explicit Tyrannosaurus cleanup) | `deployments/20261016T1740133603843Z`; `runtime-backups/live-mod/20261016T1740050751130Z` | 17:40:05 -> 17:41:49 |

Five launch attempts never entered a profile and changed nothing (verified
by the unchanged fingerprint and the absence of a new transaction): two
quoting errors in the local launch wrapper (a Bash backtick, and a runner
file with an unterminated string), a scenario list passed through `-File`
as one string (refused by the profile's own `ValidateSet`), the missing
reference root (refused during profile resolution), and one runner
invocation with a malformed candidate commit (refused at the package read).

## Gates and checks (corrections, 2026-10-16)

All on this worktree. Durations are wall-clock.

| Check | Command | Result |
|---|---|---|
| Focused gate, template contracts | `scripts/Invoke-KmgGate.ps1 -Level Focused -Filter 'alternate-summoning;expanded-summoning.;blueprints.ledger;favored-class.leaf-manifest-tail;better-vendors.catalog-manifest-append;elemental-races.identities;bodyguard-blueprints.identities'` at `c77f971529` | compile PASS; 480 / 480; about 13 s |
| Focused gate, with icon-catalog readers | same filter plus `icon;elemental-character-trait.` | compile PASS; 576 / 576 of 2,632; 6 s |
| Focused gate, player paths | `-Filter 'alternate-summoning'` at `651dde310e` | compile PASS; 18 / 18; 8 s |
| Development validator fixtures | `python tools/test_development_candidate_validator.py` | 13 / 13 |
| Development validator, live | `python tools/validate_development_candidate.py --purpose alternate-summoning-native` | PASS: 6,895 identities, 3,746 appended to the released 3,149 |
| Release validator, unchanged | `python tools/validate_repository.py --root .` | still FAIL: "The firearm patch must not change blueprint identities" (expected; this branch is not a release) |
| Development-candidate contract | `scripts/Test-DevelopmentCandidate.ps1` (Windows PowerShell 5.1 and pwsh 7) | 63 / 63 each; about 2 s |
| Repository wrapper, development mode | `scripts/validate-repository.ps1 -DevelopmentPurpose alternate-summoning-native` | PASS; 28 s. It also runs nested inside every development `Build-Local` (log: "Development-candidate repository validation passed ... not a release qualification"): the development-candidate validator replaces the release validator and the named selection replaces the complete suite; every other wrapper fixture is unchanged |
| Repository wrapper, release mode | `scripts/validate-repository.ps1` | FAIL at the v0.0.156 release validator, as before; 1 s |
| Gate and orchestration contracts | `Test-ExplicitCandidateReuse.ps1`, `Test-KmgGate.ps1` (includes `Test-DevelopmentGate.ps1`) | 7 / 7; PASS (development gate 8 / 8) |
| Runtime preflight | `scripts/Test-RuntimeScenarioPreflight.ps1` | 588 checks PASS; 14 s |
| Pre-existing failures | `Test-RuntimeDeployment.ps1`, `Test-CompatibilityCandidateReuse.ps1` | both FAIL identically on unchanged `master` `607da683b1` ("legacy-deployment-pins-complete-artifact-identity"; "Nested build/deployment/scenario count mismatch"). Baseline, not caused here; neither runs in the wrapper |

Third-pass gates and checks are in the doc's "Commands, durations and
results (third pass)", including the wrapper reconciliation. One more
pre-existing failure was found there:
`Test-MagicCircleRuntimeRequest.ps1` fails identically on unchanged `master`
("'Get-KmgRepositoryRoot' is not recognized").

## The earlier failed complete-suite execution (unclassified)

- **What happened.** On 2026-10-16, between about 06:41:15Z and 06:45Z, the
  domain test executable was run once without a filter, on a binary built
  from the uncommitted working tree before the fifteen ledger-shape tests
  were updated (base `968b35495c`). It failed.
- **Output.** Not retained. `test-domain.ps1` writes no result file, that
  invocation's standard output was discarded, and the runner writes no file
  of its own. `artifacts/gate-timings.jsonl` holds only Focused-level entries
  for that morning; a direct executable run is not recorded there. The
  session transcript holds no completion line for a run of all 2,627 cases
  registered then. No other copy was found in the worktree, the scratch area
  or the temporary directory.
- **What is known.** A filtered run of 513 cases on the same binary showed
  fifteen ledger-shape failures (`favored-class.leaf-manifest-tail`,
  `better-vendors.catalog-manifest-append`, `elemental-races.identities`,
  `expanded-summoning.sprint3/4/5/8-ledger-icons`,
  `expanded-summoning.sprint16-ledger-identities`,
  `expanded-summoning.sprint17-snake-append-only-ledger`,
  `expanded-summoning.sprint20-templated-monster-only`,
  `expanded-summoning.sprint20-ledger-append`,
  `expanded-summoning.sprint23-ledger-append`,
  `expanded-summoning.sprint14-identities`,
  `expanded-summoning.sprint14-catalog-matches-ledger`,
  `bodyguard-blueprints.identities`), all fixed in `9fa2605184`, plus two
  `bodyguard-runtime.*` environment-precondition failures (a gitignored IL
  dump missing from a fresh worktree; they pass with it present).
- **What is not known.** Its total count, pass/fail counts and any other
  failing case. It is recorded as an unclassified failed execution, not as
  evidence either way, and it was not repeated: the complete suite remains
  a Sprint-boundary gate under the validation policy.

## v0.0.157 release candidate gates

Owner authorization: `docs/BUILD-AND-RELEASE.md`, "Current 0.0.157
publication authorization". Release preparation `5a72066602`, fixture fix
`3dc23e9c7a`.

**Sprint gate** (`scripts/Invoke-KmgGate.ps1 -Level Sprint -ReferenceBundleDir
<refresh-2026-10-15>`, delegating to `Build-Local.ps1` release mode) on
`3dc23e9c7a`: PASS in 103.3 s. Repository wrapper in release mode (the
active-version validator dispatched `validate_alternate_summoning_release157.py`:
6,895 identities, 3,746 appended; its 16 corruption fixtures and every other
wrapper fixture), complete domain suite 2,646 / 2,646, exact-reference
build, deterministic package (373 files) and strict package validation. No
deployment.

A first Sprint gate attempt on `5a72066602` failed after 20 s, before any
build: `Test-DevelopmentCandidate.ps1` copies the real `Build-Local`, which
pins the active version, into a fixture whose `Info.json` was hard-coded to
0.0.156. Fixed in `3dc23e9c7a` (the fixture reads the active version); the
contract then passed 63 / 63 on Windows PowerShell 5.1 and PowerShell 7.

**Release candidate** `3dc23e9c7a2e` (`artifacts/local-runtime/0.0.157`):
package `82d6baef743717c81147b8ac394af2ffe9b2356389915f43850a676d0375e9c0`,
DLL `6ee1ab757a3798067782e66ff09c886e279c3b119bac321bbd1a9e654b25f31c`, MVID
`2f4d3a56-e82c-4c3e-bdea-2a67201efad5`, source state
`4dac09d7c9d97d7c2705665b5c3ede71f2842116617a7b7af2f7157430cd6c8b`.

**Native evidence on exactly those bytes** (release candidate, no
development purpose):

| Transaction / run | Profile | Scenarios -> result | Window (UTC) |
|---|---|---|---|
| `compat-20261016T175820Z-d3e9e558ff53` | `gunslinger-call-of-the-wild` | mod-load smoke 3/3; observer 9/9; blocked family 9/9; contract fixture 17/17; player paths 28/28; canonical inventory (`observe-expanded-summoning-inventory`) 50/50 | 17:58:32 -> 18:10:30 |
| `compat-20261016T181031Z-d7158a197c79` | `gunslinger-only` | observer 9/9; canonical inventory 50/50 | 18:10:41 -> 18:14:09 |
| `compat-20261016T181410Z-3edefc57dc90` | `gunslinger-call-of-the-wild`, Expanded Summoning OFF | observer 9/9; feature-module settings 47/47 | 18:14:21 -> 18:18:03 |
| `20261016T1819161817592Z-disposable-alternate-summoning-natural` (deployment `20261016T1819144825462Z`, backup `runtime-backups/live-mod/20261016T1819070958436Z`) | live route, read-only `KMG_AUTOMATION_WORKING` | natural scenario 11/11 under the strengthened verdict (every wait `completed`; Dire Rat 1 in [1,1], Vipers 3 in [2,5], Tyrannosaurus 1 in [1,1]; Tyrannosaurus cleanup by explicit lifecycle-buff removal followed by native dismemberment and pool cleanup); no save write | 18:18:45 -> 18:20:40 |

Each profile transaction is `Restored` with `restorationVerified=true`
(per-file SHA-256 manifest, 1,057 files). The full Mods content manifest
read `eae0b32e7c8c26b256ca36972d27e48c91e7b9b68a89aad87942f4aa2501a993`
before the release runs and again after the live run restored the mod: identical. No save was written; no human save was
touched.

## Remaining (not checked in game)

Runtime is not checked for any of these; they are not claimed. The doc's
"Owner checklist" gives the minimal manual check for each.

1. **Hotbar binding and reuse** - not automated (the `settings.SetSlot` path
   hung an earlier guarded run; not repeated or forced). Owner steps 1-2.
2. **Saved-reference cold load** (normal, Expanded Summoning OFF, restored) -
   needs a save-writing shortcut orchestrator on a disposable save. Owner
   steps 2-4.
3. **Saved-reference load with a blocked family** - BLOCKED / UNVERIFIED,
   maintainer only (case M1).
4. **Turn-based round** - no genuine turn-based combat round. Owner step 5.
5. **Scroll-required choice** - no menu needed scrolling at the natural runs'
   resolution. Owner step 6.
6. **Grapple or swallow reset on reload** - BLOCKED, needs a maintainer
   fixture (case M2): the engine refuses to save in combat.

Closed by the third pass (see the evidence matrix): Master Summoner with a
real lesser eidolon (assisted), real popup selection and natural RTWP casts
(tiers I, III and VII) under the strengthened verdict, the recent
Tyrannosaurus's cleanup by explicit lifecycle-buff removal followed by native
dismemberment and pool cleanup, the Occultist lower-tier group cost and the
Preservationist extract III -> SNA IV mapping (assisted).

## Next actions

1. Done: PR #52 merged as `8726ec737a`; v0.0.157 published and Latest
   (package `d2e6489277b56fb8a2b1a949fde6ddec8ddb1c40e1fcda145b7a2296db0d32a5`,
   DLL `b5ac181083843268075503481f8a82589a4efb7496c563616ca631b6ffc67562`; see
   `AUTONOMOUS-RESUME.md`). The owner checklist and maintainer cases remain
   open. Sprint 25 renumbers to 0.0.158 or later; the development-candidate
   validator is re-pinned before the next development candidate.
2. The owner checklist in the doc covers the remaining manual cases. If they
   should be automated instead: a save-writing shortcut orchestrator on a
   disposable save (cold load normal / OFF / restored), a turn-based round
   with an enemy in a disposable fixture, and a low-resolution natural run for
   the scroll-required choice. Hotbar binding should not be automated through
   `settings.SetSlot` without first understanding the earlier hang.
3. **Integration with Sprint 25 (PR #49).** Both branches append to
   `blueprints/blueprints.json` and update the same ledger-offset tests.
   Whichever merges second regenerates with
   `python tools/expanded_summoning_manifest.py --allocate --activate --emit-roster`,
   keeps the earlier branch's rows in place (adapter GUIDs are deterministic,
   so re-appending yields the same values), adds the other block's size to
   the ledger-offset sums and repins the icon catalog's registry hash.
   Brachiosaurus, SNA-only and withheld, gains SNA adapter identities only.
4. A release that ships this work needs its own version, release validator
   and record; development candidates never qualify a release.
5. If `origin/master` advances, record the new head here. Do not merge
   automatically.
