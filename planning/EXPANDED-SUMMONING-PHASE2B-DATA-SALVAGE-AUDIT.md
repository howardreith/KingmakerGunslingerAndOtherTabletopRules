# Read-only DATA salvage audit, 2026-10-05

## Boundary and exact comparison

Performed only after the laptop's coherent restored NOT QUALIFIED checkpoint
`9cd5cdc7c31cd2914e69eff1f889b95fabdbdc90` was policy-pushed. No production
source, tests or diagnostics were imported during this audit. The active line
remains PR #26 / `codex/expanded-summoning-phase2b-sprints14-17`.
PR #27 / `codex/expanded-summoning-phase2b-finalize-20261005` is archived
accidental concurrent work, retained open/draft as salvage evidence.

The owner's exact commands were run without modifying either comparison ref:

```text
git range-diff 3fca5aba1cdaaf6215fd5712409fc208855c79cf..bc36f97612020d6d680144bcb6ebc01bebe11216 3fca5aba1cdaaf6215fd5712409fc208855c79cf..8d12d73bc7c66be81b83d80992e3141c7d072f22
git diff --stat bc36f97612020d6d680144bcb6ebc01bebe11216 8d12d73bc7c66be81b83d80992e3141c7d072f22
```

Common ancestor is exactly `3fca5aba1cdaaf6215fd5712409fc208855c79cf`.
Range-diff pairs none of the commits: laptop
`31c9ff0f4949bfda5db21225366f52d110953d13` and
`bc36f97612020d6d680144bcb6ebc01bebe11216`; archive
`379ff53846c4f77fab993b36a223fee11be01151` and
`8d12d73bc7c66be81b83d80992e3141c7d072f22`.
`git cherry` marks both archive commits unique, not safely additive.
Tip-to-tip stat: **29 files, 757 insertions, 409 deletions**. Per-file/hunk
diffs, both commit identities, and the archived c505 reconciliation ledger were
read. An apparent deletion in tip-to-tip diff can be a laptop-only addition;
it is not a request to remove it. No file was selected by modification date.

Abbreviations below: Runtime = `src/KingmakerGunslinger/RuntimeTesting/`;
Summoning = `src/KingmakerGunslinger/Summoning/`;
Tests = `tests/KingmakerGunslinger.DomainTests/`.
Runtime rows use the `RuntimeTestRunner.` prefix unless otherwise stated.
Every differing file is classified; mixed files identify each distinct hunk.

## Documentation and governance (11 files)

All rows are **DOCUMENTATION-ONLY — retain on archive; do not port**.
Current laptop state is independently recorded from the owner's latest order
and its own exact runtime artifact. Archive authority/receipt/count/PASS claims
cannot govern it.

| File | Archive-only content / disposition |
| --- | --- |
| EXPANDED-SUMMONING-PHASE2-AUTONOMOUS-STATE.md | Canonical-finalization header, old import tip, ownership receipt, stale save/candidate and PR authority. Superseded by latest owner directive; never import. |
| EXPANDED-SUMMONING-PHASE2-EVIDENCE-INDEX.md | Archive ledger/current-evidence pointers replace laptop fourth/fifth batch pointers. Preserve each line's evidence in place; do not erase laptop history. |
| EXPANDED-SUMMONING-PHASE2-IMPLEMENTATION-REPORT.md | Archive current status/mission language; no transferable qualification. |
| EXPANDED-SUMMONING-PHASE2-JOURNAL.md | Archive creation/reconciliation and old ownership narrative; retain historical evidence there. |
| EXPANDED-SUMMONING-PROGRAM-STATE.json | Archive tranche branch, authority, receipt ancestry, 9-21/Phase 2C deferral, old counts/status; laptop independently records the latest owner-authorized true/deferred state. Do not drop laptop fourth/fifth batch records. |
| planning/EXPANDED-SUMMONING-FIDELITY-MATRIX.md | Archive partial qualification and historical candidate pointers; no imported fidelity claims. |
| planning/EXPANDED-SUMMONING-PHASE2B-FINALIZATION-RECONCILIATION.md | 188-line archived c505/2d per-hunk ledger and old canonical ownership; read as evidence, not instructions. c505/2d and verified bundle remain preserved. |
| planning/EXPANDED-SUMMONING-SPRINT16-HIDDEN-CANDIDATE-REVIEW.md | 110 added lines for local 390 failure/hashes/diagnoses; retain at archive SHA, not current proof. Some proposed corrections were explicitly not retained even there. |
| planning/EXPANDED-SUMMONING-SPRINT16-PRE-CANDIDATE-REVIEW.md | Old canonical candidate/mission/normalization header; no port. |
| planning/EXPANDED-SUMMONING-SPRINT16-FOURTH-RECONCILED-BATCH-REVIEW.md | Absent on archive because this is a laptop-only later record, not an intentional deletion to adopt. Keep on laptop. |
| planning/EXPANDED-SUMMONING-SPRINT16-FIFTH-RECONCILED-BATCH-REVIEW.md | Same: preserve the complete laptop failure/restoration record. |

