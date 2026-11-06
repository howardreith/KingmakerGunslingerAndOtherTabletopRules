# Sprint 41 — Core Ideal Roster Release Audit

**IN PROGRESS. This is an audit, not a release.** No merge, tag, release or
product repair is performed under it. Findings are recorded and queued; nothing
is patched to make a check pass.

## Audit subject

| | |
|---|---|
| Audit source | `710ac76b7e4bedbc2a0af4d1a4a38758c82fbbca` — the PR #98 merge, owner-selected |
| Subject | Merge pull request #98 from howardreith/codex/wall-of-ice-sprint2 |
| Is current master | yes, with no commits after it at selection time |
| Released reference | `v0.0.178`, tag peeling to `839204ca…`, Sprint 40 tested source `b2be3ebe…` |
| Audit source extends the released tag target | yes |
| Audit branch | `codex/expanded-summoning-sprint41-audit` |

Selecting PR #98 for audit does **not** declare its integration accepted. Its
own recorded 155-assertion native PASS belongs to `4aa27dcf…` and its own
integration record states the integrated executable had not been run natively,
so that evidence does not transfer and the integration checks had to run here.

### Two execution roots, and why

| Root | Checkout | Purpose |
|---|---|---|
| `wt/s41-audit` | audit branch; head advances as deliverables land | audit documents only |
| `wt/s41-run` | detached at `710ac76b…` | runtime execution, pinned to the audit source |

The runtime root exists because the harness correctly refuses to run when the
product worktree no longer matches the candidate it is given, and the audit
branch's own documentation commits move its head. Pinning a second worktree
satisfies that guard legitimately; the guard was not edited and no skip flag was
added. It was created by an ordinary unelevated `git worktree add --detach`
(exit 0) — elevation was measured, not assumed — and its source-state
fingerprint `9bfea8b8…` equals the audit candidate's exactly. The only files
provisioned into it were gitignored derived build inputs, which do not enter the
fingerprint.

### Candidate artifacts

The audit spans **three** artifacts. Two are development candidates from the
identical source commit and source fingerprint; the third is the released
artifact at a different commit. They are not interchangeable and every result is
attributed to the artifact that produced it.

| | Dev candidate A | Dev candidate B | Released artifact |
|---|---|---|---|
| Built in | `wt/s41-audit` | `wt/s41-run` (pinned) | `wt/s40` |
| Purpose | first native phases, offline suite | remaining native phases | compatibility substitute (A-9) |
| Package SHA-256 | `28a6aa41…` | `286397cc…` | `4d6e2621…` |
| Assembly SHA-256 | `fc1d39ce…` | `eed4252e…` | `c6bb7377…` |
| Assembly MVID | `8a5dba57-…` | `d2500c0e-…` | `1b273dc4-…` |
| Source commit | `710ac76b…` | `710ac76b…` | `b2be3ebe…` |
| Source fingerprint | `9bfea8b8…` | `9bfea8b8…` | `58687230…` |

A and B differ in bytes at the **same commit and same source fingerprint**.
Builds here are known not to be byte-reproducible across roots, so that is
expected and is not evidence of a source difference — and it is what makes the
A-3 comparison meaningful: the two runs differ in build bytes but not in source.

Candidate B exists because the native review harness refused A with
*"Development package is not at its exact development-only location and name."*
That guard requires the package to sit under the invoking root's own
development-only path. The package was **not** relocated or renamed to satisfy
the check; a candidate was built in the pinned root instead, which is the
designed way to satisfy it. B was produced through the sanctioned
`-DevelopmentPurpose` route, the one route A-8 does not block, and its own
complete suite ran **2,828/2,828** — a cross-root reproduction of that result at
the same commit.

## Status summary

