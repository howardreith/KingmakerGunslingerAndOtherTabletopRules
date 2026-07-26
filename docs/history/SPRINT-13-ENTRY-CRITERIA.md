# Sprint 13 entry criteria

## Status

**Feature work is blocked.**

Sprint 13 may become the powder-and-bullet inventory sprint only after the Sprint 12 persistence lifecycle matrix produces a GO decision.

## Required artifacts

- A compiled `KingmakerGunslinger-0.0.12.zip` generated against the target Kingmaker installation.
- Passing `runtime-contracts.json` with `firearmStatePersistence.contractPassed=true`.
- `Completed 239 tests; failures=0.` from the dependency-free C# harness.
- Exact UMM package SHA-256.
- UMM log and `output_log.txt` for every critical lifecycle phase.
- Save backups and a written result table based on `docs/PERSISTENCE-TEST-MATRIX.md`.

## Required lifecycle results

- Two identical Test Muskets retain different states after process restart.
- State remains attached through equipment, stash, party transfer, area transition, and rest.
- A native Heavy Crossbow never receives or restores state.
- A newly created Test Musket begins empty/Normal.
- Deletion does not leave an unbounded external record.
- The same sold and repurchased Test Musket retains its state through process restart, without state transfer or duplication.
- Old saves with unstamped Test Muskets load safely.
- Token enchantments have no unacceptable combat, price, tooltip, or compatibility effect.
- Invalid/future token data fails closed without preventing the save from loading.

## GO branch

If all critical requirements pass, Sprint 13 may add stackable Black Powder Charge and Lead Bullet blueprints plus an atomic inventory-query/consumption service. It must keep loaded firearm state separate from inventory stacks.

## NO-GO branch

If any critical requirement fails, Sprint 13 is an alternate persistence sprint. It must:

1. Preserve the immutable state and `IFirearmStateRepository` service contract.
2. Preserve the four stable Sprint 12 token GUIDs for migration.
3. Implement the next evidence-backed carrier.
4. Repeat the entire lifecycle matrix.
5. Continue blocking ammunition and reload behavior.

No feature sprint may bypass this gate by reverting to a wielder buff, blueprint-keyed state, or guessed item identifier.
