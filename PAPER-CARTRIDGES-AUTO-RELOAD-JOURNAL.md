# Paper Cartridges Autonomous Journal

## 2026-08-08 — Intake started

- Branch: `codex/paper-cartridges-auto-reload` (created from exact baseline).
- Source baseline: local and remote `master`
  `759685077da0aed6d7ed1fda2cd43e5ad12d0bdb`; required ancestor
  `1c570bd4211d69c5c29f6af46a870146adb1645b` is present.
- Starting version: `0.0.73`; informational version
  `0.0.73-pistolero-musket-master`.
- Starting blueprint authority: expected registration 242; append-only ledger 243
  identities, one reserved (to be independently counted during inventory).
- Worktree was clean on intake. Initial sandboxed Git remote/ref writes failed
  with Windows access-denied errors; the authorized escalated retry succeeded.
- Architecture decision: follow the work-order central profile/catalog, one reload
  plan, generic atomic source transaction, append-only token, centralized misfire,
  native activatable, bounded normalization, and guarded runtime requirements.
- Files changed at this checkpoint: durable mission, matrix, journal,
  implementation report, qualification document, resume/blocker status.
- Focused tests: no feature tests yet; the unchanged complete baseline suite ran.
- Total deterministic test count: 935/935 PASS. The first restricted-sandbox run
  reproduced the inherited `audio.staging-lifecycle` `File.Replace`
  `UnauthorizedAccessException`; the authorized exact rerun passed 935/935.
- Validation/build/package: repository validation PASS; clean exact-reference
  Release build PASS; build-output PASS; Wwise/SoundBank PASS; package creation
  and strict standalone validation PASS. Baseline package SHA-256
  `18547e6792de17827310d7d4e45549bf73363aecacd3917c7720fded9952e168`;
  baseline DLL SHA-256
  `349432fd3d3b962677a33e1f1c2be9b623087af96e968bfd9d1074a8e71a2b17`.
- Runtime run IDs/process freshness: none; no runtime authorized before source gate.
- Package/DLL SHA-256: baseline values above; final candidate pending.
- Compatibility transaction IDs/restoration: none.
- Current uncertainty: exact native activatable fields, reload seams, Bokken target,
  and installed table lifecycle remain to be proven through bounded inventory.
- Next exact action: finish source/test/document inventory and replacement matrix,
  run unchanged baseline gates, commit intake, push with approved helper, and
  verify remote SHA equals local HEAD.
- Remote branch equals local HEAD: not yet; branch has not been published.

- Independent ledger count: 243 total, 242 active, one reserved. Bootstrap exact
  expected registration count is 242. `git diff --check` and the initial
  generated/binary/save/credential tracked-file audit passed; generated package
  and build outputs remain ignored/untracked.

## 2026-08-08 — Phase 1 ammunition/state foundation

- Branch: `codex/paper-cartridges-auto-reload`; base/intake commit
  `97432a9ca1b3c8c5876591950b7ae164b290bb9b` published and remote-equal.
- Version remains intentionally intermediate `0.0.73` /
  `0.0.73-pistolero-musket-master`.
- Architecture: immutable profile/catalog; definition-driven compatibility;
  exact paper loaded ID; early-family state-rule factory; append-only finite
  token states; inert cloned inventory item; project-owned semantic icon.
- Stable additions: Paper item `fea7337cfd06417a853546af9d950f77`, Normal
  paper token `a6344f33e7344d4aab249485faedf7fd`, Broken paper token
  `fdd814300fff4eea89d9d508663aebc0`.
- Ledger/bootstrap: 246 total / 245 active / one reserved; expected registration
  245. All prior 243 rows are untouched.
- Focused tests: six new profile/compatibility/fail-closed/token/item-source
  cases plus updated legacy token totals; complete suite 941/941 PASS.
- Failure/strategy evidence: two legacy token-count assertions correctly failed
  at 14/4 versus 16/6 and were narrowed without altering old token meanings.
  Strict packaging correctly rejected the newly packaged icon until the exact
  release allowlist added `paper-cartridge.png`.