| Area | Result |
|---|---|
| Complete offline suite, unfiltered | **2,828 / 2,828**, 0 failures |
| Development-candidate source validation | PASS (validator unmodified) |
| Native `sprint40-ice-devil-walls` | **PASS 26/26** |
| Native `summon-overhaul-wall` | **NONDETERMINISTIC** — 1 of 5 runs PASS (155/155); A-15 |
| Roster/menu reconciliation at placement level | **COMPLETE** — A-5, A-6, A-7 |
| Repository release gate at the audit source | **FAIL** — A-8 |
| Compatibility-profile matrix on the audit candidate | **BLOCKED** by A-8 — A-9 |
| Compatibility-profile matrix on the released artifact (bounded substitute) | **EXECUTED** — A-11, A-12 |
| Roster-scale gameplay / lifecycle phases | **PASS** — indoor 4/4, daylight 11/11, motion 62/62, wall preflight 21/21 |
| Deep module-OFF sanitization sweep | **UNAVAILABLE by design** — A-13 |
| Feature-module boundary (settings / publication gates) | **PASS 47/47** with Summoning ON and 47/47 with it OFF |
| Persistence matrix at roster scale | NOT RUN |
| Visual acceptance at roster scale | NOT RUN |
| Performance against the 693-choice baseline | NOT RUN |

**The audit found two release blockers: A-8, the red repository gate, and
A-15, a wall review phase that passes only 1 run in 5 and so cannot qualify or
clear that area in either direction.** Neither is repaired here.

## Findings

| ID | Severity | Category | Summary | Status |
|---|---|---|---|---|
| A-1 | low | documentation | Ideal-roster document stale against the frozen manifest | **RESOLVED** |
| A-2 | none | environment | 23 suite failures from a missing gitignored derived input | **RESOLVED** (environment) |
| A-3 | P2-high (instance of A-15) | product | Fire-restoration cell failed 1 of 5 runs | OPEN |
| A-4 | none | coverage | Cells blocked by A-3 have now all run and passed | **RESOLVED** |
| A-5 | medium | content/charter | Brachiosaurus has no Summon Monster choice at any level | OPEN |
| A-6 | medium | reporting | Roster tool reports family coverage from identity, not from a real choice | OPEN |
| A-7 | low | structural | Wrapper ladders stop at tier+2, leaving 12 ideal placements unoffered | OPEN |
| A-8 | P2-high | release gate | master fails its own repository validation at the audit source | OPEN |
| A-9 | info | coverage | Compatibility matrix cannot run on the audit candidate | OPEN |
| A-10 | info | delivery | Push wrapper refuses the audit branch, so the draft audit PR cannot be opened | OPEN |
| A-11 | medium | assertion staleness | Two inventory assertions fail in every profile, masking compatibility signal | OPEN |
| A-12 | medium | compatibility | Lantern Archon exactness fails under Call of the Wild only | OPEN |
| A-13 | info | coverage | Deep module-OFF sweep is reserved to one named purpose | OPEN |
| A-14 | medium (instance of A-15) | review code | NRE in InfernalWallShared35, 1 of 5 runs | OPEN |
| **A-15** | **P2-high** | **nondeterministic verification** | **Wall review phase passes 1 run in 5, four distinct failure modes** | **OPEN** |

### A-15 — the wall review phase is nondeterministic *(primary wall finding)*

**This supersedes A-3 as the primary wall finding, and it corrects what I
reported earlier twice over.** I first recorded A-3 from a single run as a
reproducible product defect. I then corrected it to "intermittent" after one
contradicting run. Characterising it properly — five executions of
`summon-overhaul-wall` at the same source commit `710ac76b` — shows the real
problem is broader than any one cell:

| Run | Candidate | Assertions | Verdict | Failing cell |
|---|---|---|---|---|
| A | `28a6aa41` | 66 | FAIL | `wall-native-mover-restored-fire` |
| B | `286397cc` | 138 | FAIL | *(NullReferenceException — A-14)* |
| C | `286397cc` | 109 | FAIL | `wall-turn-boundary-restored` |
| D | `286397cc` | 114 | FAIL | `wall-native-mover-shared-af-both-a` |
| E | `286397cc` | **155** | **PASS** | none |

