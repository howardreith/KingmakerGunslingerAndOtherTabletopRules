# Whiteout disposable-process continuation handoff - 2026-10-05

Outcome: **QUALIFIED UNPUBLISHED FOUNDATION**. Executed 2026-10-06 UTC.

WhiteoutPublished: false
NonPartySceneReferenceRestorationRequired: false
DisposableNoSaveProcessIsolation: true

Two consecutive fresh-process foundation runs passed 65/65 assertions each,
followed by exact-artifact working-save smoke 11/11. All used the same clean
source commit, ZIP, DLL, MVID and source fingerprint. Zero save-writing APIs
were observed. Both areas' weather and all owned fixtures were restored before
leaving their areas; each process exited automatically. The exact acquired
254-file live-mod snapshot, including directory inventory, was restored
byte-for-byte and the shared runtime lease completed.

## Repository state

- Repository: `howardreith/KingmakerGunslingerAndOtherTabletopRules`.
- Origin: `https://github.com/howardreith/KingmakerGunslingerAndOtherTabletopRules.git`.
- Exact starting SHA: `5c59150a748023d8bc7195df9bfd63ca4a2079b2`.
- Authorized source: `origin/codex/whiteout-unpublished-foundation-2026-10-05`,
  verified at that exact SHA before worktree creation and at runtime closeout.
  Its worktree was clean. The prior worktree was read only for intake/evidence.
- Sole owned branch: `codex/whiteout-disposable-process-foundation-2026-10-05`.
- Sole owned worktree: `C:/Dev/KingmakerGunslingerLab/worktrees/whiteout-disposable-process-foundation-2026-10-05`.
- Weather checkpoint: `b97a0a47ca454009e892d9849d5c51aca56940c7`.
- Exact clean qualified mechanics commit, pushed local/remote:
  `db3725d7eb665731c3b8c238217cd39f2d0a65b8`.
- Final local/remote tip is the documentation-only closeout commit containing
  this handoff. Its full SHA and clean equality are recorded after commit/push
  in the session closeout and ignored `artifacts/mission/whiteout-continuation-closeout.json`.
  A committed file cannot contain its own literal Git SHA. No compiled, test,
  script, manifest or asset input changed after the qualified mechanics commit;
  the final handoff does not change the qualified artifact's embedded identity.
- All commits use the guarded push wrapper. No merge, rebase, reset, history
  rewrite, force push, tag, release, version bump or PR. No master/main,
  source, content, elemental, Lunge/Aerial or Expanded Summoning movement was
  caused by this mission. No other development line was claimed or edited.
- Intake found zero competing helpers, Kingmaker processes, active leases or
  compatibility locks; 271 historical leases were Completed. Only this agent
  owned the new worktree. Final process/lease/lock audits are recorded at closure.

## Revised fixture decision

The prior [blocker handoff](CODEX-WHITEOUT-UNPUBLISHED-FOUNDATION-HANDOFF-2026-10-05.md)
remains accurate historical evidence: native cross-area unloading disposes
non-party scene entities and returning deserializes replacements. The owner
accepted this native lifecycle only within an automatically exiting, no-save
process. The continuation never returns to the mansion, compares old NPC
references after unloading, resurrects facts or reinserts disposed objects.
**No in-memory reference restoration of replaced scene objects is claimed.**
Process exit discards those native in-memory scene changes. Exact per-area
weather, owned-state cleanup and acquired live-installation restoration remain
mandatory. The guarded loader alone loads `KMG_AUTOMATION_WORKING`. No raw save
is copied, parsed, renamed, edited, replaced or deleted; baseline is untouched.
Native area-cache activity is separate from save-writing APIs and is not
misreported as object-reference restoration.

## Weather ledger

- Native assembly MVID: `07fa1e4d-8618-41b3-9b8d-faa17d3b26f7`;
  SHA-256 `3b6450ffec440e296e586f71c711b195aed144b28d53e1cbb29406d18fef5afb`.
- Exact existing singleton: `WeatherSystemBehaviour.Instance`; public
  `WeatherType`, `RainIntensity: float`, `SnowIntensity: float`.
