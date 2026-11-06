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

The audit spans two artifacts from the **identical source commit and source
fingerprint**. They are not interchangeable and each result is attributed to the
one that produced it.

| | Development candidate | Released artifact |
|---|---|---|
| Purpose | native review phases, offline suite | compatibility substitute (see A-9) |
| Package SHA-256 | `28a6aa41…` | `4d6e2621…` |
| Assembly SHA-256 | `fc1d39ce…` | `c6bb7377…` |
| Assembly MVID | `8a5dba57-b9df-4afc-87fa-ac69f89ca68f` | `1b273dc4-236e-4f85-9395-af2ec00fae95` |
| Source commit | `710ac76b…` | `b2be3ebe…` |

Builds in this repository are known not to be byte-reproducible across roots, so
differing package hashes are expected and are not evidence of a source
difference.

## Status summary

| Area | Result |
|---|---|
| Complete offline suite, unfiltered | **2,828 / 2,828**, 0 failures |
| Development-candidate source validation | PASS (validator unmodified) |
| Native `sprint40-ice-devil-walls` | **PASS 26/26** |
| Native `summon-overhaul-wall` | **FAIL 64/66** — A-3 |
| Roster/menu reconciliation at placement level | **COMPLETE** — A-5, A-6, A-7 |
| Repository release gate at the audit source | **FAIL** — A-8 |
| Compatibility-profile matrix on the audit candidate | **BLOCKED** by A-8 — A-9 |
| Compatibility-profile matrix on the released artifact (bounded substitute) | **EXECUTED** — A-11, A-12 |
| Gameplay / lifecycle / sanitization / persistence matrix | NOT RUN |
| Visual acceptance at roster scale | NOT RUN |
| Performance against the 693-choice baseline | NOT RUN |

**The audit has found two independent release blockers (A-3, A-8).** Neither is
repaired here.

## Findings

| ID | Severity | Category | Summary | Status |
|---|---|---|---|---|
| A-1 | low | documentation | Ideal-roster document stale against the frozen manifest | **RESOLVED** |
| A-2 | none | environment | 23 suite failures from a missing gitignored derived input | **RESOLVED** (environment) |
| A-3 | P2-high | product | Fire-destroyed Wall leaves the lane obstructed with no claims held | OPEN |
| A-4 | info | coverage | Later wall cells never executed because the phase aborted at A-3 | OPEN |
| A-5 | medium | content/charter | Brachiosaurus has no Summon Monster choice at any level | OPEN |
| A-6 | medium | reporting | Roster tool reports family coverage from identity, not from a real choice | OPEN |
| A-7 | low | structural | Wrapper ladders stop at tier+2, leaving 12 ideal placements unoffered | OPEN |
| A-8 | P2-high | release gate | master fails its own repository validation at the audit source | OPEN |
| A-9 | info | coverage | Compatibility matrix cannot run on the audit candidate | OPEN |
| A-10 | info | delivery | Push wrapper refuses the audit branch, so the draft audit PR cannot be opened | OPEN |
| A-11 | medium | assertion staleness | Two inventory assertions fail in every profile, masking compatibility signal | OPEN |
| A-12 | medium | compatibility | Lantern Archon exactness fails under Call of the Wild only | OPEN |

### A-3 — a fire-destroyed Wall of Ice leaves the lane obstructed

Failing assertion `sprint32-sprint35-wall-native-mover-restored-fire` in
`summon-overhaul-wall`, runId `20261106T1901224190003Z-dfbd14490f5248a48489c4a038d28ef5`.

Three cells on one **identical** lane, start `[-5.252655, -4.8, 1.112009]` to
goal `[2.747345, -4.8, 1.112009]`:

| Cell | Wall present | Max deviation | Claims while moving | Result |
|---|---|---|---|---|
| baseline, before the wall | no | **0.0** | 0 | PASS |
| live, wall standing | yes | 1.4033 | 24 | PASS |
| restored, after fire destruction | **no** | **2.5452** | **0** | **FAIL** |

After the wall is gone the mover still detours *further* than it did while the
wall stood — actual path 10.649 against a direct 8.0 — while holding no
navigation claims at all.

What this establishes: the node-level assertion
`wall-navigation-restored-fire` **passed** immediately before, so the node bits
and claim counts it checks are restored while actual pathing is not; and the
equivalent physical-destruction sequence returns its own identical lane to 0.0,
so the two destruction routes genuinely differ. The mover pathed on
`Pathfinding.RecastGraph`, not the GridGraph that carries the wall's 24 node
claims.

What it does not establish: how long the obstruction persists, whether an area
transition or re-tiling clears it, and the mechanism. Incomplete recast-tile
restoration specific to the fire route is the leading hypothesis from the graph
selection and the physical-route contrast — it is not a confirmed cause.

This is **not** the wall-retirement claim logic: claims are 0, and the Sprint 40
interrupted-retirement and claim-restoration rows pass on this same candidate.

Smallest acceptance check: that cell reaches the baseline 0.0 deviation with 0
claims on the same lane.

### A-4 — what A-3 blocked

The phase aborted at the throw, so these never ran and are **BLOCKED**, not
skipped by choice: `wall-steam-presentation`, `wall-steam-consumer`,
`wall-steam-pool-reuse`, `wall-controlled-expiry`, `wall-no-breach-consumer-*`,
`wall-source-retirement-cleanup`.

Ordering evidence: `RuntimeTestRunner.InfernalWallAcceptance35.cs` calls
`InfernalWallRestored35(fire)` at line 184, before `InfernalWallSteamExit35`
(187) and the controlled-expiry call (198); `wall-source-retirement-cleanup`
(1175) and `wall-steam-pool-reuse` (1190) are later still.

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

1. **A-3** (P2, product) — fire-destruction restoration of the recast navmesh.
   Acceptance: `sprint32-sprint35-wall-native-mover-restored-fire` reaches
   baseline 0.0 deviation with 0 claims on the same lane. Unblocks A-4's six
   cells.
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