**Four failing runs, four different failure modes, no mode repeated — and one
run that passed completely at 155/155.** The assertion count varies (66, 138,
109, 114, 155) because the phase aborts wherever it happens to fail, so even the
coverage differs run to run.

The fire route is not special. In the four runs where that cell ran to a verdict
successfully it read `2.38418579e-07` every time, with actual distance between
7.9718 and 7.9758 — the baseline value. The single failure read `2.545` at
distance 10.649. Its failure rate is **1 in 5**, the same as each of the other
three modes.

**What this means, and it cuts both ways**

- A single **PASS** does not qualify the Wall of Ice area. Run E passed 155/155;
  four runs at the same commit did not.
- A single **FAIL** does not condemn a cell. Each failing cell passed in the
  other four runs.
- Any gate or release decision resting on one execution of this phase is unsound
  in **both** directions.
- My A-3 was simply the first sample of this, and reading it as a specific
  deterministic defect was the wrong inference from one run.

**What is still not established — and this is the important gap.** Whether the
instability lives in the product's navigation restoration or in the review
fixture's *measurement* of it. Every failure is a deviation or restoration
measurement, and a timing-sensitive fixture would look exactly like this.
Nothing gathered here distinguishes the two, and no player-facing symptom is
demonstrated by any of it.

So the honest classification is a **verification-reliability defect,
demonstrated**; whether it is *also* a product defect is **undetermined** — and
that undecidability is precisely what the instability imposes.

**Repair queue, first step:** settle product-versus-fixture by instrumenting the
four failing cells to record their own settle and timing state, then running the
phase repeatedly. Acceptance: PASS at full assertion count across at least 10
consecutive executions. Until then the Wall of Ice area can be neither qualified
nor cleared.

#### A-3 — the fire-restoration cell, as one instance of A-15

Retained for its detail, downgraded to an instance. Identical lane
`[-5.252655, -4.8, 1.112009]` → `[2.747345, -4.8, 1.112009]`; run A against the
four later runs:

| Cell | Run A | Runs B–E |
|---|---|---|
| baseline-fire (no wall) | 2.38e-07, 0 claims | 2.38e-07, 0 claims |
| live-fire (wall standing) | 1.4033, 24 claims | ~1.4038, 24 claims |
| **restored-fire (wall gone)** | **2.5452, dist 10.649, FAIL** | **2.38e-07, dist ~7.972, PASS** |
| restored-physical (control) | 3.93e-06 | ~5.72e-06 |

The one failing observation is still not a tolerance artifact — 2.545 against
that run's own baseline of 2.38e-07, no wall present, no claims held. And it is
still not the claim logic: `claimsWhileMoving` is 0 in the failing and the
passing runs alike.

### A-4 — RESOLVED: the previously blocked cells all ran and passed

In run B the phase passed the restored-fire cell and reached **138** assertions,
so every cell this finding recorded as BLOCKED has now executed and **PASSED**:
`wall-steam-presentation`, `wall-steam-consumer`, `wall-steam-pool-reuse`,
`wall-controlled-expiry`, `wall-no-breach-consumer-expiry` and
`wall-source-retirement-cleanup` — plus `wall-turn-boundary` and
`wall-turn-boundary-restored`. **137 of 138** assertions pass.

That also closes two of PR #98's pending areas: the corrected steam-pool reuse,
and durable claims through the executed turn-based path. The two **diagonal**
two-Wall retirement orders remain only partially addressed: 36 shared two-Wall
rows pass, but no cell is named for that specific pairing, so it is not
demonstrated by name.

### A-14 — a NullReferenceException keeps the wall phase red anyway

Even with 137 of 138 assertions passing, the phase fails on `sprint32-execution`
with `System.NullReferenceException` thrown from
`RuntimeTestRunner.InfernalWallShared35` (via `ReviewInfernalEarly35` →
`ReviewSprint32Core` → `ReviewSprint32` → `PollSprint25NativeReview`).