- Validation/build/package: repository PASS; clean exact-reference Release PASS;
  build-output PASS; SoundBank PASS; package creation/strict validation PASS.
- Package SHA-256:
  `9c019ff426484b8d3ddc65f1d4b1164288efe4f594e10c37cfbb17fe68ac0139`.
  DLL SHA-256:
  `69bc766e65fc13f0b239c6805ef5bb07bd0e990747bd1d230b9c2b2d9c381168`.
- Runtime run IDs/process freshness: none; Phase 1 does not wire a selectable
  production reload and therefore makes no runtime mechanic claim.
- Compatibility transactions: none.
- Next exact action: commit/publish Phase 1, verify remote equality, then build
  the authoritative plan, generalized atomic source transaction, native Paper
  mode/grants, and manual Reload integration with focused rollback/action tests.
- Uncertainty: native activatable exact fields and Paper mode ownership still
  require installed-contract inspection in Phase 2.
- Remote branch equals local HEAD: pending Phase 1 publication.

## 2026-08-08 — Phase 2 deterministic candidate

- Branch/base: `codex/paper-cartridges-auto-reload`, descendant of
  `759685077da0aed6d7ed1fda2cd43e5ad12d0bdb`; Phase 1 commit
  `28db02b616d6fe25f6fc85e1eec10574849bd573` was pushed and remote-equal.
- Version remains the intermediate source pin `0.0.73` until the final release
  transaction.
- Architecture: one immutable `FirearmReloadPlan` binds exact unit/item,
  canonical definition, effective state, selected profile, source snapshot,
  requested/loadable rounds, and effective action. Manual availability,
  `AbilityData` action, `UnitUseAbility` construction, and delivery consume that
  plan. Loose and Paper sources share one exact rollback/verification service.
- Native mode: `KMG.Ammunition.PaperCartridgeModeMarker`
  (`69a804ea1fd14a5da3ba893c373f481f`) and
  `KMG.Ammunition.UsePaperCartridges`
  (`b0f16e90dc4e48929e111a7d56b62e5d`) are off-by-default, immediate/free,
  unit-local facts granted once through full and scoped proficiencies.
- Ledger/bootstrap: 248 total / 247 active / one reserved; every prior identity
  remains unchanged.
- Focused coverage: mandatory twelve-row action matrix, incompatible/no-fallback
  policy, exact loose/paper deltas, successful Paper state transaction,
  post-write rollback, mixed-ammunition rejection, and mode/grant source
  contracts. Complete suite: 948/948 PASS.
- Validation/build/package: repository PASS; clean exact-reference Release PASS;
  build-output PASS; SoundBank PASS; strict package PASS.
- Candidate package SHA-256:
  `bff694d8676014ee93c09c0de843a429102b6a4d1c4ebbd869758931a5033f24`.
  Candidate DLL SHA-256:
  `4625ec0ade5404f91062443707fa46f6780a3e658049070eabcd68e2835ffef1`.
- Runtime: `disposable-paper-cartridge-reload` added as a save-free guarded
  scenario. The first launch attempt was correctly refused because the harness
  requires a clean Git state; no Kingmaker process was launched. The candidate
  will be committed/pushed, then exact-build smoke and scenario runs will occur.
- Failures/changed strategy: historical validators were updated only where the
  former loose-only implementation tokens were deliberately replaced by the
  central planner/transaction and for the append-only count increase.
- Compatibility transactions: none. Runtime run IDs: none yet.
- Next exact action: publish this deterministic candidate, verify remote equality,
  then run fresh guarded `mod-load-smoke` and
  `disposable-paper-cartridge-reload`; repair from structured evidence if needed.
- Remote branch equals local HEAD: pending candidate publication.

### Phase 2 packaging repair

- Published candidate commit `da091247b991a907fe5ef3b9a63acae0d9e35c06`
  was remote-equal and clean.
