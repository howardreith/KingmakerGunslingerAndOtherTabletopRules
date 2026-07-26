# Sprint 14 entry criteria

## Status

**Feature work remains blocked.**

Sprint 14 may become the powder-and-bullet inventory sprint only after the Sprint 13 UnitPart persistence lifecycle matrix produces a GO decision from a real compiled Unity Mod Manager package.

## Required artifacts

- A compiled `KingmakerGunslinger-0.0.13.zip` generated against the target Kingmaker installation.
- Passing `runtime-contracts.json` with:
  - `kingmaker.firearmStatePersistence.contractPassed=true`;
  - `kingmaker.legacyStateTokenMigration.contractPassed=true`.
- `Completed 292 tests; failures=0.` from the dependency-free C# harness.
- Exact UMM package SHA-256.
- UMM log and `output_log.txt` for every critical lifecycle phase.
- Before/after save backups and save-size measurements.
- A completed result table based on `docs/PERSISTENCE-TEST-MATRIX.md`.

## Required critical results

- Two identical Test Muskets retain different vault states after exit, process restart, and reload.
- Direct item references resolve to the actual reconstructed inventory, equipped, stash, or vendor item—not a duplicate object.
- State remains attached through equipment, weapon-set switching, party transfer, stash movement, area transition, rest, and respec.
- A native Heavy Crossbow never creates or restores a vault record.
- A new or old-save Test Musket without state begins empty/Normal.
- Sale and repurchase preserve state through process restart without transfer or duplication.
- Permanent deletion does not cause unbounded save growth or a live orphan item.
- Every supported Sprint 12 token migrates once, verifies, and disappears.
- Equivalent vault/token state cleans up safely.
- Conflicting, unknown, and duplicate legacy tokens fail closed without preventing save recovery.
- The UnitPart carrier adds no unacceptable tooltip, value, combat, or UI effect.
- The save remains loadable after ordinary mod upgrades covered by the migration schema.

## GO branch

If every critical requirement passes, Sprint 14 may add stackable Black Powder Charge and Lead Bullet blueprints plus an atomic inventory-query and consumption service. Loaded firearm state must remain separate from inventory stacks.

## NO-GO branch

If any critical requirement fails, Sprint 14 remains a persistence sprint. It must:

1. Preserve `FirearmState`, `IFirearmStateRepository`, and the exact-item service boundary.
2. Preserve all four Sprint 12 token GUIDs for migration.
3. Preserve readable Sprint 13 vault data where possible.
4. Implement the next evidence-backed carrier or repair the failing lifecycle contract.
5. Repeat the complete lifecycle matrix.
6. Continue blocking ammunition and reload behavior.

No feature sprint may bypass this gate with a wielder buff, blueprint-keyed state, inventory index, display name, or guessed runtime identifier.
