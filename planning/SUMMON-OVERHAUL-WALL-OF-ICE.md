# Summon visual overhaul: Wall of Ice investigation

Active investigation for the owner-requested summoned creature visual overhaul.
This record does not amend Sprint 35's historical qualification or release.

## Intake and status

- Source baseline: `ffb1f8bf2c0014ee6bb5465103252195f75e449b` (fetched
  `origin/master`, released v0.0.177 source).
- The actual local installation's `Info.json` reports **0.0.140**. It cannot
  reproduce the released Bone Devil grant. Do not replace this intake with a
  presumed v0.0.177 installation or leave a candidate installed.
- Owner report: "Wall of Ice does nothing." **Open; not reproduced or fixed.**
- No native execution, cast, save access or installation mutation has been
  performed by this investigation.

## Baseline build preparation

- Initial direct script invocation was refused by the machine's execution
  policy (0.02 s); subsequent commands use the repository-standard
  `powershell.exe -NoProfile -ExecutionPolicy Bypass` invocation.
- First development build refused the old private reference bundle because
  `Managed/UniRx.dll` was absent (4.08 s). The installed reference exists.
  The repository-owned `export-private-build-references.ps1` exported all
  required exact current installed references into ignored
  `artifacts/summon-overhaul-baseline/reference-input.zip`; the original
  private bundles and installed files were preserved. Build uses the explicit
  expanded bundle path, with its normal provenance verification.
- The first exact-reference build attempt ran 266.97 s and passed development
  source validation and the preceding offline wrapper fixtures, then failed
  the closed-Sprint35 fixture's historical shared-clone checkout because
  historical promised Git blobs were missing locally. No product compiler,
  package or native launch was reached. The raw failure log is
  `artifacts/summon-overhaul-baseline/build-exact-current.log`.
- Exact missing tree objects (236) for the unchanged closed-Sprint35 through
  Sprint38 fixtures were fetched from the approved repository in 2.21 s.
  No refs were reset, worktree selected, source changed, or checks bypassed.
  A justified build retry is captured separately in
  `artifacts/summon-overhaul-baseline/build-retry-prefetched.log`.
- That retry passed the full offline development wrapper and closed-Sprint35
  through Sprint39 refusal suites, then stopped after 383.49 s at domain
  compilation: sandbox `CS2012` denied creating the test executable. No
  existing frozen executable was overwritten. A narrow rerun of the same
  nine Sprint35 domain checks outside the sandbox passed **9/9** in 4.24 s.
  This is baseline source evidence, not execution on the later feedback and
  observer edits.
- `Build-Local.ps1` has no supported development resume route and expressly
  forbids `SkipDomainTests` for development candidates. A third baseline
  build was not selected. No validated baseline package or native baseline
  exists. Cumulative baseline validation was **661.01 s (11.02 min)** plus
  the approximately four-second current-reference export. All failure logs
  and partial outputs remain intact. Native installation, settings and saves
  were never mutated by these attempts.

## Reviewable changes

`ContextActionFormInfernalWall35` now reports an executed player-owned cast's
changed/invalid placement, adjacent SR/immunity disruption, successful
adjacent Reflex disruption, or null section spawn through the existing
failure-contained native combat-log publication service. The repeatedly
queried target checker remains side-effect free. These messages make
demonstrated silent paths understandable; they do **not** establish the
reported functional root cause or change any rule.

The existing early35 observer adds a scoped, read-only `RuleSummonUnit`
listener around the first ordinary wall cast. It records native duration,
bonus duration, section identity, original HP, actual original buff lifetime,
and original deadline at the synchronous native rule completion, before
yielded frames age the effect. Its predicate requires 36 HP, requested 720 s,
zero bonus, and actual 720 s lifetime (0.01 s tolerance). The listener always
unsubscribes on scope exit. No controlled clock or duration is changed.

## Traced production path

`ConfigureWallAbility35` supplies a Medium-range point-target Standard-action
spell-like ability, `InfernalWallTarget35`, and
`ContextActionFormInfernalWall35` through `AbilityEffectRunAction`.

The outer run action already uses `SavingThrowType.Unknown`. Reapplying the
earlier outer point-target-save fix would not change this source.

`InfernalWallTarget35` and the action revalidate a grounded unoccupied footprint
against the active nearby native `GridGraph`. The action then checks each
adjacent creature's native SR/immunity and Reflex disruption, and uses native
`RuleSummonUnit` for a 120-round section linked to the Bone Devil.

The exact private section's `OnDataAttached` patch creates its visible ice
plane, hides its donor renderers, creates a native rectangular `NavmeshCut`,
and queues reference-counted native GridGraph claims. The section's lifecycle
and source-lifetime components release navigation on breach, expiry, source
retirement, module disable, and view teardown. Destruction creates the
supported residual cold area; fire destruction additionally creates native
obscuring steam.

The baseline action returned silently for invalid/changed placement, adjacent
spell resistance/immunity, successful adjacent Reflex disruption, or a null
summoned section. These are **possible explanations**, not an established
root cause. A successful command alone does not establish formation.

## Bounded native reproduction selection

Existing request (no `SaveName`):

```powershell
.\scripts\Invoke-ExpandedSummoningRuntimeScenario.ps1 `
  -Scenario disposable-expanded-summoning-sprint32-native-review `
  -ScenarioParameters @{reviewPhase='sprint35-infernal-early'} `
  -ExpectedVersion 0.0.177 -PackagePath '<validated exact package>' `
  -DevelopmentPurpose '<same purpose as development receipt>' `
  -TimeoutSeconds 600 -CompletionTimeoutSeconds 600 -Confirm:$false
```

The existing save-free Oleg fixture selects an empty native floor lane, issues
the ordinary Bone Devil point cast, and records section identity, target
admission, command result, visible plane, orientation, native node claims,
actual path obstruction, physical attack/object halving, lethal fire,
residual/steam, adjacent disruption, and exact navigation restoration. It
also observes the directly related body/tail. Its evidence file is
`sprint35-infernal-early.json` under the guarded run's evidence directory.

The revised predicate asserts original 36 HP and 720-second duration. It still
does **not** observe natural expiry: its section is destroyed. The receipt
explicitly records `naturalExpiryObserved=false`. A separate original-deadline
expiry observation using the established controlled game-time seam remains
required for mission closure; no twelve-minute real-time soak is needed.

Use the clean-commit development candidate route in
[DEVELOPMENT-CANDIDATES.md](../docs/DEVELOPMENT-CANDIDATES.md), with purpose,
named focused test selection, exact installed/private reference verification,
strict package validation, runtime lease and transactional restoration. Root
coordinates the one native lane; this selection does not authorize an
unguarded launch or a replacement of current machine intake.

## Remaining acceptance

1. Reproduce once against the verified current source candidate at a legal
   empty point; retain failures and exact package identity.
2. Establish the actual failing path before selecting a mechanical repair.
3. Observe the new player feedback for legitimate placement/disruption results.
4. Prove a visible grounded 10-foot plane, real movement obstruction, 36 HP,
   original 12-minute duration, and obstruction removal on destruction/expiry.
5. Keep the established planar adaptation and breach/steam behavior. No
   hemisphere, line-of-effect blocking, arbitrary blast or rebalance is selected.

The Wall defect remains explicitly open until these native observations exist.