- Guarded `mod-load-smoke` preflight stopped before launching Kingmaker because
  deterministic packaging expected 42 staged files but correctly observed 43:
  Phase 1 had added `paper-cartridge.png` without advancing this second exact
  package count. No runtime run ID or process was created.
- Strategy changed from runtime retry to correcting the narrow deterministic
  package invariant: counts are now 43 with SoundBank / 41 without, and the
  Python packager permits exactly those values. A focused source-contract
  assertion was added; complete suite remains 948/948 PASS.
- Next action: publish the repair, re-establish clean remote equality, then
  rerun exact-build smoke from a fresh process.

- Fresh guarded smoke process PID 31728 produced structured PASS run
  `20260808T2214448694538Z-7d5b72d3d60943959188117a8ce90793` on exact commit
  `bf9d8a3b0715d021016ba3232fd59beaa22972db`. The calling shell timed out after
  120 seconds, but the guarded result flushed at 92.214 seconds and automatic
  exit was initiated; the process subsequently exited. Evidence, not shell
  timeout, establishes PASS.
- The first feature-scenario request then failed before launch because the
  PowerShell preflight allowlist had not yet been extended even though the C#
  catalog was. Added the same save-free/basic-timeout scenario contract to
  `RuntimeAutomation.Common.ps1` and its exact preflight test list; 86 preflight
  checks PASS. No second Kingmaker process was created by that rejected request.
- Exact-commit smoke on `6d81f96a679b6d14d52c845f786b35c9258e9e4e`
  passed (evidence directory `20260808T2218450837788Z-mod-load-smoke`). The first
  launched Paper scenario run
  `20260808T2221032802923Z-71effcc5272d44c2823e830ac010dae6` ended ERROR before
  ammunition/state mutation: the disposable fixture used `UnitDescriptor.AddFact`
  for a `BlueprintBuff`, which returned no live Buff. Changed only the fixture to
  established native `Buffs.AddBuff` with an exact `MechanicsContext`, retaining
  AddFact as a narrow fallback. Production mode state remains native fact-owned.
- The native `Buffs.AddBuff` theory also returned no marker in run
  `20260808T2226306134022Z-f150d006759f4828bf3eee1c7103cfc2`; no reload or
  inventory mutation occurred. Exact installed reflection then proved
  `ActivatableAbilityCollection.Enumerable` and the `ActivatableAbility.IsOn`
  setter. The fixture now finds the exact granted activatable fact and toggles
  `IsOn`, exercising the real player-mode lifecycle rather than synthesizing its
  implementation buff. This is the changed strategy after the repeated marker
  insertion failure.

### Phase 2 runtime qualification complete

- Exact commit: `477145bb9c7de4e5037296644c21545004971d77`, pushed and
  remote-equal before both launches.
- Fresh exact-build smoke PASS:
  `20260808T2230529903156Z-088c067cec2b4397a53fa59ff09e4384`
  (90.302 seconds).
- Fresh save-free Paper reload PASS:
  `20260808T2233133870047Z-7cc71df785694c1bbdb6d3a747cb87d7`
  (90.280 seconds).
- Structured Paper assertions all PASS: proficiency granted the exact native
  mode and it was off by default; native activation produced the marker; the
  Pistol plan/action was Move and loaded the paper ID while loose stock remained
  exact; a named Reliable Pistol shared family compatibility, preserved both
  static enchantments and retained Broken condition; Advanced Rifle rejected
  Paper without fallback; request-local inventory/facts/items cleaned exactly.
- No save API was invoked. Both launches were fresh guarded Steam App ID 640820
  processes with automatic exit.
- Phase 2 remaining uncertainty: full-attack and Lightning Reload integration
  are deliberately Phase 3; misfire propagation is deliberately Phase 4.
- Next exact action: bind native auto-use/full-attack policy and Lightning Reload
  to the same plan, require exact Reload Firearm auto-use, and add the one-use
  genuinely-free Lightning fallback.

## 2026-08-08 — Phase 3 deterministic candidate