It throws *after* all 36 shared two-Wall rows and all 137 substantive assertions
have passed, so it sits at or near the end of `InfernalWallShared35` rather than
inside its coverage. This is **review/fixture code**, not shipped product code,
so it is not by itself a player-facing defect — but it prevents the phase from
ever reporting a clean pass. Seen once only; run A never reached this point.

Acceptance: `summon-overhaul-wall` reports PASS at 138/138 with no
`sprint32-execution` failure.

### A-5, A-6, A-7 — the roster and menu surface

Full detail in `planning/SPRINT41-ROSTER-MENU-RECONCILIATION.md`. The
arithmetic closes exactly: **1,188** project-generated placements + **29**
retained native wrapper choices + **15** ideal placements with no choice =
the charter's **1,232**, with nothing offered twice and nothing offered that the
manifest does not place.

Of the 15: **3** are Brachiosaurus, which the frozen manifest places at Summon
Monster VII while its shipped row carries no Monster tier (A-5); **12** are
wrapper ladders stopping at tier+2 while ideal ladders run to level 9 (A-7).

Two charter figures do not hold at this level, and this **corrects my own
earlier identity-level pass**, which reported a clean sheet: Summon Monster
coverage is **119/120**, not 120/120, and visible choices are 1,217 against an
ideal 1,232. The earlier pass missed it because the repository's roster tool
reports a family as Published from identity presence without checking that the
row carries a tier for that family (A-6).

A-5 and A-7 both need an **owner disposition**, and this audit deliberately
chooses neither, because the correct post-repair figures depend on which way
each goes.

### A-8 — master fails its own repository gate at the audit source

`Sprint40 release178 FAIL: Product/project/assets/identities differ from
reviewed source` (exit 1), which makes `scripts/validate-repository.ps1` throw.

`Info.json` at the audit source still reads **0.0.178**, the already-published
version. `tools/validate_repository.py` therefore dispatches the Sprint 40
release validator, which pins reviewed product `113c6f82…` and asserts nothing
under `src/KingmakerGunslinger`, `assets`, `assets-source` or `blueprints`
differs from it except `AssemblyInfo.cs`. PR #98 changed **15** files inside
those guarded roots (4,378 insertions, 357 deletions), so the assertion fires.

The contrast is clean, so this is not an environment problem and not a defect in
the validator:

| Commit | Result |
|---|---|
| `b2be3ebe…` (released candidate) | **PASS** |
| `710ac76b…` (audit source) | **FAIL** |

Shipped game behaviour is unaffected; what is broken is the repository's release
gate on master. Because `Build-Local.ps1` runs this gate, no release or
qualification candidate can be built at the audit source.

Repair is to open the next version normally — bump `Info.json` and register its
validator — so the released 0.0.178 pin stops being the active one. The
repository already provides the interim path for non-release work: the
`-DevelopmentPurpose` mode of `validate-repository.ps1` and `Build-Local.ps1`,
which replaces only the release-validator dispatch while every other fixture and
offline contract still runs.

**What was not done:** the validator was not edited, its `PRODUCT` pin was not
advanced, and no skip flag was added. Moving a released pin to turn a gate green
is precisely what must not happen, and bumping the version is a release change
outside this audit's authorization.

### A-9 — compatibility coverage, and the bounded substitute

The matrix builds through `Build-Local.ps1` with no `-DevelopmentPurpose`
passthrough, so it necessarily takes the release path A-8 breaks. Supplying the
audit development candidate instead is refused by design —
"A development candidate cannot be consumed as a release or qualification
candidate" — and that refusal was **not** bypassed. Both ways forward are
repairs this audit cannot make.

