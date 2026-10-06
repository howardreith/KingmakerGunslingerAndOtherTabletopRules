# Lunge and Aerial Observer foundations handoff — 2026-10-05

Finite DATA mission closed after native qualification and exact restoration.
Lunge reached its permitted evidence-backed engine gate; Aerial Observer has a
qualified unpublished mechanics foundation. Neither option is player-acquirable.

LungePublished: false
AerialObserverPublished: false

## Repository and ownership

- Repository: `howardreith/KingmakerGunslingerAndOtherTabletopRules`.
- Origin: `https://github.com/howardreith/KingmakerGunslingerAndOtherTabletopRules.git`.
- Exact starting/source branch tip: `6a4dc1b26350c1045171b42099ef2ac5584e303d`,
  `origin/codex/elemental-race-trait-foundations-2026-10-05`.
- Dedicated branch: `codex/lunge-aerial-observer-foundations-2026-10-05`.
- Dedicated worktree:
  `C:\Dev\KingmakerGunslingerLab\worktrees\lunge-aerial-observer-foundations-2026-10-05`.
- Exact final mechanics source, locally committed and remotely pushed:
  `a0992df35a28d0049ad25b3f746f0d465ed5426a`. Its clean artifact below is the runtime authority.
- Final local/remote branch tip: the documentation-only closeout commit containing
  this handoff. The guarded push output and ignored `artifacts/mission/final-state.json`
  record its literal SHA after creation; a commit cannot contain its own SHA.
  Resolve both with `git rev-parse HEAD` and
  `git ls-remote --heads origin refs/heads/codex/lunge-aerial-observer-foundations-2026-10-05`.
  Both must be equal. This closeout changes only this non-packaged handoff and does
  not replace the qualified source/artifact identity with an untested embedded commit.
- Final tracked/untracked status is required clean; final-state receipt and final
  response attest it after this file's qualified closeout commit and wrapper push.
- No merge, rebase, reset, history rewrite, force-push, tag, version bump or PR.
  Version remains `0.0.141` with inherited informational identity unchanged.
- Protected local refs remain: master `2ce70e4e7e9d3c97ca1008ab05a341e758f5cf5a`,
  source foundation `6a4dc1b26350c1045171b42099ef2ac5584e303d`,
  content follow-up `b28b5786d10a94f3257fafa5cf02bbd50471d182`.
  This mission pushed only its dedicated branch through the required wrapper.
- Source ref was fetched explicitly with `--no-tags`, without pull, rebase, tags,
  branch rewriting or another development input. Its exact local/remote tip and
  clean source worktree were verified before the new branch/worktree was created.
- Intake found no unfinished Git operation, competing owner, Kingmaker process,
  active old lease/compatibility lock/deployment/staging/source-ownership helper.
  No cleanup mission was resumed. No subagent was used.

Checkpoint commits:

1. `bd2dddbb743a70c59d3d8c6b5b6ae4baf1f2eeb5` — exact engine/flight research.
2. `20099a6cb250eba51dc37f3f57fbb94bc53b1e79` — qualified unpublished rule-path
   foundation and guarded fixture; later strengthened for direct passive consumers.
3. `a0992df35a28d0049ad25b3f746f0d465ed5426a` — qualified event-driven actual-stat maintenance and passive,
   suppression and listener cleanup coverage.
4. The closeout commit containing this handoff — curated evidence only.

Every coherent checkpoint was pushed through
`powershell.exe -NoProfile -ExecutionPolicy Bypass -File C:/Dev/KingmakerGunslingerLab/codex-policy/Push-KingmakerGunslinger.ps1`.

## Lunge engine gate and behavior ledger

Classification: **BLOCKED-NO-SEPARABLE-SEAM**, specifically the frozen RTwP turn
and attack-attempt contract. Range and threat themselves have separate native
paths; the label is not a claim that attack-only range is inherently impossible.
Exact map and bounded alternatives are in
[the Lunge engine contract](docs/research/LUNGE-ENGINE-CONTRACT.md).

Installed `Assembly-CSharp.dll` MVID `07fa1e4d-8618-41b3-9b8d-faa17d3b26f7`,
SHA-256 `3b6450ffec440e296e586f71c711b195aed144b28d53e1cbb29406d18fef5afb`.
Complete private IL was generated with the already installed ILDASM; raw IL and
proprietary assemblies remain ignored. Reflection's six unloaded dependency types
were not treated as complete negative evidence; complete IL was inspected.