- `Player.Weather.ActualWeather` reads those fields; no saved-schedule write.
  Rain thresholds `RainIntensitites.Values = [0, 0.15, 0.4, 0.75, 1]`;
  Snow thresholds `SnowIntensitites.Values = [0.5, 0.75, 0.9, 99, 999]`.
  First i with intensity below Values[i]-0.01 yields i-1; otherwise Storm.
  Native Light uses Values[1]: Rain 0.15, Snow 0.75, other intensity zero.
- Existing paused controller: `Game.GetController<WeatherController>(true)`.
  Exact native `OnUpdateWeatherSystem(true)` dispatches
  `IWeatherChangeHandler.OnWeatherChange`; `Tick`, visual Update/Init,
  campaign-time advance, replacement singleton and transition coroutines are
  never invoked between first weather write and synchronous finally rollback.
- Each scope captures/restores the three visual fields, exact controller
  `m_SeasonData: WeatherRoot.SeasonalData` reference and `m_Overridden: bool`.
  Original-state native notification runs before restoring controller fields.
  Singleton, ActualWeather, saved CurrentWeather/NextWeatherChange, controller
  fields, ordered listeners and existing same-area facts must match afterward.
- Idle `SubscriptionManager<IGlobalSubscriber>.m_Listeners` and exact typed
  `SubscribersList.List` inspection admits only the known SoundState, party
  weather parts with null m_LastBuff, conditional weather components whose
  Clear/Light branch is no-op with target buff absent, and exact owned probes
  and providers. Unknown/unsafe listeners fail before writes. Foreign native
  handlers are never removed or suppressed.
- Mansion area/part `2849fdde28fe50f4d935bf2cf3405051`: actual indoor true,
  original Normal/Clear. Oleg area/part `ead426a6c23d39548a670ee515d77df4`:
  actual outdoor false, original visual Rain/ActualWeather Clear.
- Indoor evidence requires actual `LocalMapArea.GetClosest(position).AreaPart`
  and agreement with `LocalMapArea.IsIndoor(position)`. Unknown/contradictory
  identity fails closed. Actual Rain/Light and Snow/Light indoors stayed
  Whiteout-inactive; indoor real attacks used zero Whiteout rolls. The same
  exact native precipitation outdoors activated the actual provider. Clear
  deactivated it, and visual Rain with ActualWeather Clear stayed inactive.
- Sequence: guarded mansion load; baseline, Rain, repeated Rain, Snow, Clear;
  mansion weather rollback and all owned fixture removal; one native
  `LoadArea(..., AutoSaveMode.None, false, null)` to Oleg; outdoor baseline,
  Rain/combat/repeated Rain/Snow/Clear; Oleg weather rollback and owned removal;
  automatic exit. Eight explicit weather mutations, ten native notifications,
  two exact weather restorations per complete run.
- Party membership/character identity, inventory item identities/counts,
  money, GameTime, saved weather reference and schedule were checked before
  transition and before exit. No quest/kingdom progression API was deliberately
  invoked. No save-writing API was observed. Same-area existing units, facts,
  positions, combat state, areas, party, blueprint registry and listeners were
  checked before scene replacement; no prior-area references were retained.

## Attack and behavior ledger

- Exact patched method: private instance
  `RuleAttackRoll.TryOvercomeTargetConcealmentAndMissChance(): bool`, no arguments,
  149 native IL bytes. `WhiteoutAttackStagePatch.Prepare` checks declaring type,
  signature/body/MVID, required native fields/properties, weather/map contracts
  and unchanged `SeekingExactItemResolver.IsAuthorized(ItemEntityWeapon)`.
  Contract mismatch skips the patch and makes provider state inactive.
- Native `OnTrigger` calls this stage once before AC/hit resolution and later
  mirror-image/parry/damage systems. Its stronger MissChance branch and
  RuleConcealmentCheck remain native. Every native false is preserved with zero
  extra rolls. Native true may become false solely on Whiteout d100 1-10;
  11-100 continues the stage, which does not promise an overall attack hit.
- Frozen activation: **ADAPTED - OUTDOORS ONLY**; exact provider, visual Rain
  or Snow, ActualWeather Light/Moderate/Heavy/Storm, known non-indoor point,
  valid contracts and module. Normal/Clear, magical fog alone, visual fog,
  waterfall VFX, unknown maps and foreign lookalikes have no activation path.