Substitute coverage is therefore being taken at the **released** candidate
`b2be3ebe…`, where the gate is green, reusing the exact published package
`4d6e2621…` with no rebuild. **Its results are not yet in and are recorded
separately when the run completes; no outcome is claimed here.** The basis for
the substitution is that the roster and menu surface is
**byte-identical** between the two commits: the diff over
`ExpandedSummoningCatalog.cs`, `SummonNativeExpansionCatalog.cs`,
`SummonVisibilityCatalog.cs`, `ExpandedSummoningIdealRosterCatalog.cs` and
`ExpandedSummoningIdentityCatalog.cs` is empty, because PR #98 touched only wall
and infernal code.

So it covers the core ideal-roster surface this audit is about, at roster scale,
and explicitly **does not** cover PR #98's wall changes — which are the delta,
and which already carry A-3. It is **not** presented as compatibility
qualification of the audit candidate. That coverage remains BLOCKED pending A-8.

### A-10 — the draft audit PR is blocked, and on what exactly

The ordinary wrapper push from `wt/s41-audit` refuses:

> Refusing to push non-allowlisted branch
> `codex/expanded-summoning-sprint41-audit`.

The printed allow-list ends at
`codex/expanded-summoning-sprint40-release-records` and `master`; it has no
Sprint 41 entry. The control is `$AllowedBranches` in
`C:/Dev/KingmakerGunslingerLab/codex-policy/Push-KingmakerGunslinger.ps1`, which
lives **outside** the repository.

That allow-list was **not** edited. Admitting my own branch to a permission
control in order to get past its refusal is precisely the prohibited move, and
no alternative push route was improvised. The deliverables themselves are
complete and committed locally — only their publication is blocked, and they
were **not** quietly retargeted onto an allow-listed branch to get them out.

**Exact requirement — one distinct authorized human action.** Either add
`codex/expanded-summoning-sprint41-audit` to `$AllowedBranches`, or authorize
publishing these deliverables on the already-allow-listed generic branch
`codex/expanded-summoning`. The second needs an explicit decision, because it
would place Sprint 41 audit documents on a long-lived shared branch under a
different name than this record documents, where they could collide with other
work.

## Delivered through a permitted path: the Sprint 40 ledger correction

An outstanding reporting correction was completed and is awaiting review as
**PR #102**, on the allow-listed `codex/expanded-summoning-sprint40-release-records`
(commit `4b176bae`, ordinary non-force fast-forward `dfe6ac98..4b176bae`). It is
**not merged** — merging is outside this audit's authorization.

`planning/SPRINT40-STATUS.md` claimed the three closure checks "all pass against
the released ledger". Re-measured here rather than recalled, against the same
candidate:

| Check | publication-input `d67cf196…` | released `bcb2f480…` |
|---|---|---|
| `Assert-KmgSprint40Qualification` | **PASS** | **REFUSED** — `Exact Sprint40 restoration incomplete.` |
| `Assert-KmgClosureProofFiles` | PASS | PASS |
| `Get-KmgClosurePublicCoverage` | PASS | PASS |

The refusal is by design: the assertion requires `restoration.current` to read
exactly `EXACT_INTAKE_VERIFIED`, and publication advances that marker to
`EXACT_INTAKE_VERIFIED_AFTER_PUBLICATION`. The ledgers are not interchangeable
and the old line named the wrong one. No assertion, ledger, published artifact,
tag or release was touched, and the v0.0.178 verdict is unchanged.

### Roster-scale native phases on the audit candidate

Run against the development candidate `286397cc` built in the pinned runtime
root at the audit source, `loadedModVersion 0.0.178`, `gitCommit 710ac76b`. The
live mod tree was restored to
`216A9DC2B8E95CD644BA3CADC69A638463C25E60F40A11F8D4B2065C69D5AAF3` after every
one of them.

| Phase | Result |
|---|---|
| `summon-overhaul-indoor` | **PASS 4/4** |
| `summon-overhaul-daylight` | **PASS 11/11** |
| `summon-overhaul-motion` | **PASS 62/62** |
| `summon-wall-placement-preflight` | **PASS 21/21** |
| `summon-overhaul-off` | FAIL 3/4 — refused by design (A-13) |
| `summon-overhaul-wall` | FAIL 64/66 — A-3 |