## Request diagnostics (5 files)

All are **USEFUL TEST/DIAGNOSTIC — no port in this audit**.

| File | Every diagnostic hunk |
| --- | --- |
| scripts/Invoke-KingmakerRuntimeTest.ps1 | Optional typed mechanics/combat/lifecycle scope, exact working save, automatic exit required. |
| scripts/RuntimeAutomation.Common.ps1 | Matching allowlist, parameter-count branch and serialization. These three edits must remain one closed contract if reproduced later. |
| scripts/Test-ExpandedSummoningWorkingSavePersistence.ps1 | Three diagnostic round trips and eight rejection cases. Independent of the already-present persistence scope checks. |
| Runtime/RuntimeTestRequest.cs | In-game allowlist plus corresponding parameter-count branch; not authorization to expand other scenarios. |
| Runtime/ExpandedSummoningSprint16.cs | Optional diagnostic routing and explicitly nonqualification completion warning. Full default still runs all stages. |

A narrower diagnostic request could reduce investigation time, but no subset
replaces the required full exact-artifact batch. Any adoption must be manually
reproduced on a new short-lived audit branch, with paired script/C# rejection
tests, not cherry-picked. No such branch or adoption was performed here.
The current policy allowlist does not yet include a new audit branch; do not
bypass it or reuse an unrelated allowed branch. This optional salvage path
does not block laptop fixes based on its own failed runtime evidence.

## Runtime fixture differences (4 files)

| File / hunk | Classification and disposition |
| --- | --- |
| Runtime/ExpandedSummoningSprint16Combat.cs: parent type, capital parent setup/restoration | **DUPLICATE/SUPERSEDED**. Laptop already has the bounded capital path plus the now-proven noncapital faction path and original UnitReference restoration. Keep one laptop implementation, not both. |
| Same: removed faction state/restore and before/after control diagnostics | **DUPLICATE/SUPERSEDED**. Archive predates bc36 repair; adopting this diff would regress the native control predicate outside capital. Keep laptop code/observations. |
| Same: exact Summoner guard/predicate and capital-parent diagnostics | **USEFUL TEST/DIAGNOSTIC** only. Laptop already records retained Summoner; an additional assertion could be independently reproduced on an audit branch. Do not replace the functioning control path. |
| Runtime/ExpandedSummoningSprint16Lifecycle.cs | **USEFUL TEST/DIAGNOSTIC**: dead/turn-based/clock/turn-start/deadline observations. Archive-only instrumentation is not qualification. No hunk copied. |
| Runtime/ExpandedSummoningSprint16Mechanics.cs: session reset helper + invocation | **USEFUL TEST/DIAGNOSTIC**: double explicit-cohort production reset with separate Purple Worm hold control. Does not replace fresh-load persistence and cannot narrow production all-owned cleanup. |
| Same: visual weapon-binding assertion replacing contact-carrier assertion | **DUPLICATE/SUPERSEDED** for fact ownership; laptop actual contact carrier now executes. **USEFUL TEST/DIAGNOSTIC** for exact bite/tail identity assertion, but do not remove the stronger attached-carrier check. |
| Same: Animal Growth before/after physical oracle and exact expected dice | **USEFUL TEST/DIAGNOSTIC**: stronger independent oracle; useful future audit-branch test. Preserve the laptop native-versus-explicit dice correction. |
| Same: creature/item/rule size fields | **USEFUL TEST/DIAGNOSTIC**: additional observation, no product effect. |
| Same: lethal before/after-life counts and dead replay | **USEFUL TEST/DIAGNOSTIC**: stronger no-second-check/no-second-damage test. No port without independent reproduction. |
| Runtime/ExpandedSummoningSprint16Ui.cs | **USEFUL TEST/DIAGNOSTIC**: diagnostic-scope completion and section before/after fields. It does not fix the laptop's selection/clock restore failure. |

