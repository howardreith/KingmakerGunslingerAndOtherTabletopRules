# Expanded Summoning alternate sources - mission state

Status: **corrections delivered; focused native validation partly complete.**
Runtime is checked only for the scenarios in the transaction table below
(details in the doc's "Native coverage"); the cases under "Remaining" are
not checked.

- This file is the resumable state for the alternate-summoning-sources mission
  only. It does not replace the Sprint 25 worker's resume narrative or any
  older handoff; older handoffs remain history.
- The design, route audit and maintainers' note are in
  [docs/ALTERNATE-SUMMONING-SOURCES.md](../docs/ALTERNATE-SUMMONING-SOURCES.md).
  The development-candidate route is in
  [docs/DEVELOPMENT-CANDIDATES.md](../docs/DEVELOPMENT-CANDIDATES.md).

## Intake

First intake 2026-10-16 (first checkpoint); refreshed at 13:13Z before the
first guarded launch and checked before every later one.

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
| this commit | Documentation and state |

## Candidates

Every launch went through `scripts/compatibility/Invoke-KingmakerCompatibilityProfile.ps1
-ReuseInstalledArtifact -PackagePath <development package> -DevelopmentPurpose
alternate-summoning-native`: Steam App ID 640820, the profile's own runtime
lease, intake snapshot, staged Mods tree, deployment receipt and exact
restoration. Every scenario is save-free (no save was selected, loaded or
written) and exits automatically. Evidence directories are under
`C:\Dev\KingmakerGunslingerLab\runtime-evidence` and are not committed.

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
| **`651dde310e63`** (final) | `7e76626cd5846444b7b23a11ef37c16955b5916fc340b0ea43de8f1ecdd4f677` | `1e5d53fe6f611912dc3d42afe811bf9e52dc1ea972e0d3c93f38c0a9935dfb0f` | `e49cb11e-6d1c-4770-b2dd-80f9fcd55530` | 577 / 2,633, 0 failures |

The final candidate's receipt (SHA-256 `6959f0cc9f0ca1f63df9910d18a2ac6deb8ce8dfbb84756b7f4179a19118d80e`) binds the 14 private reference hashes, the development validation (baseline `607da683b1f6`, 6,895 identities, 3,746 appended) and the selected-test log. Each build took about 70 s. Candidates and evidence stay under the gitignored `artifacts/` and the lab's `runtime-evidence`; nothing was installed permanently.

## Runtime transactions and restoration receipts

Every transaction is a compatibility-profile transaction under
`C:\Dev\KingmakerGunslingerLab\compatibility-state`. Its own record says
`Restored` with `restorationVerified=true`, and after each one the Mods-tree
fingerprint read back `9736b4f7c906b6f9` with the original KMG DLL
`00ef6757...966fd`, no Kingmaker process and no lock. No save was selected,
loaded or written, and no human save was touched.

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
| Repository wrapper, development mode | `scripts/validate-repository.ps1 -DevelopmentPurpose alternate-summoning-native` | PASS; 28 s |
| Repository wrapper, release mode | `scripts/validate-repository.ps1` | FAIL at the v0.0.156 release validator, as before; 1 s |
| Gate and orchestration contracts | `Test-ExplicitCandidateReuse.ps1`, `Test-KmgGate.ps1` (includes `Test-DevelopmentGate.ps1`) | 7 / 7; PASS (development gate 8 / 8) |
| Runtime preflight | `scripts/Test-RuntimeScenarioPreflight.ps1` | 588 checks PASS; 14 s |
| Pre-existing failures | `Test-RuntimeDeployment.ps1`, `Test-CompatibilityCandidateReuse.ps1` | both FAIL identically on unchanged `master` `607da683b1` ("legacy-deployment-pins-complete-artifact-identity"; "Nested build/deployment/scenario count mismatch"). Baseline, not caused here; neither runs in the wrapper |

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

## Remaining (not checked in game)

Runtime is not checked for any of these; they are not claimed.

1. **Real UI selection.** No native action-bar popup was opened or clicked
   and no hotbar slot was used: the casts used the same converted-ability
   data the action bar builds, through `UnitUseAbility`. A popup-click
   driver exists for spellbook spells (Magic Circle) but not for class
   abilities, and the repository records that an earlier attempt to
   install a slot through `settings.SetSlot` hung the game. Needs a
   party-member caster selected in the UI and an ability-group popup
   driver.
2. **Hotbar reuse and saved references.** No saved shortcut to an adapter
   child was reloaded: the reduced CotW profile cannot hydrate the
   established working save, and no save-backed fixture exists for a
   shortcut. What was observed instead: a blocked family's identities stay
   registered as unavailable placeholders; with the module OFF every
   identity registers and nothing is exposed; and an unexposed registered
   child is refused through its root (the preserved Master Summoner tier-I
   cast). That is what the real loader resolves a saved reference to, but a
   reload of a real shortcut was not performed.
3. **Turn-based sample.** Every cast ran synchronously outside combat, which
   is the same command path in either mode; no turn-based combat round was
   exercised.
4. **Master Summoner with an eidolon.** The disposable Master Summoner took
   no lesser eidolon, so the one-summon-while-the-eidolon-lives branch and
   the eidolon's return were not exercised; only the no-eidolon coexistence
   branch was.
5. **Recent-creature cleanup** (for example Tyrannosaurus swallow/grapple
   reset on reload) through an adapter route.
6. **Higher tiers.** One tier per family was cast (tier I or III; Occultist
   also at level 20). Middle and last menu positions were not each cast.

## Next actions

1. Owner review of PR #52 (draft). No merge, tag, release or install from
   this mission.
2. If the remaining native cases above are wanted before integration: a
   party-member caster selected in the UI with an ability-group popup driver
   (real click and hotbar reuse), a save-backed shortcut fixture for the
   module-OFF and blocked-family reload cases, a Master Summoner with a
   lesser eidolon, and one turn-based combat round. Each needs its own
   guarded fixture; none should reuse the working save without its own
   prepare/verify contract.
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