- Starting commit `c3b9957fdfc7fc901fd231f2547b61505743faba` was clean,
  pushed, and remote-equal.
- Architecture: Lightning Reload now reuses the selected `FirearmReloadPlan` and
  generic exact-source transaction. Its independent action result is Swift for
  loose/no matching Rapid Reload and Free for matching Rapid Reload or Paper.
  Dynamic `AbilityData`, runtime action, and `UnitUseAbility` constructor patches
  use that result. Marker addition remains success-bound with exact removal on
  transaction failure.
- Full-attack policy now requires exact Reload Firearm native auto-use, selected
  source availability, same item/target/native full-attack boundaries, and gives
  unlimited normal Free reload priority over a genuinely-Free, granted, unused
  Lightning Reload fallback. Non-free/unavailable cases end before an empty shot.
- Empty-attack continuation now also binds and revalidates the per-unit Paper
  mode state in addition to exact item/target/condition/action state.
- Focused tests: Lightning Swift/Free/selected-source/used-marker decisions and
  full-attack auto-use/no-fallback/Free-priority/one-fallback branches. Complete
  suite 950/950 PASS; guarded preflight 86 PASS.
- Clean Release/build-output/SoundBank/strict-package gate PASS. Candidate package
  SHA-256 `e583143792982e8d02790f125bb688ef7dff107590f8d432263c20d41393b60b`;
  DLL SHA-256 `15374279ea0a7a03c34474135da28860483c90720231c82015ec43dbad511f07`.
- Runtime scenario `disposable-paper-cartridge-lightning-reload` is allowlisted
  and extends the established level-11 fixture with real native Paper activation,
  Free `AbilityData`/runtime action, exact paper consumption, loaded identity,
  round reset, loose Swift control, Broken preservation, grit, and cleanup.
- Runtime run IDs: pending clean candidate publication and exact-build smoke.
- Next exact action: publish candidate; smoke and qualify Lightning; then add and
  qualify the dedicated native `UnitAttack` full-attack fixture.

- Published exact candidate `fd6bc51a06ef4ed7651cf81f1d3d5ceb7d729f40`.
  Fresh smoke PASS run
  `20260808T2247181810217Z-bb28c3276a5c45db90c706a39d6fd178`;
  fresh Paper Lightning PASS run
  `20260808T2249322389967Z-45e14b6dfb8f4312bc591156fdf7ec28`.
  The latter proved loose Swift control, actual Paper mode Free action in both
  `AbilityData.ActionType` and `RuntimeActionType`, one cartridge/one chamber,
  same-round rejection, native round reset, Broken preservation, grit, and exact
  external restoration.
- Added the dedicated `disposable-paper-cartridge-full-attack` fixture around a
  real native `UnitAttack`, exact `LastAttackRule`, `AttackHandInfo` planned
  attack, exact slot/item/target, and native `AutoUseAbility`. It covers repeated
  Pistol reload boundaries, auto-use-off termination, paper exhaustion with loose
  stock preserved, and exact cleanup. Initial isolated preflight fingerprint
  check raced a just-completed local build once; unchanged isolated retry passed
  all 86 checks. Complete domain suite remains 950/950 PASS.
- Next exact action: publish the full-attack fixture checkpoint, run exact-build
  smoke, then runtime-qualify the native full-attack scenario and repair only
  from its first failed invariant if necessary.

- Exact `e1fcc00dca06a11b95f8bd946952f7f4cf7298e5` smoke PASS; first
  full-attack run `20260808T2257433338888Z-3c23c3d324f0435497ab500989ec5d02`
  ended ERROR before the reload prefix or inventory mutation because the generic
  property helper found inherited/declared `UnitAttack.Target` ambiguous. The
  fixture now uses a narrow declared-type setter for the three exact UnitAttack
  properties; production mechanics are unchanged.