- Target faction legality: `UnitEntityData.CanAttack`.
- Selected legal range: `ItemEntityWeapon.AttackRange`,
  `UnitDescriptor.GetWeaponRange`, `CharacterStats.ReachRange` and the selected
  `AttackHandInfo.WeaponRange`; native weapon/reach-stat and corpulence are preserved.
- Candidate narrow approach/execution seam: private
  `UnitAttack.GetApproachRadius`, refreshed by `UpdateTarget`; native
  `UnitCommand.IsUnitCloseEnough` retains geometry/LOS and native pathing.
  This is a researched possible future +`5.Feet()`/1.524-metre adjustment only.
- Full attack: `TryStartNextAttack`, `OnAction`, `TriggerAttackRule` remain native.
- Threat/AoO: separate `UnitEngagementExtension.GetThreatHand/IsReach/IsEngage`,
  `UnitCombatState.AttackOfOpportunity`, and `UnitAttackOfOpportunity`.
- Turn based: native `CombatController.CurrentTurn`, `TurnController.Prepare/End`
  expose ownership and discrete boundaries.
- RTwP: Standard cooldown positive-to-zero raises a new combat round; independently,
  `UnitTicksController` ticks each unit on its six-second periodic clock.
  Idle activation at zero has no later Standard transition, while a periodic tick
  can divide an ongoing full attack. Command end is not the next turn. Mixing those
  clocks or starting a private timer invents an additional turn interpretation.
- Attempt boundary: command `Start` rejects invalid/range targets before its event;
  private `UnitCommands.Run` has distinct acceptance, merge-return and queue paths.
  No demonstrated exact combined boundary covers every canceled/invalidated accepted
  attack without a broad command/turn rewrite.
- Native `ModifierDescriptor.Penalty` and negative AC filtering support a possible
  future normal/touch/flat-footed -2 penalty, but no modifier or stacking/expiry
  gameplay result is claimed here.

No Lunge state owner, toggle, command, AC penalty, range patch, weapon/corpulence
mutation, blueprint or selection was created. No RTwP adaptation was chosen.
No TB/RTwP gameplay PASS is claimed. Six deterministic installed-contract checks
prove the documented gate, not the 48 gameplay obligations. BAB +6 construction
and its +5 rejection remain deferred behind this gate. Combat maneuvers are
excluded/unqualified because their ability/context paths are not the selected
melee-hand command seam. There is no stale Lunge state to clean up.

## Aerial Observer adaptation and mechanics

Classification: **EXACT-MECHANICAL-FLIGHT-CARRIER**, bounded to the existing
released KMG Wings of Air mechanical flight abstraction. Behavior is **ADAPTED**:
+2 Trait Perception while that exact active mechanical flight is present, not
30-feet-above-ground tabletop altitude or universal foreign-mod flight.
Exact candidate/lifecycle/stat-consumer map:
[the flight contract](docs/research/AERIAL-OBSERVER-FLIGHT-CONTRACT.md).

Accepted existing symbol `KMG.ElementalRaces.Feats.WingsOfAir.Buff`, GUID
`e116e1e0a17a4aceb001000000000019`. Constructor and runtime activation validate
canonical object identity, exact GUID and the complete existing three-component
native graph: +3 melee Dodge AC, DifficultTerrain immunity, Ground-descriptor
buff immunity. An equal name/GUID on a cloned object does not qualify. Feature
ownership without its armor-gated active buff does not qualify.

Visual wings, hover, jump/fall/FlyHeight, model height, elevated terrain, polymorph
appearance, standalone terrain immunity and standalone Ground immunity are not
flight carriers. No altitude approximation, movement/global patch or polling.
Missing exact native contracts or an altered carrier graph fail closed.

`AerialObserverPerceptionBonus` owns its own bindings, modifier and weak per-rule
replay ledger. It listens to native owner-filtered buff membership changes and
attaches one `AerialObserverFlightTransition : OwnedGameLogicComponent<UnitDescriptor>`
instance per provider/exact carrier fact through the native public `Fact.Components`
list and `GameLogicComponent.Fact` setter. Only that unit's fact-instance list is
changed; registered flight blueprint/source components remain unchanged.
Native `OnTurnOn/OnTurnOff` records activation/deactivation after the carrier's
mechanical components. Native exact-buff suppression and release use those same
callbacks. Provider removal detaches only its exact listener instances.