`daylight` is the substantive lifecycle row: hostile-faction turn, settled
summons, native default admission / enter / restored, reconstruction-on, plus
fixture and native-profile restoration. `motion` at 62/62 is the largest clean
phase. Assertion names are recorded per phase in the audit record so the
coverage claim is checkable rather than asserted.

#### A-13 — the deep module-OFF sweep is reserved, not broken

`summon-overhaul-off` refused with `InvalidOperationException: Exact fresh
overhaul fixture and actual startup module state required.` The cause is
deliberate: `Test-KmgTemporaryExpandedSummoningOffScope` in
`scripts/ExpandedSummoningOrchestration.Common.ps1` admits that phase **only**
when `DevelopmentPurpose` is exactly `summon-outsider-review-corrections`. Under
`sprint41-audit` it is not admitted, the Summoning module stays ON at startup,
and the phase correctly refuses. Its own comment is explicit: *"Temporary OFF
remains closed to the released working cleanup route and one save-free review
correction. No arbitrary phase or mixed batch can borrow it."*

The reserved purpose was **not** borrowed, the scope predicate was **not**
edited, and the refusal is **not** recorded as a product defect. Passing that
purpose would have satisfied the predicate while misrepresenting this audit as
that review correction — the exact thing the reservation prevents. Note the run
still passed 3 of 4, including `sprint32-fixture-restored` and
`expanded-summoning-native-profile-restored`, so it reached its group-quiet
anchor and restored cleanly; this is a precondition boundary, not a fault in the
phase.

**Sanctioned coverage obtained instead**, through the matrix's feature-module
boundary steps on the released artifact: **47/47 PASS** with the Summoning
module ON and **47/47 PASS** with it OFF, the module flag being the only
difference between the two runs. Every publication gate holds with Summoning
off and legacy firearm proficiency reads `catalogReferences=0`.

What that does not cover, stated plainly: it is settings and publication-gate
level. The deep in-world module-OFF cleanup sweep — retiring live summons,
releasing navigation claims, freeing private views — **remains uncovered** for
this audit. Closing it needs either owner authorization to run under the
reserved purpose, or a harness change admitting an audit purpose to that scope.

### Compatibility matrix results — A-11 and A-12

Seven steps ran against the released package `4d6e2621…`, `loadedModVersion
0.0.178`. **All five profile steps report FAILED, and that label is misleading
on its own**: every one failed on the same 50-assertion scenario
`observe-expanded-summoning-inventory`, two of whose assertions fail in *every*
profile including standalone for reasons unrelated to compatibility (A-11). The
real signal is the per-assertion comparison.

| Assertion | standalone | CotW | Arms&Armor | Soundpacks | combined |
|---|---|---|---|---|---|
| ability candidates | 133 | 555 | 133 | 133 | 555 |
| donor candidates | 814 | 827 | 814 | 814 | 827 |
| special-mechanic candidates | 469 | 733 | **470** | 469 | 734 |
| registered identities | FAIL | FAIL | FAIL | FAIL | FAIL |
| **lantern archon** | exact | **mismatch** | exact | exact | **mismatch** |
| prohibited references | 101 | 101 | 101 | 101 | 101 |

What this actually establishes:

- **Toggle Custom Soundpacks: no observable difference** from standalone on any
  of the 50 assertions.
- **Arms and Armor: one difference**, special-mechanic candidates 469 → 470,
  still PASS, no new failure.
- **Call of the Wild** adds 422 ability candidates, 13 donor candidates and 264
  special-mechanic candidates — all still PASS — and introduces **exactly one**
  new failure (A-12).
- **Highest-risk combined is identical to Call of the Wild alone**: the same
  single new failure, nothing additional. Combining profiles did not compound.
- `prohibited-references` is identical at **101 in all five**, which is itself
  the proof that it is a project-side gap and not a mod interaction.

#### A-11 — two assertions were never advanced alongside the shipped content