- Second full-attack run
  `20260808T2304152732713Z-bcc2ee8c7b474fd3a522650a7e584f03` completed cleanly
  but FAILed all mechanical assertions with `attempted=0`, proving the prefix
  returned before recognizing a planned iteration. Bounded installed IL for
  `UnitAttack.get_PlannedAttack` reads only `m_AllAttacks.Count` and
  `m_AttackIndex`, rejecting the constructor's `-1` index by unsigned comparison.
  The fixture now sets the exact demonstrated index to 0 after adding its native
  `AttackHandInfo`; no production logic changed.
- Third full-attack run
  `20260808T2310433234529Z-3a45bd3b8c174f0aa5aba0c25896c0f9` on published
  commit `cee6b96cec8f1edfd25d3b7d775c6e6c3d48fc27` again reached structured FAIL
  with `attempted=0`, while cleanup passed. No ammunition or firearm mutation
  occurred. The next bounded fixture revision records the exact native boundary
  inputs (full-attack flag, executor, previous/planned weapon identity, equipped
  firearm resolution, and auto-use identity) before changing any theory.
- The diagnostic run
  `20260808T2317028628347Z-39c191cc571e4c19b8aaccd77e2810f0` produced an exact
  `UnitAttack.get_PlannedAttack` null-reference before mutation. Installed
  Assembly-CSharp IL proves the constructor leaves `m_AllAttacks` null and the
  public `AllAttacks` getter returns a shared fallback when null; adding through
  that fallback did not initialize the command. The fixture now assigns a
  request-local `List<AttackHandInfo>` to the exact demonstrated backing field,
  matching the native property contract without changing production behavior.
- The next run
  `20260808T2322018744715Z-d79c969cbc9146a583109904b3e6a92b` proved every
  instrumented boundary exact except `executor=False`: full attack, previous and
  planned exact weapon, equipped firearm resolver, and Reload auto-use were all
  true. Installed reflection proves `UnitCommand.Executor` has an exact native
  setter and backing field normally populated by command scheduling. The fixture
  now sets that exact base property to its request-local unit.
- Phase 3 runtime qualification PASS on published commit
  `73e3cdf7f4773b8a5b5985c65cfd96384c5c054c`:
  - prerequisite smoke evidence directory
    `20260808T2324554987384Z-mod-load-smoke`;
  - native full-attack run
    `20260808T2327250125449Z-4dc36ba60fe44e9c9af7ca06fb32912b` PASS;
  - exact observations: native boundary identities all true, two repeated paper
    reloads committed (`attempted=2`, `succeeded=2`), auto-use-off ended without
    spending, exhaustion ended without loose fallback, and cleanup was exact.
- Deterministic total remains 950 PASS. Phase 3 is complete; next exact action is
  Phase 4 central ammunition-aware misfire policy and ordinary/Dead Shot/Scatter
  propagation with focused boundary tests.

## Phase 4 central misfire mechanics checkpoint

- One dependency-free `EffectiveFirearmMisfireValuePolicy` now owns the fixed
  arithmetic: base plus condition/training, loaded profile modifier, exact-weapon
  reduction, then one `0..20` clamp. The Kingmaker-bound
  `EffectiveFirearmMisfirePolicy` remains the sole exact-item Reliable resolver
  and delegates to that arithmetic.
- Ordinary discharge passes the exact pre-discharge ammunition identity into its
  item-bound misfire context. Dead Shot and Scatter capture the same `before`
  identity before emptying the chamber. Dead Shot probe registration now accepts
  the centralized legal threshold 0.
- Focused coverage proves loose control, Paper +1, Broken trained/untrained,
  Paper-before-Reliable ordering, Reliable loose 0 / Paper 1 boundaries, maximum
  clamp, unknown-ID fail-closed, and all three consumer source seams.
- Complete deterministic suite: 952 PASS, 0 failures.
- Clean Release build, output validation, SoundBank validation, package creation,
  and strict standalone package validation: PASS.
- DLL SHA-256:
  `04BEEA35CBF9B7455CDF36ECEC2AAB0E50634233699F869DA009C9BD7D2CD348`.
- Package SHA-256:
  `092526BD31F42A2ECD5C555DFFCBD7C767E03645DF8549B4F2959F1B98ED2B5D`.