- Native Partial 20% used the exact installed Blur control
  `dd3ad347240624d46a11a092b4dd4674`; no Heavy/Storm weather was forced.
  Native Partial failure short-circuited. Partial success plus forced
  Whiteout 10 missed, and 11 continued. Complete 100x100 enumeration remains
  28%: 1-(0.80*0.90), never additive 30%. No fake concealment tier was created.
- Native IgnoreConcealment and exact authorized Last Word Seeking bypass
  Whiteout while preserving current native failure. Equal names/copied GUIDs/
  foreign markers did not bypass. The existing Seeking implementation is
  byte-for-byte unchanged.
- Real melee, natural/Bite, native Longbow, mod firearm and native canonical
  RayWeapon/RangedTouch rules reached this seam. A transient faction clone
  with native Neutral=true avoids friendly touch AutoHit without modifying
  registered factions or preexisting units. AutoHit/AutoMiss paths that skip
  this stage, nonattack saves/damage and unproven maneuvers are excluded;
  no new spell delivery or targeting/legality coverage is invented.
- `WhiteoutWeatherProvider` has per-fact/per-owner nonserialized state. Native
  fact lifecycle subscribes/unsubscribes its `IWeatherChangeHandler` and
  `ISceneHandler`; activation, weather changes and area load reconcile, unload
  and removal clear. Every attack revalidates exact current state. Repeated
  events are idempotent; two owners use independent native component instances.
  No polling, global unit dictionary or unrelated-unit scan.
- Weak exact-rule cache:
  `ConditionalWeakTable<RuleAttackRoll, WhiteoutNativeStageDecision>`. It caches
  only Whiteout applicability/roll, never a native concealment result or unit
  reference. Current native false and current ineligibility always prevail.
  Replay used one extra roll; duplicate/cross-thread pure replay is locked.
  Exceptions/invalid RNG preserve native success and cannot retry that rule.
- Production uses `RulebookEvent.Dice.D100.Value`; actual unforced native rolls
  were observed. Guarded diagnostics require the exact request, completed
  no-write working-save guard, thread and registered attack. Another rule with
  the same attacker/target/weapon tuple could not consume its queued roll.
  Every cleanup clears queued/native concealment forces and owned cache keys.
- Each complete runtime run has 30 observed real attacks and 32 stage calls
  including two replay calls. All existing effects remain; no cleanse,
  suppression, immunity, range/threat/AC/weapon/RNG mutation by the adapter.
  All owned removal attempts run even after a prior cleanup error. The actual
  cleanup verified zero errors, no owned actors/facts/providers/listeners,
  no forced state and no owned cached decision.

## Publication-negative ledger

| Identity/component | Status, GUID and reachability | Publication / save visibility |
|---|---|---|
| WhiteoutPolicy, WhiteoutFoundationPolicy, WhiteoutNativeStageDecision | Unregistered policy/state classes, no blueprint GUID | No acquisition, icon, localization or save identity |
| WhiteoutWeatherProvider | Compiled component, no registered identity; exact per-fact blueprint backlink | No bootstrap grant, selection, race/feat grant, setting or icon consumer |
| WhiteoutAttackStagePatch / WhiteoutAttackRuntime | Exact dormant compiled patch and weak rule storage; ordinary Harmony initialization only | No effect without exact provider; no published marker or saved unit dictionary |
| UnpublishedWhiteoutFoundationFactory provider buff | Hidden, unregistered, random request GUID; only closed runtime-fixture caller | No icon/localization/registry/catalog entry or ordinary player acquisition; transient facts only on disposable actors; no save write |
| WhiteoutGuardedDiagnostics / site/weather fixture/probe | Guarded request/thread-local diagnostics and owned synchronous fixtures | No ordinary menu/setting/cheat or acquisition path; all references/forces/subscribers cleaned before exit |
| Disposable unit prototypes and native-control items | Request-owned unregistered clones/native item instances; no party/inventory grant | Existing control art only; no authored/remapped icon; actors and item runtime state removed |
| Neutral faction clone | Unregistered random request GUID, native Neutral flag | No registered faction change or player grant; destroyed after owned actors |
| Foreign Whiteout / Seeking negative controls | Unregistered request clones; intentionally copied GUID/name for identity rejection | No catalog, localization or icon assignment; cannot authorize protection/bypass; destroyed in cleanup |