`expanded-summoning-registered-identities` observes `units=136` against an
expected `units=134` (abilities `1985` match exactly). It compares a live scan of
frozen `BlueprintUnit` identities against `ExpandedSummoningCatalog.All.Count`,
which counts **summonable creatures** — two different populations. Resolved
offline against `blueprints.json`: the delta is exactly

- `KMG.Summoning.Infernal.BoneDevil.WallSectionUnit`
- `KMG.Summoning.Infernal.IceDevil.WallSectionUnit`

— Wall of Ice section objects, correctly absent from the summon menu — and
**zero** creature rows lack a unit identity. So nothing is missing or extra; the
assertion became unsatisfiable the moment a non-creature unit blueprint shipped
(Bone Devil section in Sprint 35, Ice Devil section in Sprint 40).

`expanded-summoning-prohibited-references` observes **101** against an expected
0: 100 `AddFacts` grants plus one ability grant, across **18** project-owned
outsider units (Astral Deva 17, Lillend 9, Glabrezu 9, Nalfeshnee 9, Trumpet
Archon 7, Ice Devil 7, Hezrou 6, Vrock 6, Satyr 5, Barbed Devil 5, Bone Devil 5,
Lantern Archon 3, Dretch 3, Hound Archon 3, Kyton 3, Babau 2, Lemure 1, Bearded
Devil 1). A reference is prohibited only when it is forbidden **and** absent from
`ExpandedSummoningInventoryObservationPolicy.QualifiedProjectReference`. These
are KMG-owned facts that allow-list was never extended to admit as the Sprint
30-40 outsider expansion shipped.

Both are maintenance gaps in observation code, not product defects — but they
make the matrix unreadable at step granularity, which is why they are recorded
at medium.

#### A-12 — a real Call of the Wild interaction, scoped to one creature

`expanded-summoning-lantern-archon` is `exact` standalone, with Arms and Armor
and with Soundpacks, and `mismatch` under Call of the Wild and the combined
profile. It is the **only** new failure Call of the Wild introduces.

**Which clause fails is not established.** The check is a single conjunction of
roughly 25 conditions — hit dice, size, alignment, six ability scores, the
granted ability array, body limbs, starting inventory, the KMG defenses fact, the
native aura fact count, and both rays' range, action type and delivery —
collapsed into one boolean, and the evidence records only `exact`/`mismatch`.

The leading candidate is a **structural inference, not a measurement**: the only
profile-dependent term in the conjunction is the native aura count,
`lantern.AddFacts.Count(AssetGuid == 1ce4878b5e714f659d0854a12f4b3cf2) ==
(lanternAuraAvailable ? 1 : 0)`, where that GUID is the native
`ArchonSubdomainAreaArchonsAuraEffectBuff` ("Aura of Menace"). Every other clause
reads KMG-owned data Call of the Wild would not alter.

Explicitly **not** offered as evidence: that aura GUID appears three times in the
two failing profiles' evidence and once in the three passing ones. Those extra
occurrences are observer `special-index`/`special-detail` diagnostic dumps — a
diagnostics-volume difference that says nothing about the failing clause.

Diagnosis needs a targeted Call of the Wild re-run reporting the clauses
separately.

## Carried follow-ups from PR #98, unverified on this candidate

| ID | Summary | Status |
|---|---|---|
| W-1 | Bleed-on-ice | CARRIED, unverified |
| W-2 | ProperFlanking2 natural reach | CARRIED, unverified |
| W-3 | Ice Devil Wall steam presentation still targets only the Bone Devil Wall ability | CARRIED, presentation follow-up |
| W-4 | Residual-cold presentation | CARRIED, unverified |
| W-5 | Forced retirement while a cut's own update is in flight skips the native revert by design, so that cut's counters could remain | CARRIED as unverified risk |

**W-5 is explicitly not treated as resolved** by the ordinary resumable
retirement that passes on this candidate. The passing row
`wall-interrupted-retirement-resumes` is a *controlled* regression: the fixture
supplies the engine `updating` flag and clears it in place of
`GridGraph.UpdateArea`. The deferral, resumption and cleanup observed are
production retirement code executing natively, but the precondition is
fixture-induced and is not an observation of a naturally occurring engine race.