The provider maintains one actual +2 `ModifierDescriptor.Trait` modifier on
`SkillPerception`. It adds no modifier for other skills; inactive, absent or
suppressed flight gets none. Removal/deactivation/suppression removes the bonus
immediately; activation/release restores it before any next resolution. Perception
rule delivery reconciles once per exact rule object and never adds a second bonus.
Native descriptor rules keep duplicate providers at one effective +2, allow a
foreign +3 Trait to win, and stack distinct descriptors normally. Units are independent.
Repeated grant/remove and builder initialization create no duplicate registered identity.

Covered native paths:

- Real `RuleSkillCheck` active checks.
- Real `RuleCachedPerceptionCheck` cached detection; its native cached d20 remains native.
- `PartyPerceptionController.RollPerception` passive map-object discovery, through
  its actual RuleSkillCheck stat-resolution path.
- Native world-map preselection/discovery and fog radius raw-stat readers before
  any skill rule. Their exact pure lambdas were invoked on the disposable actor;
  no global-map tick, campaign discovery, camera, party or fog mutation was invoked.
- Other native raw-stat consumers, including camp guard sorting and description
  display, read the same maintained stat. Their separate UI flows were not automated.

Runtime assertions prove absence/grounded, real flight without foundation, visual
height-only rejection, active/cached rules, passive readers before a rule, raw-stat
cleanup, Stealth control, two independent units, lesser/greater foreign Trait,
distinct descriptor, duplicate providers, deliberate rule replay, immediate native
inactivity/reactivation/removal, actual native suppression/release, three repeated
transitions, listener detachment with the flight fact still active, provider removal,
wrong native type, cloned-carrier rejection, idempotent unregistered builders and
exact fixture cleanup. No combat/area/aura subsystem or other trait was added.

## Publication-negative ledger

| Identity/component | Status and GUID | Reachability and ordinary acquisition | Icon/localization/save visibility |
|---|---|---|---|
| Lunge | Absent; no symbol/GUID | No bootstrap, state, selection, feat/race grant or setting | No icon/localization/catalog/save identity |
| `AerialObserverPolicy`, `AerialObserverFlightContract` | Unregistered CLR code only | No grant, selection or setting | No blueprint/icon/localization/save identity |
| `AerialObserverPerceptionBonus` | Dormant CLR component; no registered GUID | Only unregistered fixture provider graph; no main-bootstrap caller | No icon consumer/localization/ordinary-save path |
| `UnpublishedAerialObserverFoundationFactory` provider | Hidden, transient, random request GUID; unregistered | Exact guarded fixture only; no ordinary race/feat/selection/setting/cheat/menu | Icon null; no visible localization; no save write/acquisition path |
| `AerialObserverFlightTransition` | Transient fact-instance CLR component; no blueprint GUID | Only provider's exact carrier facts on its own request-owned actor | No icon/localization/ordinary grant; detached on removal |
| `AerialObserverReplayProbe` and probe buff | Fixture-only, transient random request identity | Exact guarded replay measurement only | Hidden/iconless; no ordinary-save path |
| Existing Wings of Air buff | Registered immutable baseline GUID above | Existing baseline bootstrap/acquisition unchanged; not an Aerial Observer grant | Baseline icon/localization/save behavior unchanged |

No BasicFeatSelection, Fighter bonus feats, Favored Class racial_traits, alternate
racial selection, character creation, automatic race grant, ordinary player fact,
setting, icon catalog exception, waiver, README feature claim or release note was added.
No registered new identity exists. Icon validators and protected assignments were
unchanged. Only disposable actors receive temporary fixture facts; no save is written.

Prior Fiery Glare and Stoic Dignity mechanics, factories, specialized runtime file
and focused test files are byte-unchanged from the base; their 53 registered
focused cases (20 Fiery, 33 Stoic) remain green in the unfiltered suite.
Their publication remains false. Their specialized runtime scenario was not rerun.

## Validation ledger

