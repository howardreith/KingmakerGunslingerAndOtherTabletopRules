# Sprint 15 report — persistence evidence harness

## Result

Sprint 15 is complete as a source milestone.

```text
Version:                    0.0.15-s15-persistence-evidence
Source implementation:      COMPLETE
Portable structural checks: PASSED
Compiled Kingmaker build:   NOT PRODUCED
UMM install package:        NOT PRODUCED
Persistence gate:           NO-GO / INCOMPLETE
Ammunition work:            BLOCKED
```

Sprint 14 established an engine-item-identity-keyed `UnitPart` vault candidate, but the project still lacks runtime evidence from Kingmaker. Sprint 15 does not invent another carrier and does not treat unobserved behavior as success. It adds the tooling needed for a compiled build to produce an auditable persistence decision.

## Delivered

### Fixed lifecycle catalog

`PersistenceMatrixCatalog` codifies all 35 rows from the authoritative lifecycle matrix:

- 30 Critical rows;
- 5 High-severity rows;
- 7 Critical rows that require a second passing run: I03, I10, I11, I13, I15, I19, and I23.

The catalog is immutable and has deterministic order from I01 through I35.

### Pure gate evaluator

`PersistenceEvidenceEvaluator` evaluates the latest observation for each row. Its decisions are:

- `NoGoFailed` when any Critical row's latest result is FAIL;
- `NoGoIncomplete` when a Critical row is missing, BLOCKED, or lacks required reproduction;
- `Go` only after all Critical requirements pass.

High-severity failures are retained as warnings rather than silently discarded.

### Build-fingerprinted evidence sessions

The external recorder fingerprints:

- mod informational version;
- mod DLL SHA-256;
- blueprint manifest SHA-256;
- Unity-reported game version;
- game assembly identity and SHA-256;
- UMM assembly identity and SHA-256;
- Harmony assembly identity and SHA-256.

An existing session resumes only when the entire fingerprint matches.

### Structured snapshots

Each BEFORE and AFTER snapshot records available data for visible firearms:

- process-local repository identity and revision;
- engine item ID;
- runtime type;
- item and weapon-type blueprint IDs;
- loaded rounds, ammunition identity, and condition;
- identity and legacy-reference record counts;
- repository creation, mutation, and removal counters;
- identity-migration and token-migration snapshots.

Unavailable runtime data causes snapshot capture to fail rather than being replaced by plausible values.

### Atomic external evidence

Evidence is written under the installed mod directory's `evidence/` folder as:

```text
current-session.json
persistence-evidence-<session-guid>.json
persistence-evidence-<session-guid>.md
```

JSON writes use a temporary file followed by replacement or move. The Markdown report contains the gate result, matrix status, pass-run counts, notes, optional save hashes, and canonical before/after summaries.

These files are diagnostic artifacts only. Firearm repositories and save migration code contain no dependency on the recorder and never read the evidence directory.

### UMM controls

The development panel now supports:

- start new session;
- previous/next matrix row;
- begin independent reproduction run;
- optional note;
- optional save-before and save-after SHA-256 values;
- capture BEFORE;
- record PASS only with matching BEFORE/AFTER snapshots;
- retain FAIL or BLOCKED observations even when a crash, failed load, or mod-removal operation prevents snapshot capture;
- discard pending BEFORE;
- export Markdown report.

### Portable test runner

`tools/run_portable_domain_tests.py` reads the classic .NET Framework test project's explicit compile items and creates a temporary SDK-style `net8.0` project. It lets the dependency-free harness run on a machine with Python 3 and a .NET 8 SDK without modifying the Kingmaker-targeted project.

Sprint 15 adds 24 evidence-domain cases, bringing the declared dependency-free total from 327 to 351.

## Stable blueprint ledger

Sprint 15 introduces no blueprint IDs and changes no existing GUID.

```text
Manifest entries: 12
Active entries:    8
Reserved entries:  4
New Sprint 15 IDs: 0
```

## Deliberately excluded

- Black Powder Charge and Lead Ball item blueprints.
- Inventory ammunition transactions.
- Reload actions.
- Attack-time state consumption.
- Any new persistence carrier.
- Any claim that `UniqueId` or the custom `UnitPart` survives Kingmaker's lifecycle.
- Any DLL or UMM-installable package.

## Decision

The persistence gate remains closed because no observations from an actual compiled Kingmaker build exist. Sprint 16 may begin ammunition work only after a complete evidence session evaluates to `Go`. Otherwise Sprint 16 must address a failure demonstrated by the evidence files rather than selecting a carrier from speculation.