No stable player-facing GUID, feature blueprint, localization, icon, waiver,
OwnedIconAssignments exception, selector, racial grant, feat grant or new setting
exists. Neither ordinary initialization nor ordinary play calls the provider
builder. Runtime-only facts exist in memory during the fixture and are never
written to a save; no ordinary-play save-visible acquisition route exists.

## Exact clean artifact and validation

- Semantic version unchanged: `0.0.141`; inherited informational/package naming
  and 289-file inventory unchanged. No release claim.
- Exact source/artifact commit: `db3725d7eb665731c3b8c238217cd39f2d0a65b8`.
- DLL SHA-256: `366c518d0b252ec4eb7335746587b4b2c4250eb5da5fa6c75de8f440a6d1d004`.
- DLL MVID: `569ce324-53f5-4c36-9406-a16e983ac229`.
- ZIP SHA-256: `9e1e320da34daa12e04a5581b0dd1e6bc4e02f36327e2d829a4019a71bccc7e0`.
- Source fingerprint: `7420a0bdb981b4c9912105374a9a3829efe5b8404e4f5b4c3826519ad0c2ee41`.
- ZIP: ignored `artifacts/local-runtime/0.0.141/KingmakerGunslinger-0.0.141-local-runtime.zip`.
  Independently repackaging the same staging tree reproduced the ZIP bytes.
- Actual intake: 2,142 unfiltered registered tests PASS. Final: **2,188/2,188**
  PASS (46 additions: 6 isolation-contract checks and 40 foundation checks).
- Whiteout focused: **96** (35 retained policy checks, 15 retained native
  contracts, 6 isolation, 40 foundation). Prior Fiery **20**, Stoic **33**,
  Aerial **28** remained green: **177/177** combined focused and all within the
  final unfiltered run. No prior foundation mechanics or specialized runtime
  scenario changed. The older publication-discovery test now permits only the
  three exact unpublished foundation files with explicit negative guards;
  all existing icon/blueprint/manifest validators remain intact.

Commands/checks used on this branch:

1. Changed PowerShell files parsed with
   `[Management.Automation.Language.Parser]::ParseFile`; zero errors. Changed
   Python count-validator source parsed with `ast.parse`; zero syntax errors.
2. `powershell.exe -NoProfile -ExecutionPolicy Bypass -File scripts/Invoke-KmgGate.ps1 -Level Focused -Filter 'whiteout-native-foundation;whiteout-continuation;whiteout-policy;whiteout-native-contract;fiery-foundation;stoic-foundation;aerial-foundation'`: 177 PASS.
3. `powershell.exe -NoProfile -ExecutionPolicy Bypass -File scripts/Invoke-KmgGate.ps1 -Level Sprint`: repository validation, complete unfiltered domain suite,
   clean exact-reference Release build, static/icon/manifest checks,
   deterministic package and strict standalone validation PASS. Clean final
   log: ignored `artifacts/mission/foundation-clean-final-gate.log`.
4. `python tools/create_deterministic_package.py --source artifacts/staging/install/KingmakerGunslinger --output artifacts/mission/foundation-clean-determinism.zip --expected-file-count 289`: byte-identical SHA-256 PASS.
5. Normal-user `scripts/Test-RuntimeScenarioPreflight.ps1`: **496 PASS**;
   guarded metadata/request refusal checks. Actual guarded requests also passed
   request-schema, allowed-scenario, save-name, artifact/version and hook stages.
6. Guarded `scripts/Invoke-KingmakerRuntimeTest.ps1` with
   `-Scenario observe-unpublished-whiteout-foundation -ExpectedVersion 0.0.141 -SaveName KMG_AUTOMATION_WORKING -ExitAfterCompletion:$true -ReuseInstalledArtifact:$true -PackagePath <exact ZIP> -DeploymentManifestPath <owned receipt> -RuntimeLease <owned lease> -Confirm:$false`:
   two consecutive fresh-process PASS runs; then same exact options with
   `-Scenario working-save-smoke`: PASS. No AllowDirtyGit in final qualification.
