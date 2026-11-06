# Summon visual overhaul: Wall of Ice investigation

## Sprint 2 navigation continuation — durable claims, stationary sections, pooled steam

[Navigation continuation in the Sprint 2 checkpoint](WALL-OF-ICE-SPRINT2.md#navigation-continuation-development-candidate-pr-98-stays-draft-and-unmerged).
Current; supersedes the closeout's navigation finding below.

- **Result.** Executable source `4aa27dcfea3220b1c87fa2a73a7c7ebe061985cc`
  passed the guarded `summon-overhaul-wall` phase: **PASS, 155 assertions**.
  - Run `20261106T1642352570083Z-384c0780508b46ab889275a3ada550d7`; ZIP
    SHA-256 `e9abfcc07d5e43a66e88a0e15381b155c650f8956c964c4408ee1e7166801d68`;
    DLL MVID `818561cd-e8b1-4633-9ce8-c6363cd3e9ab`.
  - Product source unchanged since `865ce2f881`. The 152 and 95 credits stay
    with their own candidates.
- **Placement survey** now uses production legality: no claim-owner
  exclusion, and `TryResolve` with `CanTarget` deciding. Under the closeout's
  production it reached genuine shared claims (nav1: 3 cells with both
  owners, both orders, exact restoration). Under the selected design they are
  unreachable by construction.
- **Turn-based effect of the reopened cells** (closeout production):
  - Turn-based entry erased every claim.
  - Native turn-based routes and an ordinary turn-based move cut the ice's
    end corner, with the centre about 0.23 m inside the footprint (nav4).
  - No crossing through the Wall's length was observed.
- **Production.**
  - Claims hold both static layers.
  - The section refreshes grid connections and erosion tags natively after
    its own writes. That refresh is the cause of close28 that was missing.
  - Wall sections carry `CantMove`: native summon follow had walked a section
    about 2.8 m (nav4).
  - The steam cloud's flame renderers are restored at
    `GameObjectsPool.Release`: the Wall's released cloud had been reused by a
    native claim with 16 renderers off (nav2, nav4).
- **Evidence for the selected design** (nav5, nav9):
  - Claims held in turn-based mode, and routes and the move stayed 0.12 m or
    more from the ice.
  - The diagonal two-Wall geometry kept the survivor's claims in both orders.
  - Final restoration exact (connections and tags included).
  - The physical-lane route was restored; steam pool reuse matched the prefab.
- **Restoration.** All nine runs restored the intake (mod 0.0.140), settings
  and protected saves exactly.
- **Ledger.** Continuation ≥ 97.40 min; cumulative ≥ 592.01 min. Lower bounds.

## Sprint 2 closeout — two-Wall navigation, delivered feedback, steam and tail

[Closeout in the Sprint 2 checkpoint](WALL-OF-ICE-SPRINT2.md#closeout-development-candidate-pr-98-stays-draft-and-unmerged).

- **Result.** Executable source `24a1b935831a2376571d706d382c1db40a45ddfc`
  passed the guarded `summon-overhaul-wall` phase: **PASS, 152 assertions**.
  - Run `20261106T0745404351478Z-fee2167da95f4e00bdb23eb8c1b69a45`; ZIP
    SHA-256 `ff4af5e181c4c11af4b65a9d7bf7742b0c4a53285a8f623ef44d9d80d33f1e22`;
    DLL MVID `1942ad28-e1f6-4572-a112-73cf24d5b512`.
  - The 95-assertion credit stays with its own candidate `36e44513d3`.
- **Production.**
  - The Wall's steam draws the native kineticist steam cloud, with its flame
    renderers off. The native Obscuring Mist effect link loads nothing in this
    install.
  - A standing Wall section no longer counts as a creature that can save
    against another Wall's formation.
- **Observer.**
  - Feedback passes only when delivered: one attempt, one publication,
    0 faults, exact message.
  - New: a two-Wall scenario in both retirement orders, an ordinary-camera
    tail sequence, and steam presentation checks.
- **Finding.** Native grid updates (`erodeIterations` 10) overwrite Wall
  claims while a Wall stands. Retiring Wall B reopened 4 cells that Wall A
  still claimed; the ice footprint stays blocked by each cut's own counter.
  A narrow fix failed native acceptance (close28) and was reverted; the
  navigation continuation above resolves it.
- **Restoration.** Every run restored v0.0.140, settings and protected saves
  exactly; close4 was restored manually after the host stopped it.
- **Ledger.** Additional ≥ 283.29 min; cumulative ≥ 494.60 min. Lower bounds.

## Sprint 2 complete — full native Wall acceptance and tail contact

[Sprint 2 checkpoint](WALL-OF-ICE-SPRINT2.md): exact executable source
`36e44513d3fa7e9a670245a2d1bfe39153058702` passed phase `summon-overhaul-wall`
in one fresh guarded process, **95 assertions**, run
`20261105T2213068465724Z-afd04aeb8dcf441bafaf72b5dce04b7f`.

Production repairs: donor body stays hidden through formation and removal;
rejection feedback reaches a player's summoned Bone Devil; ice uses native
Stone hit/death effects instead of red blood; a retiring section reverts its
cut's native grid `DynamicBlocked` block before the cut is destroyed.

Every item under "Remaining acceptance" below is now natively observed:
formation (36 HP, 720 s, grounded), player feedback without polling spam, live
obstruction, unmodified attack eligibility, repeated-attack and fire
destruction, owned residual cold and steam, controlled original-deadline
expiry, exact two-layer navigation restoration after each retirement, and
correlated tail contact. The historical attack refusal was the installed
ProperFlanking2 reach dead zone, not distance or a Wall LOS defect. Open owner
decisions (stone chips, bleed on ice, invisible residual, fire embers/steam
visibility, natural reach vs ProperFlanking2) are listed in the checkpoint;
their closeout dispositions supersede them.
Validation 93.99 min added; cumulative 211.31 min. Dated records below are
historical.

## Sprint 1 complete — fixture prerequisite verified; full Wall acceptance pending

[Sprint 1 checkpoint](WALL-FIXTURE-SPRINT1.md) records the implemented shared
native-constrained lane/placement/baseline/return correction and two successful
fresh-process preflights on exact source
`8c9ccb761c4742ba1da9aa1e028064df883f806b` (21 assertions each).

The first candidate's repeat failure was retained and diagnosed before repair:
native Translocate agreed immediately, then a new Move appeared while the
witness carried naturally applied Bone-source frightened state. Shared fixture
setup now isolates that exact unrelated effect, preserves natural source memory
and requires ordinary native panic retirement. No production aura/Wall change,
roll override, position write or tolerance increase. Both final passes had no
setup fear to remove; nonempty removal was not separately reproduced. The old
nineteen-metre displacement's causal explanation remains unproven.

Both final runs proved immediate and three-frame intended placement, real native
Success movement and return to the immutable planned start. They explicitly
stopped before Wall ability lookup/casting, with zero sections/claims. Exact
installation/settings/protected saves restored. Additional validation 32.46min,
retaining 84.86min; the owner superseded that prior ceiling for this sprint.

Do not continue into Wall acceptance within this sprint. Next: legal casting,
actual obstruction and attack eligibility, physical breach, destruction, exact
navigation cleanup, controlled original-deadline expiry, post-removal movement
and Bone Devil tail. Prior P2 navigation/LOS snapshot correction remains intact
but its post-cast branches remain unexercised here. PR stays draft/unmerged.
The earlier dated outcomes and allowance chronology remain below unchanged.

## Latest Wall-only diagnosis: early record corrected, initial placement refused

Owner authorized one diagnostic build/run within a ceiling raised from 70 to
85 minutes, retaining 69.78. Creature/model/scale work and completed motion
checks were held fixed. Source `ee5812b64e5a914a3036588827d75a58d83fe552`
now persists all existing navigation observations before the live mover/attack
abort boundary, attaches that snapshot to eligibility and records the exact LOS
object ID used. No new framework, gameplay change or weakened assertion.
Compilation / 10 focused tests and immutable candidate packaging PASS.

ZIP SHA-256 `0a14c203579a31b115eab142d104ddd5f73f66eb0efb3ca18875046bc0bad5c1`.
Wall-only run `20261105T1539045203048Z-disposable-expanded-summoning-sprint32-native-review`
FAIL / 15 assertions total; result SHA-256
`ab2ea894f82bd5b9480218d35a0606ad16b556c9f54ca1d3507bf5aa78104785`.

The run failed **before casting** at initial-path settling. Requested origin
`[12.747345,-5.749453,21.9154739]`; actual settled actor
`[-5.7529707,-5.61068439,26.8096256]`, horizontal gap about 19.14m. Three quiet
frames did not imply correct placement. Native floor mismatch was 0.02015m
against the unchanged 0.02m tolerance; expected horizontal position also failed
independently. This is a fixture-precondition failure, not new evidence about
Wall obstruction or attack eligibility. The new pre-breach/LOS records were not
reached. Their diagnostic branches remain native-unexercised, despite being in
the actual loaded candidate. Earlier two questions remain unanswered.

Next narrow investigation: reconcile entity/view/native movement state across
the initial placement and first settled frames, then select a fixture repair
before another Wall run. No evidence supports reach/LOS overrides, a relaxed
support guard or a production Wall repair. Do not repeat the unchanged failure.
Physical damage, obstruction, exact grid/claim restoration, controlled expiry,
post-removal movement and Bone Devil tail contact remain open.

Current installation/settings/protected saves restored exactly; no load/save.
One build 647.231s, one Wall run 237.418s, review reserve 20s: **84.86/85 minutes**.
Exact candidate/DLL identities, commands and prior ledger retention are in the
active overhaul checkpoint. PR stays draft; accepted creature checks stay
credited. The records below retain historical results and allowance chronology.

## Latest approved 70-minute checkpoint: drift fixed; native attack eligibility unresolved

Candidate source `59603558d754c0fe3c8aa5767430030b218daa2b`, ZIP SHA-256
`01a33a806b43c6689bb46e478f6bb5675fb3c8c04a6b5440695dc1eee6faaea9`,
ran Wall at `20261105T1450190511141Z-disposable-expanded-summoning-sprint32-native-review`.
FAIL / 18 assertions total; result SHA-256
`58ff9a8575c112f1c6f6d7086aeba1e127c36b3699ffcf90e935b5dbbc7aaf4d`.

Baseline actual movement passed, and the visible plane was captured. The physical
witness now remained at its intended point through three supported quiet frames:
the previous 17.92m drift was corrected. Its native UnitAttack eligibility still
refused at reported separation 2.5m / approach radius 4.848. This run recorded no
live-mover detour assertion. Physical damage, exact grid restoration, controlled
expiry, post-removal movement and the later tail check were not reached. Earlier
actual-detour and controlled fire-destruction results remain credited to their
own candidates; they are not relabeled as results on this binary.

Actual 2.1.7b IL shows IsUnitEnoughClose includes distance **and LOS**. The prior
exception incorrectly called every refusal “outside native attack reach”.
Compiled source `c40c2d552e4b4b92873caf661ece79a18839d508` records the separate
mechanics distance, radius, native LOS query, eye/target/approach positions,
creation receipt, path state and claims before retaining the same refusal.
Production compilation / 10 focused tests PASS; diagnostic native **NOT RUN**
and absent from the tested package. No mechanical fix or Wall acceptance claimed.

The same candidate's corrected motion phase passed 62 assertions, including all
seven generic movers, Lillend casting, Bearded Devil contacts and two-Kyton
isolation. Both transactions restored exact current v0.0.140 installation,
native settings and protected saves. No save loading/mutation occurred.

Owner approved ceiling 50 to 70, retaining 48.76; total now **69.34/70 minutes**.
Another build/native run does not fit the remaining allowance. The active overhaul
state records commands, durations, exact identities and the 65-frame illustrated
review. Next runtime work must first distinguish native distance from LOS on
a corrected candidate; do not override either guard or weaken the retained
damage/navigation/expiry assertions. PR #96 stays draft and Wall acceptance OPEN.
Earlier dated budget requests below are historical, not current approval state.

## Current 50-minute continuation: formed wall observed; witness drift blocks remainder

One candidate `55a6bd27dd40`, ZIP SHA-256
`7f68ceef848c2d7bd914d80e8ec20102d93859e38fa66e729e1dc754a0582822`,
ran corrected phase `summon-overhaul-wall` at
`20261105T1347234912998Z-disposable-expanded-summoning-sprint32-native-review`.
Result FAIL (20 assertions total), SHA-256
`294051eaf92b78b4b74021cf8edc8fc109a19a48307303c3177e020b0ba9ced9`.

The ordinary-camera formed-wall image was inspected: one visible plane.
Baseline/live native movers and actual detour passed again. The physical
witness was then observed at its old/other position, 17.92196m from the section,
despite idle/support predicates; native reach admission correctly refused an
attack. This run does not prove physical breach, exact cleanup, expiry or tail.
Original 36 HP/720-second creation and earlier controlled fire destruction /
return of Recast route remain credited to their original receipts.

The new compiled observer correction (included in 1994b436af / 09752d6ba3)
fully quiets disposable witnesses, stops pending native movement, places the
attacker after combat preparation and requires expected horizontal position
within 5cm as well as support. This is fixture preparation, not a mechanical
Wall repair or a new PASS. It requires a new candidate/native run. Strict
damage, grid and original-deadline controlled-expiry assertions are retained.

All protected installation/settings/saves were restored exactly. The approved
continuation used 48.76/50 minutes including conservative report QA. A further
20-minute allocation (50 to 70) is requested and pending. See active overhaul
state for exact commands, candidate identities, all four native results and
the 39-frame illustrated review. PR remains draft; full Wall acceptance open.

## Current second-review result: actual formation/obstruction, incomplete cleanup acceptance

Candidate `162d089e2636`, ZIP SHA-256
`2eb94b4d3569aa5eb190bff739ff02cbc782e08dc17addc289a7b2594493f16f`,
reached a legal ordinary cast in exact phase `summon-overhaul-wall`. Evidence
timestamp `20261105T0517083304769Z-disposable-expanded-summoning-sprint32-native-review`.
Result FAIL, 17 assertions total; result SHA-256
`615c066745059ed752566fd139ede37e2318d6e978bb03aaefe91b9c16abe3ed`.

Observed: native cast Success, one original 36-HP/720-second section, enabled
visible plane renderer, native cut and 16 GridGraph claims. Baseline and live
actual mover both passed with three stable grounded starting frames; the live
wall caused a measured detour. Controlled native fire damage destroyed the
wall and the Recast route returned. This is substantive functional evidence,
not a complete Wall PASS or a claim to have reproduced the owner's exact trigger.

The full viability cell failed: the physical attack did not start; the observer
required GridGraph index 1 to equal active path RecastGraph index 0; and final
snapshot walkability differed while adjacent test witnesses remained nearby.
Expiry and Bone tail were not reached. No gameplay repair is selected from
these observer failures. All protected intake/settings/saves were restored.

Follow-up `fa457ca668cfed7affd4347a18a98a40070fc012` grounds/settles exact
witnesses, clears witness occupancy before bare-grid inspection, recognizes
both supported path backends, retains strict physical/grid restoration checks,
adds diagnostic restoration rows and captures the formed wall early using the
ordinary camera. It compiles but has no candidate/native execution yet.
Controlled original-deadline expiry remains explicitly unrun and cannot be
described as a twelve-minute real-time soak.

Current review validation 27.04/30 minutes; prior ledgers retained. Additional
20-minute allocation request is pending before a new build/native sequence.
See the active overhaul state for exact commands, package/DLL identities,
four phase results and the illustrated evidence. PR remains draft.

## Latest continuation: observer failed before casting

The immutable development candidate `1dda3428e72f`, ZIP SHA-256
`bb9de770a6465286b86942471203e151340909ede3a5fcbd09f42414dc9ee24d`,
ran exact save-free phase `summon-overhaul-wall`. Evidence timestamp
`20261105T0329350156477Z-disposable-expanded-summoning-sprint32-native-review`
is under the assigned lab's runtime-evidence. Result FAIL: native Default was
admitted, then a nested setup wait reset its local waiting flag. The outer
poller attempted a forbidden manual clock pulse. **No Wall cast was reached.**
Result SHA-256: `c67de1a082f5880bb440d823edde49377fa1784b26969b7cdef214b4597826f2`.
The installation, native settings and protected saves were restored exactly.

Observer correction `45c90e2ac519b6c56f9d35d79457863be6edf393` prevents
manual pulses throughout an owned Default lease. Production compilation and
10 focused neighbors pass, but a new candidate/native execution is required.
The observer contains actual movement controls and original-720s-deadline
controlled expiry; neither executed. No gameplay repair is selected from an
observer failure, and feedback messages are not a Wall PASS.

The additional allowance used 28.10/30 minutes, retaining the earlier 26.80.
Another approximately 11-minute build plus native execution exceeds the
remaining 1.90 minutes. See the active overhaul state for all commands,
durations, exact identities and remaining acceptance. Earlier investigation
below is historical and does not override this failed result.

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

Final visual candidate `0ecccbf2c601c33ce250ac285bca1d3a529accb1` compiled and
passed strict development package validation. Its ZIP SHA-256 is
`56bef143018617b63943b5316019e47edacd8ba35220c5c5afa14dadfd540238`.
The cumulative instrumented validation reached 26.80 min; the next existing
native sequence was deferred because its additional 6-10 min would exceed the
30-minute owner policy ceiling. Consequently no Wall reproduction, functional
repair, actual obstruction or native expiry is claimed. See the
[active candidate record](SUMMON-OUTSIDER-VISUAL-OVERHAUL-STATE.md) for exact
artifact identities, complete timings and the prepared unrun guarded driver.

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