Actual baseline: **2,093/2,093 PASS**, freshly discovered on the exact base.
Final: **2,127/2,127 PASS**, unfiltered; **34/34** focused mission checks (6 Lunge
engine-contract, 28 Aerial). Four passive/lifecycle cases strengthened the initial
30-check/2,123-test candidate. All inherited 2,093 registrations retain their exact
names/order; inherited compile entries retain their order. Only the two current
shared count pins changed to 2,127; no historical record or package count changed.

Commands, run from the dedicated worktree:

```powershell
powershell.exe -NoProfile -ExecutionPolicy Bypass -File scripts/Invoke-KmgGate.ps1 -Level Focused -Filter 'lunge-engine;aerial-foundation'
powershell.exe -NoProfile -ExecutionPolicy Bypass -File scripts/Invoke-KmgGate.ps1 -Level Sprint
powershell.exe -NoProfile -ExecutionPolicy Bypass -File scripts/validate-repository.ps1
git diff --check
git diff --cached --check
python artifacts/mission/Audit-Mission.py
```

The Sprint pipeline runs repository validation (static/icon/manifest and current
validation contracts), complete clean Release domain suite, provenance-checked
private-reference clean Release build, deterministic package generation, supply
icon checks, build-output validation and strict standalone UMM validation. No game
launch is part of that gate. Exact final source log: `artifacts/mission/final-source-gate.log`.
Focused log: `artifacts/mission/passive-focused.log`; intake baseline log:
`artifacts/mission/baseline-domain.log`. Parser checks passed for both changed
PowerShell files; Python AST validation passed for the changed count-pin validator.

`Test-RuntimeScenarioPreflight.ps1` passed **488 checks** in the normal-user context.
Six direct `New-KmgRuntimeRequest` schema probes at the actual active version passed:
valid, missing/wrong save, no automatic exit, extra parameter, wrong version.
They did not write a request or launch the game. Closed scope remains exactly
`KMG_AUTOMATION_WORKING` with explicit qualified working-save stage timeouts.

Final artifact (no dirty-Git override):

| Field | Exact value |
|---|---|
| Embedded source commit | `a0992df35a28d0049ad25b3f746f0d465ed5426a` |
| Version | `0.0.141` |
| Source fingerprint | `d2d4641000a0ff39d1fbc73997db6007eb8a3cab5bbd25588d632fd66745d9a2` |
| DLL SHA-256 | `edfc34fd21a97e1f0742355ab31b665310b973c9f2d65fae97b6bb8604a55501` |
| DLL MVID | `e9f9e98e-27ff-4b84-a36f-808f7c3d45bf` |
| ZIP SHA-256 | `67504ce3fc110118d5bb555d04a70e8c98ba831fb49b7565221a13cb7b351386` |
| ZIP | `artifacts/local-runtime/0.0.141/KingmakerGunslinger-0.0.141-local-runtime.zip` |

Build-local manifest, deployed DLL, loaded-module identity, MVID and source commit
were checked against this exact artifact on every final run. Strict standalone
validation passed for both standard and runtime ZIPs. The existing historical
package/informational naming is unchanged; no Expanded Summoning qualification is implied.

## Runtime evidence and restoration

All game launches used guarded `Invoke-KingmakerRuntimeTest.ps1`, Steam App ID
**640820**, normal-user guard, exact owned process correlation and automatic exit.
No mouse/keyboard automation, UI navigation, screenshot or OCR proof. The existing
closed working-save loader supplies a qualified loaded native area for the two
request-local actors; no raw save access or save write. KMG_AUTOMATION_BASELINE was untouched.

For each transaction: `Enter-KmgRuntimeLease` **before any live observation/snapshot**,
full current file SHA/length and directory membership snapshot, `Deploy-Local.ps1`
with exact package/lease, native fresh launches, `Restore-Live-Mod.ps1` from that
transaction's exact backup, full before/after equality, `Exit-KmgRuntimeLease`.
No fixed historical baseline or file count was assumed.

Applicable runtime call (existing guarded transaction supplies the exact owned
lease, package and deployment receipt):

```powershell
.\scripts\Invoke-KingmakerRuntimeTest.ps1 `
  -Scenario observe-unpublished-aerial-observer-foundation `
  -ExpectedVersion 0.0.141 -SaveName KMG_AUTOMATION_WORKING `
  -TimeoutSeconds 900 -CompletionTimeoutSeconds 600 `
  -ExitAfterCompletion:$true -ReuseInstalledArtifact:$true `
  -PackagePath $package -DeploymentManifestPath $deployment `
  -RuntimeLease $ownedLease -Confirm:$false
```