7. Scoped `Deploy-Local.ps1` and `Restore-Live-Mod.ps1` under the acquired lease;
   full file path/length/SHA-256 and directory snapshots equal before/after.
8. `git diff --check`, `git diff --cached --check`, tracked/untracked scope,
   generated/binary/proprietary/private/credential/secret and machine-local
   code-path audits PASS. Inherited compile/test entries retain their prefix
   and order; only current shared count pins changed 2142 to 2188. Historical
   qualification records and inherited package/test registrations remain intact.
9. `git merge-base --is-ancestor <exact base> HEAD` and
   `git rev-list --min-parents=2 <exact base>..HEAD`: ancestry PASS, zero merges;
   no unfinished merge/rebase/cherry-pick/revert/bisect/sequencer state.
10. Documentation closeout uses repository validation and the same staged/scope
    audits, followed by guarded wrapper push and final clean local/remote audit.

## Runtime evidence and restoration

| Scenario | Run ID | Fresh PID | Assertions |
|---|---|---:|---|
| observe-unpublished-whiteout-foundation | 20261006T0457457360242Z-d654335a8d3b40679cedb190dbea84c1 | 26792 | 65/65 PASS |
| observe-unpublished-whiteout-foundation | 20261006T0458509828090Z-ba13cdf771434d91ba9693071457dc65 | 27500 | 65/65 PASS |
| working-save-smoke | 20261006T0459553422511Z-dd8c2a81f1ae46f8abcc78984fe17492 | 39572 | 11/11 PASS |

Evidence directories under `C:/Dev/KingmakerGunslingerLab/runtime-evidence/`:

- `20261006T0457457240250Z-observe-unpublished-whiteout-foundation`.
- `20261006T0458509818095Z-observe-unpublished-whiteout-foundation`.
- `20261006T0459553412532Z-working-save-smoke`.

Every complete foundation run recorded two exact weather rollbacks and ten
native notifications, plus exact owned cleanup before its native one-way load
and exit. All three loaded identities match the artifact above; save-writing
API observed=false and automaticExitInitiated=true in every result.

Shared final transaction:

- Lease: `runtime-20261006T045720Z-986a500f4b5e40eda47d6e2bbd488978` / `runtime-lease.json`;
  status Completed, recoveryRequired=false, owned helper exited.
- Backup: `C:/Dev/KingmakerGunslingerLab/runtime-backups/live-mod/20261006T0457382975962Z`.
- Actual acquired live snapshot: 254 files plus directory inventory; full
  byte-for-byte equality verified after restoration while still holding lease.
- Processes: PIDs above exited, no Kingmaker remains. No owned provider,
  listener, diagnostic/forced state, active lease, compatibility lock,
  staging/deployment transaction or helper remains. Historical receipts and
  backups are retained as evidence, not active transactions.
- Ignored exact audit: `artifacts/mission/foundation-final-runtime-audit.json`;
  no raw runtime artifacts, saves, proprietary assemblies, packages or local
  configuration were committed.

Earlier continuation evidence:

- Weather-only gate: 15/15 PASS, run
  `20261006T0402483970988Z-50487eef976d485bacab33716fce964f`, PID 39268,
  exact weather/live restoration and Completed lease. This preceded production
  attack implementation; its engineering artifact was embedded at starting SHA.
- Initial foundation candidate: 59/60 assertions PASS, overall FAIL, run
  `20261006T0443164659345Z-58e43c29c1794e01a9e82aecf6551bd5`, PID 34996. Native friendly-target ray AutoHit
  skipped the seam. No assertion was weakened: corrected to actual native
  Neutral=true on a request-owned faction clone and proved genuine ray-stage
  delivery. Failure auto-exited, restored both weather scopes and live 254-file
  snapshot, and completed its lease.
