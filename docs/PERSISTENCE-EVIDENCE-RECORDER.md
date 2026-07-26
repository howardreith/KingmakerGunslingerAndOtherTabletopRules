# Persistence evidence recorder

## Sprint 17 trusted preflight status

The recorder may append I01 and I02 without firearm BEFORE/AFTER snapshots only through `RecordTrustedRuntimePreflight()`. The report is produced by a fixed evaluator that can contain only I01 followed by I02. I03 through I35 continue to use the ordinary evidence workflow. Evidence snapshots now reject any firearm whose strict `ItemEntityWeapon.UniqueId` cannot be read and validated.

## Purpose

The recorder evaluates the Sprint 14 engine-item-identity vault candidate against the 35-row lifecycle matrix. It is an external diagnostic system, not a firearm-state carrier.

## Non-carrier boundary

The recorder may read repository snapshots and write diagnostic JSON/Markdown files. It may not:

- restore firearm state;
- seed the UnitPart vault automatically;
- substitute for a missing save record;
- change an item's engine identity;
- influence attack, reload, damage, or inventory behavior;
- be read by any firearm repository or migration service.

Deleting the `evidence/` directory must have no effect on a campaign save.

## Session schema

A session contains:

- schema version and session GUID;
- start/update UTC timestamps;
- current run number and matrix row;
- exact build fingerprint;
- optional pending BEFORE snapshot;
- ordered observations with globally unique sequence numbers.

Each observation contains:

- row ID and PASS/FAIL/BLOCKED status;
- UTC timestamp and run ID;
- optional note;
- optional SHA-256 values for save files before and after the operation;
- structured BEFORE and AFTER snapshots when available.

A PASS is accepted only with a matching BEFORE snapshot and a successfully captured AFTER snapshot. FAIL and BLOCKED observations may be recorded without either snapshot so crashes, failed loads, mod-removal tests, and other operations that make the campaign unavailable can still be preserved as evidence. Missing snapshots are written explicitly as `<missing>`; they are never fabricated.

## Gate rules

A Critical row is complete only when its latest result is PASS. A BLOCKED row is incomplete. A latest FAIL is a failed gate.

The following rows require PASS observations from two distinct run IDs:

```text
I03 I10 I11 I13 I15 I19 I23
```

Five High-severity rows may warn without independently closing the gate.

## Atomicity and resumption

The recorder writes a UTF-8 temporary file and then replaces or moves it into place. It does not append partially formed JSON.

`current-session.json` is resumed only when the complete fingerprint matches. A file from another DLL, blueprint manifest, game assembly, UMM assembly binary, Harmony assembly binary, or game version is preserved but ignored.

## Manual workflow

1. Start a session.
2. Select a matrix row.
3. Set up the row's initial state.
4. Optionally enter a note and save-before SHA-256.
5. Capture BEFORE.
6. Perform the lifecycle operation.
7. Optionally enter a save-after SHA-256.
8. Record PASS only when AFTER can be captured. Record FAIL or BLOCKED even when the operation prevents an AFTER capture; include the failure in the note.
9. For reproduction rows, begin another run and repeat independently.
10. Export the Markdown report.

The human tester remains responsible for interpreting visual-only criteria such as tooltip or value changes. Notes should record those observations explicitly.
