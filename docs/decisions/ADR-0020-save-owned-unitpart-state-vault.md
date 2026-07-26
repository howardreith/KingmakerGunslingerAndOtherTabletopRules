# ADR-0020 — Use a save-owned UnitPart vault as the Sprint 13 persistence candidate

- Status: **Provisional — runtime gate remains NO-GO**
- Date: 2026-07-13
- Supersedes for new writes: ADR-0019's item-enchantment token carrier
- Preserves for migration: all four Sprint 12 token GUIDs

## Context

The firearm subsystem requires state on an individual gun: loaded rounds, loaded ammunition identity, and condition. The representation must eventually support more than the finite capacity-one states encoded by Sprint 12's four enchantment tokens.

The token carrier has not passed the required Kingmaker lifecycle matrix, and scaling it would require a combinatorial number of blueprints. Feature work is blocked until a durable carrier is proven.

Public Kingmaker mod code provides precedent for custom `UnitPart` subclasses and the generic `Get<TPart>()` / `Ensure<TPart>()` access pattern. That supports trying a save-owned UnitPart, but it does **not** establish that Kingmaker will serialize a mod-defined part containing direct item references. The direct-reference graph is the hypothesis this sprint must test.

## Decision

Create `UnitPartFirearmStateVault` on the current main-character entity. Store records containing:

- one direct `ItemEntityWeapon` reference;
- one primitive `FirearmStateData` payload;
- one record-schema version.

Use exact reference comparison. Treat the main character only as the save host, not as the state owner. Keep process-local weak metadata for diagnostics only.

Route all new reads and writes through `VaultBackedFirearmStateRepository`.

Wrap it in `MigratingFirearmStateRepository`, which reads the four stable Sprint 12 token blueprints and migrates them into the vault using verified one-way transactions. Never create a new legacy token during normal state operations.

## Alternatives rejected

### Character or wielder buff

Rejected because switching, transferring, or dual-wielding guns would move or merge state incorrectly.

### Blueprint-keyed dictionary

Rejected because identical guns need independent state.

### Guessed item runtime ID

Rejected because public runtime members have not been proven stable across process restart, sale, duplication, or reconstruction.

### Finite enchantment-token expansion

Rejected for new writes because arbitrary ammunition and capacity would cause a combinatorial blueprint catalog. Existing tokens remain migration inputs.

### External sidecar file

Rejected as the first alternative because it would need a stable item identifier and transactional coordination with campaign saves. It also creates rollback and cloud-save divergence risks.

## Consequences

### Positive

- Arbitrary future state fits one primitive payload.
- Identity remains attached to a direct item reference rather than a character.
- No new blueprint GUID is required.
- Old Sprint 12 tokens remain recoverable.
- Conflicts and invalid legacy data preserve evidence.
- The existing repository and service contracts remain stable.

### Negative

- The vault may retain sold or deleted items in the save graph.
- Custom UnitPart and direct-reference serialization are runtime assumptions.
- Main-character reconstruction or respec may discard the host part.
- Removing the mod may make saves containing its concrete UnitPart type unloadable.
- Compatibility with inventory-rebuilding mods is unknown.

## Acceptance gate

This ADR remains Provisional. It becomes Accepted only after a compiled UMM build passes every critical row in the persistence lifecycle matrix, including process restart, sale and repurchase, old-token migration, conflict handling, respec, deletion behavior, save-size inspection, and native Heavy Crossbow negative controls.