- Current version remains the intentional vertical-work pin `0.0.73`; release
  pin advancement remains Phase 7. Next exact action: commit/push this mechanics
  slice, then add the guarded ordinary/Dead Shot/scatter Paper runtime fixtures.
- First guarded Paper misfire run
  `20260808T2340116760278Z-c1e0c7a064e04633bbfc7442f5f49d83` reached the
  ordinary Reliable Paper/loose boundaries but the subsequent Dead Shot reuse of
  that exact previously fired Reliable item exposed no native probe roll. Cleanup
  passed. The narrower retry uses a separate mundane Pistol paper chamber and
  forced natural 2, which directly proves Paper raises Dead Shot's base threshold
  from 1 to 2 without coupling the fixture's independent Reliable lifecycle.
- Retry `20260808T2345237314982Z-6a4c7a1122d84226bae6ef7cc7a5567c`
  reproduced the missing Dead Shot probe roll with the isolated mundane item,
  disproving item reuse. The only remaining difference from the established
  passing Dead Shot fixture was a retained native Immortality state on the target.
  Because all ordinary controls use `-100` attack bonus and the Dead Shot is an
  all-misfire delivery, the retry removes that unnecessary target-state mutation.
- Retry `20260808T2350428300401Z-2231d4fc6e1b4b27b066536b96dffaea`
  reproduced the failure, disproving target Immortality. The established legacy
  Dead Shot scenario then also ERRORed on the same assembly in run
  `20260808T2353155279726Z-0adbd45793fc496ea35cd383d24a9522`, proving a touched
  source regression rather than Paper-fixture construction. Narrow counters are
  added at registration, exact Roll-setter prefix, and success postfix so the
  next structured failure identifies the first missing native boundary.
- Diagnostic run
  `20260808T2359038674190Z-b4309659f0c34d978069812c3a9f6f9f` observed
  `registered=1;rollSetter=0;success=1`. This exactly matches the installed
  direct-field-write composition already handled by ordinary misfire: native
  `IsSuccessRoll(d20)` is authoritative when the Roll setter is bypassed. Dead
  Shot now records and verifies that exact d20 at the success postfix. Its
  runtime-only forced hook seeds the native d20 to the requested value and then
  verifies equality; production attacks retain native randomness.
- Phase 4 final runtime evidence on published commit
  `30536b5174923c306fdae3955b2ae8c263ffd24e`:
  - smoke `20260809T0003000198385Z-mod-load-smoke` PASS;
  - legacy Dead Shot regression
    `20260809T0005170088591Z-05e0389ee5354550a79a12fd20a5f141` PASS;
  - Paper ordinary/Dead Shot
    `20260809T0007093055795Z-14d2669b768a48b09fd7e560d546fb93` PASS;
  - separate Scatter smoke `20260809T0009488210941Z-mod-load-smoke` PASS;
  - Paper Scatter
    `20260809T0011589772079Z-885ae93125bf4979adf69afe8b7948eb` PASS.
- Phase 4 is complete with 952 deterministic tests, clean Release/package gates,
  exact pre-discharge ammunition across ordinary/Dead Shot/Scatter, Reliable
  ordering, direct-write d20 fallback, aggregation, transition, and cleanup
  evidence. Next exact action is Phase 5 crafting/vendor/economy implementation.

## Phase 5 crafting/vendor source checkpoint

- Appended `KMG.Gunsmithing.CraftPaperCartridges` at stable GUID
  `936ffac5400b46b3a72fe503e0947288`; registry is now 248 active and the
  append-only ledger is 249 identities including one reserved.
- Extracted one shared crafting transaction for both recipes. It snapshots exact
  money, output counts, and the shared marker; verifies 20-output commits; and
  restores marker, inventory, and money on failure. Existing basic output/cost is
  unchanged; Paper is exactly 20 for 120 gp and both abilities use the same marker.
- Gunsmithing grants four facts exactly: Repair, Overhaul, basic crafting, and
  Paper crafting. Paper now has zero sale value.