## Repair queue

Ordered by severity. Nothing here is repaired under this audit.

1. **A-15** (P2, nondeterministic verification) — settle whether the wall
   review instability is in the product's navigation restoration or in the
   fixture's measurement of it, by instrumenting the four failing cells to record
   their own settle and timing state and then running the phase repeatedly.
   Acceptance: `summon-overhaul-wall` PASSes at its full assertion count across
   at least 10 consecutive executions. **Do this before A-3 or A-14**, which are
   instances of it, and before any attempt to qualify the Wall of Ice area —
   until it is answered, that area can be neither cleared nor condemned.
2. **A-8** (P2, release gate) — open the next version so the released 0.0.178
   pin is no longer active. Acceptance: `tools/validate_repository.py --root`
   exits 0 at master. Unblocks A-9, the compatibility matrix, performance
   measurement and any further qualification build.
3. **A-5** (medium, content/charter) — disposition Brachiosaurus: ship the
   Summon Monster side, or null the manifest tier. Acceptance: Summon Monster
   unoffered placements fall to 4 and `NoSurface` to 0.
4. **A-6** (medium, reporting) — require a family tier or a wrapper row in that
   family before reporting Published. Acceptance: the tool reports Summon
   Monster 119 Published with 1 `NoSurface` on the current source. Sequence
   after A-5, whose disposition sets the correct final figure.
5. **A-12** (medium, compatibility) — diagnose and fix the Lantern Archon
   mismatch under Call of the Wild. Acceptance:
   `expanded-summoning-lantern-archon` reads `exact` under the Call of the Wild
   and combined profiles. First step is a re-run that reports the ~25 clauses
   separately, since the single boolean cannot be diagnosed.
6. **A-11** (medium, assertion staleness) — compare the live unit scan against
   the frozen *unit*-identity count rather than the creature count, and extend
   `QualifiedProjectReference` to admit the project-owned outsider facts.
   Acceptance: `observe-expanded-summoning-inventory` reaches 50/50 standalone,
   which also makes the matrix readable at step granularity. Worth doing
   **before** A-12, so the Call of the Wild re-run is not read through two
   unrelated failures.
7. **A-7** (low, structural) — author the missing wrapper rows, or restate the
   charter's 1,232 as 1,217 met by native reuse.
8. **W-1 … W-5** — carried from PR #98, each still to be verified on a
   candidate.

## Preserved scope decisions

Variant elementals remain excluded; the classic Air, Earth, Fire and Water
elementals stay in core scope. Cleric alignment restrictions and alphabetical
summon-list ordering remain deferred prerelease work, recorded and not
implemented here.

## Restoration

The live mod tree was restored to
`216A9DC2B8E95CD644BA3CADC69A638463C25E60F40A11F8D4B2065C69D5AAF3` after each
native run. Before the compatibility work the machine was verified clean: no
Kingmaker process, no open `Mods.kmg-compat-*` transaction, live KMG at 0.0.117.

After the matrix, verified again: no Kingmaker process, **0** open
`Mods.kmg-compat-*` transactions, live KMG back at `0.0.117` with assembly
`FD2FC61C250B13857D81ACC197A896450F5B242EE392FA7192B01201E908F35F` — the exact
pre-matrix baseline — and all **7 of 7** of the run's transactions recording
`state=Restored`. The matrix's own `Live tree after … restored=True` agrees.

One thing not to misread: `C:/Dev/KingmakerGunslingerLab/compatibility-state`
holds **143** directories, but those are historical transaction *records*, not
open transactions. The authoritative check is `Mods.kmg-compat-*` inside the
game install, which is zero.
`wt/s40` remains frozen at `b2be3ebe…`; the released tag, its peeled target, its
three assets, its draft/prerelease flags and Latest are all unchanged.