## Product, tests and validators (9 files)

| File / hunk | Classification and explicit comparison |
| --- | --- |
| Summoning/CrocodilianAttackVisualPose.cs | **DUPLICATE/SUPERSEDED**. Archive extracts ResolveWeaponStats; laptop already selects the exact owning combat-trait component from declared AddFacts using SingleOrDefault. Same owner/weapon binding, different helper shape. Keep laptop implementation only. |
| Summoning/CrocodilianVisualPolicy.cs: SelectWeaponStats | **DUPLICATE/SUPERSEDED** production helper; extra testability is **USEFUL TEST/DIAGNOSTIC**. Do not retain two binding resolvers. |
| Summoning/CrocodilianRulesPolicy.cs: ResolveWeaponSize | **PRODUCTION BEHAVIOR**, no port. Laptop clamps result to 2..8; archive validates all inputs 0..8 and clamps to 0..8. Example body=2/item=4/calculated=3 gives laptop 2 vs archive 1; invalid 9 is clamped by laptop but throws on archive. Ordinary Large/Gargantuan and one-step Animal Growth cases coincide. Fine/Diminutive handling needs an explicit supported-configuration test and fresh runtime qualification before consideration; not assumed safe from source PASS. |
| Summoning/ExpandedSummoningPteranodonViewPatch.cs: two rollback predicates | **PRODUCTION BEHAVIOR**, no port. Archive excludes any candidate reference already in OriginalMaterials before adding name-matching rollback clones. Paths differ only for aliasing of those exact original references. Normal freshly cloned crocodilian materials remain candidates in both. A controlled alias/rollback fixture and fresh resource-lifetime qualification are required before adopting the exclusion; do not import a newer whole shared visual file. |
| Tests/ExpandedSummoningCrocodilianRulesTests.cs: live size test and Tiny-minus-one expected 1 | **USEFUL TEST/DIAGNOSTIC**, coupled to the above production choice. Do not change the expectation solely to make the present clamp pass/fail; establish supported native size behavior first. |
| Tests/ExpandedSummoningSprint16Tests.cs: mixed/foreign/duplicate fact cases | **USEFUL TEST/DIAGNOSTIC**. Could independently test the one retained laptop lookup; do not import the archive helper just to satisfy these tests. |
| Tests/Program.cs | **USEFUL TEST/DIAGNOSTIC** registration only, coupled to a not-yet-adopted test; do not increase test count without it. |
| tools/validate_expanded_summoning_phase2a141.py | **DUPLICATE/SUPERSEDED** count pin 2019 (laptop remains 2018 until its own added test); **DOCUMENTATION-ONLY** authority/deferral schema coupling, not a reason to port archive authority. Current owner requirements are independently controlling. |
| validation/static-validation.json | **DOCUMENTATION-ONLY** archive branch/mission/range/deferral fields; no port. **DUPLICATE/SUPERSEDED** count 2019 without its test; preserve current source-derived count. |

## Outcome

Zero cherry-picks, merges, source copies, branch-authority imports or competing
visual/control implementations. No frozen-branch push. No qualification claim
transferred between artifacts. Archive files and safety refs remain intact.

The laptop bc36 run proves the required noncapital native-faction repair and
all eighteen combat cells now execute; it still fails contact and fixture gates.
Continue its demonstrated corrections on the active line, with a new immutable
candidate and complete qualification. Do not start Sprint 17 yet.

After committing/policy-pushing this audit, PR #26's top notice must identify
the owner-designated canonical Phase 2B line and PR #27 as the archived accidental
concurrent branch. Keep both PRs draft/open/unmerged and retain their history.

