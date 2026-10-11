## Approved default-branch integration - October 11, 2026

The owner approved merging the workflow changes. Integration branch
`codex/workflow-reset-master-2026-10-11` applies review commit
`bfb9192f7b725b26d712c230af836b59b75d3bdf` onto master
`d0ceafae75c1d74a1e0d4fa318193ef002240da1`. Product source, assets and version
remain master's 0.0.149. The 28 intervening Sprint 21 commits remain on their
existing branch. Both master and Sprint 21 pre-reset chronology are archived.

Package naming stays at Sprint 20; the matrix resolves candidate/report version
from current Info.json and writes evidence outside the clean product worktree.
The review evidence below retains its original Sprint 21 inputs and verdicts.
No release or native acceptance campaign is selected by this merge.

Integration validation: one supervised invocation of `Test-KmgGate.ps1` and
`Test-CompatibilityCandidateReuse.ps1` completed in **3.330 seconds**. The 11
changed PowerShell scripts parsed; 19 filter/refusal contracts, 8 nested
development/publisher/package contracts and 6 compatibility contracts passed.
The actual matrix/profile wrappers and runtime artifact-selection branch observed
**one fixture build, seven fresh deployment receipts and 19 scenario reuse calls**.
External compiler/deploy/game boundaries used bounded filesystem fixtures;
this is orchestration evidence. Production source/assets/Info.json are unchanged
from the master base, and the integration creates no product artifact.
Captured log: `C:\Dev\.workflow-reset\logs\g-master-integration.log`.

# Workflow reset review checkpoint - October 11, 2026

Branch: `codex/workflow-reset-2026-10-11`, from fetched active Sprint 21
`851f2a4dcc9cae23329d5e8d1cdbdc4c2a5b6e8d`. The original clean master checkout
was fast-forwarded to fetched `d0ceafae7`; other worktrees/artifacts were preserved.
This is a workflow repair for review, with no merge, release or game campaign.

The permanent entry is [VALIDATION-POLICY.md](VALIDATION-POLICY.md). Root AGENTS
and current resume/state defer to it; former root/resume/state instructions and
the original Sprint 21 handoff are intact under [history](history/README.md).
The actual shared home is `C:\Users\howar\.codex` (CODEX_HOME is unset). No home
AGENTS/override, ancestor instruction or nested override was found. Repository
policy is effective without changing unrelated projects' global defaults. A fresh
read-only Codex session confirmed Focused, the 15/30-minute limits and explicit
release boundaries. Its missing handoff link was supplied by this checkpoint.

## Selected commands

```powershell
# Icon/UI work: compile production and relevant domain contracts; inspect the view/export.
.\scripts\Invoke-KmgGate.ps1 -Filter 'eastern-weapons'
# Mechanics: replace the filter with affected behavior and actual dependencies.
.\scripts\Invoke-KmgGate.ps1 -Filter 'expanded-summoning.sprint20'
# Explicit tranche candidate only (do not run Sprint first for unchanged inputs).
.\scripts\Invoke-KmgGate.ps1 -Level Tranche
# Explicit broad matrix on that unchanged, verified candidate; no candidate rebuild.
.\scripts\Invoke-ExpandedSummoningCompatibilityMatrix.ps1 -ReuseCandidate -PackagePath '.\artifacts\local-runtime\0.0.149\KingmakerGunslinger-0.0.149-local-runtime.zip'
```

Focused now compiles the production project as well as the current domain runner;
the domain project alone does not cover every engine-facing source file. It
rejects empty selection and reports deferred coverage. Sprint/Tranche remain
explicit Build-Local boundaries. Private-reference packaging stays in those
boundaries; Focused uses GamePath.props or the supported installed-game argument.

The matrix validates one candidate before profile entry. Every profile creates a
fresh deployment record under its transaction lease and every scenario uses that
record/package through the existing runtime reuse validator. External deployment
records are refused at profile entry. The actual installation guard, fresh game
processes, guarded saves, lease disposal and transactional restoration remain.
Build-Local now delegates one package assembly/output/strict validation to
package.ps1, rather than assembling/validating again. The publisher consumes that
package on its private-reference path; its second reproducibility compilation
skips the unchanged domain suite after the first pass. Publication intake and
reproducibility requirements remain explicit release work.
The normal build compiles the runner once and production once, rather than
compiling the runner again through the solution. Publisher qualification clears
an inherited focused filter and restores the caller's environment; verified-byte
copies retain the existing public ZIP name without assembling another package.

## Focused verification

Commands below ran through a supervising Windows PowerShell 5.1 invocation with
`-NoProfile -ExecutionPolicy Bypass`; logs are retained locally in
`C:\Dev\.workflow-reset\logs` and are not committed machine evidence.

| Actual command | Result | Elapsed |
| --- | --- | --- |
| `scripts/Test-KmgGate.ps1` | 19 filter/refusal checks and 8 nested development/publisher/package guard checks PASS | 1.972 s |
| `scripts/Test-CompatibilityCandidateReuse.ps1` | 6 positive/negative orchestration checks PASS | 1.604 s |
| `scripts/Invoke-KmgGate.ps1 -Filter 'harness.focused-selection-filter'` | Production compile PASS; 1 selected domain test PASS, 0 FAIL | 9.012 s |

The bounded compatibility fixture executes the actual matrix and profile
wrappers plus the actual runtime artifact-selection branch: **1 build, 7 fresh
profile deployments, 19 scenario reuse calls**, one immutable package, one receipt
per profile. It also checks stale receipt/changed candidate refusal before entry
and exact fixture restoration/lease disposal after an injected failure. Compiler,
deployment and game boundaries are fixtures; these are not live purity or native
PASS results. The gate fixture observes actual production/test child invocations
and the publisher function; PlanOnly is descriptive and no longer used as proof.
An initial direct-project compile exposed the solution/project Platform spelling
difference; CompileOnly now uses AnyCPU and the final real compile passed.

Actual development DLL SHA256:
`8fde4c1f20e526dc474f2c72885c3bb3662381bcdd6c17e5e5902a3c12586c0f`.
Production source remained at the Sprint 21 base; validation ran on the uncommitted
workflow diff with that HEAD. This is an unpackaged development artifact, not a
qualified release or current-game coverage. No historical PASS was relabeled.

## Retained failures and deferred release work

Four boundary failures were reported already at 0.0.140: Brown-Fur publication
and three teleportation-scroll-template checks. That is historical evidence on
earlier inputs, not a matched current-binary baseline or a new PASS. The three
vendor differences remain unresolved: capital 10/7, eastern 66/54, spear 34/28.
Precise inspection of RuntimeTestRunner.cs shows capital stock's fixed 10-row
check is separate from BTSL stock; spear/eastern expectations already incorporate
loaded Honest Guy table counts (22/42 plus six per relevant table). Absence of
optional mods or an on/off match does not by itself explain those failures.
No observer expectation was changed without a proved contract. Preserve raw
evidence and perform one matched comparison/affected native case when selected
for release; do not repeatedly replay unrelated scenarios.

Deferred: final inventory/player-path census, the real five-profile matrix,
whole-roster persistence, module-disabled and regression closure on one frozen
candidate, the observer/driver failure cases, the above boundary classification,
owner presentation review and release reproducibility/publication. Earlier
closure records spanning several candidates retain their original identities.
