# Sprint 17 report — executed evidence and runtime-reference handoff

## Milestone

```text
Version:                    0.0.17-s17-executed-evidence-handoff
Source implementation:      complete
Net47 semantic compile:     passed
Executed pure tests:        373/373, three runs
Main Kingmaker DLL:         not compiled
UMM install package:        not produced
Persistence gate:           NoGoIncomplete
```

## Branch decision

Sprint 16 did not produce a Kingmaker runtime candidate or any in-game lifecycle observations. Sprint 17 therefore remains on the qualification branch and does not add powder, bullets, reload actions, or another persistence carrier.

The sprint closes all practical compiler/test gaps that can be closed without the proprietary Kingmaker runtime and adds the exact handoff needed to compile against that runtime next.

## Executed C# evidence

The classic dependency-free test project was read directly from its explicit compile-item list and compiled with:

- Roslyn C# compiler 4.7.0.
- C# 7.3.
- `.NET Framework 4.7` reference assemblies.
- Warnings as errors.
- Release optimization.
- Deterministic mode.

The produced executable completed three consecutive runs:

```text
Completed 373 tests; failures=0.
Completed 373 tests; failures=0.
Completed 373 tests; failures=0.
```

All three stdout files are byte-identical. The complete evidence record and hashes are retained under `evidence/executed-domain-tests/`.

## Defects found and fixed

Execution exposed four issues that static parsing did not establish:

1. Recursive `FirearmItemId` equality operators caused a stack overflow.
2. The missing-state diagnostic contract used stale implementation-specific wording.
3. The duplicate-token test expected the wrong exception type.
4. Invalid attack distances could be normalized to zero and incorrectly select touch AC.

The armor-class fix now rejects `NaN`, infinity, and negative distance values before range tolerance is applied. Two additional tests cover negative and infinite distance, bringing the suite from 371 to 373 cases.

## Runtime-reference handoff

The new script:

```text
scripts/export-private-build-references.ps1
```

creates a private archive containing only the ten managed assemblies currently required by the project, plus a hash manifest and a non-redistribution notice. It includes no executable, save, configuration, account, or installation-path data.

The new tool:

```text
tools/build_mod_from_private_references.py
```

can compile the main mod and create a UMM-shaped compile candidate after that private bundle is supplied. The candidate remains unqualified until loaded in Kingmaker.

## Stable blueprint ledger

Sprint 17 adds no blueprint identifiers and changes no GUIDs.

```text
Manifest entries:  12
Active entries:     8
Reserved entries:   4
New Sprint 17 IDs:   0
```

## Readiness decision

This milestone is **not ready for Kingmaker** under the project's user-facing definition because it contains no UMM-installable ZIP with `KingmakerGunslinger.dll`.

The next unavoidable input is the private managed-reference bundle from the target Kingmaker installation. Once supplied, the main source can be type-checked and packaged against the exact runtime before the first in-game evidence run.