- Smith and all installed BTSL bounded normalization desired/owned sets now
  contain exactly one Paper entry at count 200. Advanced/named exclusions and
  fixed rare-loot code are unchanged.
- Focused plus complete deterministic suite: 954 PASS, 0 failures. Clean Release,
  build-output, SoundBank, package, and strict standalone validation: PASS.
- DLL SHA-256:
  `4D91A3A45FDAA9639A0B2A1B06EBC26649C311467CD7F13661736CFF7E224272`.
- Package SHA-256:
  `E3C022FF4A7E34FD8B086235F4C752937CE2882502D152336276400A549ED234`.
- Next exact action: publish this source checkpoint, run the vendor observer to
  capture bounded Bokken owner/table evidence, then implement only an exact safe
  Bokken target or document an evidence-backed defer.
- Vendor graph observation PASS:
  `20260809T0023419883620Z-05148294115f464c8e6deb578ace0270` after smoke
  `20260809T0021341618639Z-mod-load-smoke`. Smith/BTSL Paper publication passed
  the updated exact 12-entry/48-total assertions; rejected Jhod and all five rare
  fixed-loot relationships remained exact.
- Bokken outcome: evidence-backed defer. The exhaustive installed shared-vendor
  owner/unit/direct-reference graph contains no Bokken-named identity or unique
  safe Bokken table/lifecycle tuple. Exact evidence and the no-guess conclusion
  are recorded in `planning/PAPER-CARTRIDGES-BOKKEN-INVENTORY.md`.
- Dedicated crafting/vendor runtime PASS on published commit
  `49dc01a51d31690b3f2697e4f0c8d06cbc13d667` after smoke
  `20260809T0029043362376Z-mod-load-smoke`: run
  `20260809T0031147623118Z-dad8b1e070d049d09c4f22c295fb6504` observed
  `crafted=True;sharedBlocked=True;smith=1:200;btsl=4/4;jhod=0;cleaned=True`.
- Phase 5 is complete: 954 deterministic tests, clean Release/package gates,
  shared atomic crafting, exact vendor stock, zero-sale source policy, and bounded
  Bokken defer. Next exact action is Phase 6 paper-token persistence,
  reconstruction, static-enchantment lifecycle, and per-unit mode isolation.

## Phase 6 persistence and lifecycle checkpoint

- Published fixture checkpoint `2da2f60948c8e70b964e88e54675e069c042a457`
  adds request-local two-unit Paper mode isolation and proves that exhausting the
  exact Paper Cartridge stack does not deactivate the native activatable mode.
  Cleanup restores a nonzero pre-fixture cartridge snapshot as well as loose ammo.
- Repository validation and the complete deterministic suite passed: 954 PASS,
  0 failures. Two sandboxed attempts failed only because the audio staging test's
  deliberate Windows-Temp `File.Replace` was outside the workspace write root;
  the same unchanged suite passed with the required disposable-temp permission.
- Clean Release build, build-output validation, SoundBank validation, package
  creation, and strict standalone package validation: PASS.
- Guarded exact-build runtime evidence on the published checkpoint:
  - smoke `20260809T0036508292089Z-mod-load-smoke` PASS;
  - enhanced Paper reload
    `20260809T0038463040427Z-disposable-paper-cartridge-reload` PASS;
  - lifecycle prerequisite smoke `20260809T0040527737094Z-mod-load-smoke` PASS;
  - installed item lifecycle
    `20260809T0042467907112Z-observe-firearm-item-lifecycle-contracts` PASS;
  - switching prerequisite smoke `20260809T0045098278584Z-mod-load-smoke` PASS;
  - production firearm switching
    `20260809T0046598688957Z-disposable-production-firearm-switching` PASS.
- The enhanced reload fixture proves exact Normal and Broken Paper token use on a
  named Reliable Pistol while preserving its two static enchantments. Existing
  deterministic token/codec/repository/reconciliation tests prove round trips,
  exact old meanings, two-item isolation, corruption fail-closed behavior, and
  token-only replacement. The installed lifecycle and switching scenarios prove
  the inherited carrier/equip boundaries remain exact.