Run twice from independent fresh processes on the same final artifact, followed
by the same exact-artifact `working-save-smoke` call. Evidence root:
`C:\Dev\KingmakerGunslingerLab\runtime-evidence`.
Each run directory is named with its timestamp prefix and scenario; its request,
result, loaded-build identity, orchestration and fixture JSON preserve full details.
Final assertion/process ledger:

| Transaction | Run ID | Native process PID | Assertions |
|---|---|---|---|
| aerial-final-clean-1 | 20261006T0131059418798Z-55d682d8432b48d4a99cb13e1d9b7431 | 34872 | 29/29 PASS |
| aerial-final-clean-1 | 20261006T0132053769251Z-a5a728592e074c4eb6dee1681a895abc | 22224 | 29/29 PASS |
| aerial-final-clean-1 | 20261006T0133024629170Z-5189e5a121d34b74b04c5bc997e94cee | 35456 | 11/11 PASS |

Final lease: `C:\Dev\KingmakerGunslingerLab\compatibility-state\runtime-20261006T013040Z-828bd335364a43aa8f150db3cb1373a0\runtime-lease.json` — **Completed**.
Final deployment receipt: `C:\Dev\KingmakerGunslingerLab\runtime-evidence\deployments\20261006T0131043916744Z\deployment.json`.
Final exact backup: `C:\Dev\KingmakerGunslingerLab\runtime-backups\live-mod\20261006T0130584480626Z`.
Final snapshot: **254 files / 7 directories**, measured after lease acquisition;
all relative paths, lengths, SHA-256 values and directory membership match afterward.
Private before/after receipts are `artifacts/mission/aerial-final-clean-1-live-before.json`
and `aerial-final-clean-1-live-after.json`. Original units, party, area effects,
positions, buffs, library order/index, game time, pause and RNG are restored by the
fixture. Native assertions confirm no residual actor/fact/listener/modifier.
No save-writing API was observed on any run; no raw save was inspected/manipulated.
The closeout machine check found zero Kingmaker processes, zero mission helpers,
no compatibility lock, and all 267 runtime lease records Completed. No active
owned deployment/staging/helper remains. The post-push check is recorded in the final-state receipt.

Earlier exact candidate/checkpoint runs also passed and restored. These are history,
not substitutions for the final artifact's two fresh PASS runs above:

| Transaction | Run ID | Native process PID | Assertions |
|---|---|---|---|
| aerial-candidate-1 | 20261006T0053439504482Z-759465b475324f60868514f45d13c8ca | 17068 | 25/25 PASS |
| aerial-candidate-1 | 20261006T0054493844758Z-63e9878534b245e9a92e01e905efe654 | 20732 | 25/25 PASS |
| aerial-candidate-1 | 20261006T0055467619943Z-c104b82a699d4f71a99bdde1ae6bec3f | 31396 | 11/11 PASS |
| aerial-clean-1 | 20261006T0104143663498Z-c9b876b80fc248c997c78e24d0a91691 | 13504 | 25/25 PASS |
| aerial-clean-1 | 20261006T0105141386657Z-879da57495c04956944fde9cfa74ba20 | 7804 | 25/25 PASS |
| aerial-clean-1 | 20261006T0106150392429Z-9411e43ad7c746a3bdfa3fc6a1512cd3 | 27516 | 11/11 PASS |
| aerial-passive-candidate-1 | 20261006T0122568536084Z-ddf7768fb2734c7793014d3d129345c5 | 2584 | 29/29 PASS |
| aerial-passive-candidate-1 | 20261006T0123594321010Z-b09e454b802c43d19ac4060a21d4d6e7 | 32260 | 29/29 PASS |
| aerial-passive-candidate-1 | 20261006T0124554587069Z-5749206f92cd44d294a65b4bd5daf228 | 22004 | 11/11 PASS |

Every transaction is restored with lease Completed. Initial candidate/clean source
used the 25-assertion rule-only graph; passive candidates/final source use 29.
Their private build-local manifests record distinct DLL/ZIP/MVID/fingerprints.

## Meaningful failures, fixes and scope audits

- Initial publication-negative test referenced nonexistent FeatureModuleCatalog;
  corrected this mission's own test to actual FeatureModuleConfiguration.
