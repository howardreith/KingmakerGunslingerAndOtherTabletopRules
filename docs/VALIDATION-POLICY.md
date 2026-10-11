# Validation policy

Owner directive of October 11, 2026. This document governs validation frequency
for development, review, packaging, runtime wrappers and release work. Earlier
mission/handoff chronology describes its original campaign; it cannot silently
select a broad campaign for an ordinary checkpoint.

1. Ordinary fixes end at a reviewable development checkpoint: inspect the diff,
   compile affected production code when needed, and test changed behavior and
   actual dependencies. State the focused pass's limits. Documentation changes
   need relevant text/link checks, without a DLL, package or acceptance ledger.
2. Review the coherent change before broad qualification. A review correction
   gets the failing reproduction and affected neighbors first. Batch related
   corrections before an expensive candidate build. Broaden only for an
   identified shared dependency, unresolved regression risk, or selected release
   or milestone qualification.
3. Full compatibility, historical replay, whole-roster persistence, live-lab
   purity and release reproducibility belong to a frozen candidate or justified
   broad task. They are not prerequisites for every edit, commit or review.
   Inspect nested child calls; a wrapper must not silently broaden the scope.
4. Build an unchanged candidate once and reuse its verified artifact. Distinguish
   product source, DLL/package hashes, harness/reader/fixture versions, environment
   and documentation provenance. Keep evidence bound to its original inputs;
   never label old PASS as execution on changed bytes. Documentation updates
   preserve a qualified artifact, using its frozen worktree when identity guards
   require it, rather than rebuilding to stamp a newer documentation HEAD.
5. A reader-only correction may re-evaluate immutable raw evidence only when all
   needed observations exist. Retain old and new verdicts. A changed observer or
   fixture needs its affected native scenario again. Observer/diagnostic code
   compiled into the product changes the binary identity.
6. Preserve per-transaction locks, current installation and save protection,
   package validation, controlled staging, rollback and exact restoration. Never
   kill an active transaction or omit cleanup for a time budget. Safety changes
   need focused positive and negative behavior checks before live use.
7. Ordinary validation has a 15-minute planning target and 30-minute ceiling,
   excluding necessary safe completion/restoration already in progress. If a
   selected check will exceed the ceiling, do not automatically launch broad work.
   Report useful evidence and the precise remaining check. Explicit release campaigns have
   separately declared budgets. These are workflow limits, not measured speeds.
8. Prefer behavior and stable contracts; reuse existing infrastructure. Add tests
   for concrete risks, not implementation text, cosmetic edits, arbitrary counts
   or prose wording unless wording is the contract. Reject zero-test selections.
   Reproduce a suspected baseline failure once under matched conditions, classify
   it accurately and retain it. Do not repeatedly rerun unrelated suites or
   convert known failures to PASS.
9. Use one supervised invocation with captured logs and a concise completion
   summary; preserve required child-process isolation and resumable receipts for
   unchanged inputs. Avoid repeated polling/restarts. Report tier/scope, changes,
   actual commands/durations, pass/fail counts, deferred release coverage and
   artifact identity where relevant. A focused pass is not full qualification.

## Selected entry points

An icon/UI repair (for example the Nodachi) uses production compilation and the
affected presentation tests. Follow the icon art guide and inspect the affected
view; owner visual approval remains separate. Select a matching test name:

```powershell
.\scripts\Invoke-KmgGate.ps1 -Filter 'eastern-weapons'
```

A Sprint 20 mechanics correction on master starts with its affected rules and dependencies:

```powershell
.\scripts\Invoke-KmgGate.ps1 -Filter 'expanded-summoning.sprint20'
```

Then run the affected guarded native fixture/scenario when mechanics changed.
Shared registration or persistence changes broaden to related summoning tests
and the affected cold-load/module behavior. Observer repairs start with their
affected fixture, not the entire compatibility matrix.

After review, an explicitly selected frozen candidate boundary uses one build:

```powershell
.\scripts\Invoke-KmgGate.ps1 -Level Sprint
# Tranche selects the same build path; choose it instead of Sprint for closure.
.\scripts\Invoke-KmgGate.ps1 -Level Tranche
# Execute this only for explicitly selected compatibility qualification, from
# the candidate's unchanged worktree, with the package produced above:
.\scripts\Invoke-ExpandedSummoningCompatibilityMatrix.ps1 -ReuseCandidate -PackagePath '.\artifacts\local-runtime\0.0.149\KingmakerGunslinger-0.0.149-local-runtime.zip'
```

Do not run Sprint and then Tranche on unchanged inputs. The matrix otherwise
builds one candidate before entering profiles. Each profile creates a fresh
deployment record, reused only within that transaction; restoration invalidates
that deployment's authority. Fresh game processes and save isolation remain.

Deferred release checks include full census, roster persistence, 16 module states,
profiles, exact-reference reproducibility and final native closure. The Sprint 21
handoff retains observer failures, four historical boundary failures and three
unresolved vendor-count differences. An on/off match alone does not establish
attribution. Use one matched baseline comparison or precise contract inspection
before classifying them. This workflow repair does not close those findings or
repair gameplay/observer expectations.