- Persistence limitation is deliberate: no autonomous save was created or
  written. Fresh save/restart persistence of the two newly appended Paper tokens
  remains one narrow human-only checklist item; inherited item-token persistence
  evidence is reused only for the carrier contract, not misreported as a fresh
  Paper-token save round trip.
- Current version remains `0.0.73`; blueprint registry remains 248 active and the
  append-only ledger 249 identities including one reserved. Next exact action:
  publish this Phase 6 evidence, then begin the transactional 0.0.74 release pin
  and final comprehensive/compatibility qualification.

## Phase 7 final candidate and qualification

- Advanced every active source, assembly, package, runtime, schema, validator,
  and compatibility pin to `0.0.74` / `0.0.74-paper-cartridges-auto-reload`.
  Added the version-aware Paper validator and guarded comprehensive scenario.
- Final source retains 248 active blueprints and 249 ledger identities including
  one reserved. Six active Paper identities were appended; every baseline identity
  and all merged rare-firearm registrations remain unchanged.
- Compatibility exposed one exact installed direct-field forced-roll defect in the
  Wwise misfire control. Runs `20260809T0113406691395Z`,
  `20260809T0117398519440Z`, and `20260809T0124083733031Z` failed the same first
  invariant. Added structured counters, observed queued rolls consumed but native
  values evaluated, then repaired evaluation to use the exact context's final roll.
  Wwise passed on the changed strategy in
  `20260809T0134477575049Z-disposable-firearm-wwise-audio`.
- First Arms & Armor attempt `compat-20260809T014514Z-ae0c75c10df7` timed out in
  the unrelated production-switching fixture and initially refused restoration
  while guarded PID 21460 remained. The process was verified as the exact request,
  stopped, and restoration then verified `True`. The retry removed that unchanged
  theory because Paper comprehensive already exercises native full attacks.
- Final compatibility PASS/restoration transactions:
  - standalone `compat-20260809T013637Z-4660ecf4446e`;
  - Arms & Armor `compat-20260809T015702Z-08d5c0a31965`;
  - Toggle Custom Soundpacks `compat-20260809T020420Z-0100b5e97026`;
  - qualified combined `compat-20260809T021134Z-a8ec10bef81e`;
  - one bounded Call of the Wild smoke
    `compat-20260809T021852Z-bf16e6df813d` (PASS/restored; existing human-gate
    `CONFLICT-CONFIRMED` classification unchanged).
- Two independent exact-final comprehensive PASSes:
  `20260809T0223531574928Z-disposable-paper-cartridge-comprehensive` and
  `20260809T0228043712566Z-disposable-paper-cartridge-comprehensive`.
- Two canonical guarded working-save PASSes:
  `20260809T0230156578884Z-working-save-smoke` and
  `20260809T0232431480084Z-working-save-smoke`.
- Final complete deterministic count: 954 PASS, 0 failures. Clean Release,
  build-output, SoundBank, strict standalone package, scenario preflight, request,
  compatibility filesystem, and repository validation gates PASS.
- Final hashes: DLL
  `24C06ABAADB0F6CD9BD9BDE1153766C5F343933D93D1CE3F5FD6B94750A1B928`;
  local-runtime package
  `19AE04841664CF5C54C02D70140D932ED30315DA5026184874F1E20D8B16CE94`;
  strict release package
  `86C701611008EC9DDD11072130E0B45CF06768444F80C51E3429A877AAC93B4F`.
- Known inherited unrelated blockers remain detached Gunslinger's Dodge missing
  buff and Targeting Torso cached critical threat. No assertion was weakened.
- One narrow human-only item remains: an optional disposable save/restart check of
  a Paper-loaded token plus visual icon/text review. No autonomous save write was
  performed or claimed. Next exact action: finalize documentation commit, run the
  final clean-tree/audit gates, publish, and prove local/remote equality.