- First rule-only candidate passed active/cached native checks, but full stat-consumer
  review found pre-rule passive reads. Replaced rule-only temporary modification
  with the exact instance-local native flight listener/maintained stat; added four
  deterministic checks and native passive/suppression/listener assertions. The
  final artifact qualifies the repaired behavior, not the earlier limited proof.
- Listener's first compile used UnitFactComponentDelegate, absent in this installed
  engine (CS0246). Corrected to native/repository-proven
  OwnedGameLogicComponent<UnitDescriptor>; full suite/build/package/native gates reran.
- Unchanged legacy Test-RuntimeRequest.ps1 names obsolete 0.0.87 and is rejected by
  the current exact-version guard. No unrelated test/guard was weakened; six direct
  active-version constructor probes plus 488 preflight checks and real guarded
  request acceptance supply current request evidence. Private helper import/stage
  timeout mistakes were corrected without modifying repository safety architecture.
- No runtime FAIL or restoration failure occurred. Ambiguous evidence was not accepted.
- Diff/staged checks, exact changed/untracked allowlist, generated/binary/proprietary/
  private/credential audits, new machine-path audit, inherited source/registration/
  count-pin preservation, protected refs and ancestry/no-merge checks passed.
  Final staged/document/clean audits run again for the closeout file.
- No new binary, package, proprietary IL/assembly, raw evidence, save, credential,
  machine configuration or generated artifact is committed. Curated evidence only.
- No Expanded Summoning branch/worktree/PR/source/asset/fixture/scenario was developed,
  altered or qualified. Inherited tests/source/registrations remain immutable. The
  allowed current shared count pins alone include this mission's new tests. No
  Expanded Summoning specialised runtime or PR operation was run.
- No other backlog item, icon authoring, publication, merchant/firearm work,
  integration, release, unrelated cleanup or refactoring was started.

Complete changed-file ledger (19 files across the mission):

- `docs/research/AERIAL-OBSERVER-FLIGHT-CONTRACT.md`
- `docs/research/LUNGE-ENGINE-CONTRACT.md`
- `scripts/RuntimeAutomation.Common.ps1`
- `scripts/Test-RuntimeScenarioPreflight.ps1`
- `src/KingmakerGunslinger/ElementalRaces/AerialObserverMechanics.cs`
- `src/KingmakerGunslinger/ElementalRaces/AerialObserverPolicy.cs`
- `src/KingmakerGunslinger/ElementalRaces/UnpublishedAerialObserverFoundationFactory.cs`
- `src/KingmakerGunslinger/KingmakerGunslinger.csproj`
- `src/KingmakerGunslinger/RuntimeTesting/RuntimeTestRequest.cs`
- `src/KingmakerGunslinger/RuntimeTesting/RuntimeTestRunner.UnpublishedAerialObserverFoundation.cs`
- `src/KingmakerGunslinger/RuntimeTesting/RuntimeTestRunner.cs`
- `src/KingmakerGunslinger/RuntimeTesting/RuntimeTestScenarioCatalog.cs`
- `tests/KingmakerGunslinger.DomainTests/AerialObserverFoundationTests.cs`
- `tests/KingmakerGunslinger.DomainTests/KingmakerGunslinger.DomainTests.csproj`
- `tests/KingmakerGunslinger.DomainTests/LungeEngineContractTests.cs`
- `tests/KingmakerGunslinger.DomainTests/Program.cs`
- `tools/validate_expanded_summoning_phase2a141.py`
- `validation/static-validation.json`
- `CODEX-LUNGE-AERIAL-OBSERVER-FOUNDATIONS-HANDOFF-2026-10-05.md`

## Finite future work and stopping condition

Lunge is blocked before mechanics/publication: an owner must supply a faithful
RTwP/attempt-boundary contract or qualified native route before that gate can reopen.
No approximation or additional mission is authorized here. Aerial remains bounded
to the exact documented carrier; broadening to another flight carrier needs exact
independent evidence.

For a later explicitly authorized publication mission only:

1. Original icon authoring.
2. Owner visual approval.
3. Visible feat/trait blueprint graph after its mechanics gate is satisfied.
4. BAB/race prerequisites and selection publication.
5. Combined player-facing runtime and manual acceptance.

No human acceptance/visual result was supplied or inferred. DATA stops after the
qualified documentation closeout, guarded push and clean/restored final audit.