- Corrected engineering candidate: 65/65 PASS, run
  `20261006T0449041404911Z-848e461d76554d54a1bfa0b2510cd692`, PID 7580. Dirty embedded checkpoint b97a0a;
  DLL `4615eb600eb88470a0bcbcece24c72f399c33d27fd3c58bb345b769f885258b4`, ZIP
  `daf61a5679c42bb53a67f4f8d942d4f265b29ea5b35d5a74b6441f8fdd9004a8`. This established the commit-ready
  candidate; it is separate from final clean two-process qualification.
- An early weather-edit workflow translated inherited LF text to CRLF and
  broke two source-text regression checks. Restoring the mission-edited shared
  file's exact LF format via UTF-8 byte writes repaired the introduced format
  regression. No inherited feature source or test was repaired. All full gates
  subsequently passed. No unrelated inherited failure remains.

## Changed files and commits

- `b97a0a47ca454009e892d9849d5c51aca56940c7`:
  `test(runtime): qualify disposable-process Whiteout weather fixture`.
- `db3725d7eb665731c3b8c238217cd39f2d0a65b8`:
  `feat(traits): add unpublished Whiteout native foundation`.
- Documentation closeout commit containing this file: full SHA in final receipt.

Complete changed-file inventory from exact base:

- `CODEX-WHITEOUT-DISPOSABLE-PROCESS-CONTINUATION-HANDOFF-2026-10-05.md`.
- `docs/research/WHITEOUT-NATIVE-ATTACK-CONTRACT.md`.
- `docs/research/WHITEOUT-NATIVE-WEATHER-CONTRACT.md`.
- `docs/research/WHITEOUT-WEATHER-FEASIBILITY.md`.
- `scripts/RuntimeAutomation.Common.ps1`.
- `scripts/Test-RuntimeScenarioPreflight.ps1`.
- `src/KingmakerGunslinger/ElementalRaces/WhiteoutFoundationPolicy.cs`.
- `src/KingmakerGunslinger/ElementalRaces/WhiteoutNativeMechanics.cs`.
- `src/KingmakerGunslinger/KingmakerGunslinger.csproj`.
- `src/KingmakerGunslinger/RuntimeTesting/RuntimeTestRequest.cs`.
- `src/KingmakerGunslinger/RuntimeTesting/RuntimeTestRunner.UnpublishedWhiteoutFoundation.cs`.
- `src/KingmakerGunslinger/RuntimeTesting/RuntimeTestRunner.WhiteoutAttackFixture.cs`.
- `src/KingmakerGunslinger/RuntimeTesting/RuntimeTestRunner.cs`.
- `src/KingmakerGunslinger/RuntimeTesting/RuntimeTestScenarioCatalog.cs`.
- `src/KingmakerGunslinger/RuntimeTesting/WhiteoutGuardedDiagnostics.cs`.
- `tests/KingmakerGunslinger.DomainTests/KingmakerGunslinger.DomainTests.csproj`.
- `tests/KingmakerGunslinger.DomainTests/Program.cs`.
- `tests/KingmakerGunslinger.DomainTests/WhiteoutContinuationTests.cs`.
- `tests/KingmakerGunslinger.DomainTests/WhiteoutNativeFoundationTests.cs`.
- `tests/KingmakerGunslinger.DomainTests/WhiteoutPolicyTests.cs`.
- `tools/validate_expanded_summoning_phase2a141.py`.
- `validation/static-validation.json`.

No Expanded Summoning branch, worktree, PR, source, asset, feature-specific test,
runtime scenario, save fixture or deployment was developed, modified, repaired
or reviewed. Inherited tests appear only in mandatory unfiltered/repository
gates; the two shared active count pins were the only permitted bookkeeping
changes in inherited validation files. No other backlog item began; Fiery,
Stoic, Aerial, Lunge, firearms, merchants and their publication remained unchanged.
No human visual acceptance was inferred or performed.

## Finite future work

1. Original Whiteout icon.
2. Owner art approval.
3. Stable visible feature/marker identities.
4. Undine prerequisite and Favored Class racial_traits publication.
5. Honest player text: outdoors-only Rain/Snow, no fog/waterfall, independent
   10% miss chance and concealment bypass.
6. Final player-facing runtime/manual acceptance.

The foundation mission ends here. No publication or other backlog work is authorized.
