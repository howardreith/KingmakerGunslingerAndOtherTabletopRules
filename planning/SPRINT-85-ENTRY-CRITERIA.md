# Sprint 85 battered-firearm origin ledger

The authorized persistent-owner mapping requires one immutable relationship
between an engine-issued firearm item GUID and the originating unit's exact
stable identity. This checkpoint implements the dependency-free ledger before
introducing Kingmaker serialization or starting-item hooks.

The ledger must create a first binding, treat reconstruction of the identical
binding as an idempotent no-op, reject rebinding to another unit without
mutation, isolate multiple items, expose defensive snapshots, and reject null,
empty, padded, or oversized identities. It never derives ownership from the
current wielder and never assigns an engine identity.

Six focused cases plus the complete domain suite, repository validation, clean
Release build, and strict package validation qualify this core. Runtime testing
is deferred until the exact ledger is hosted by a save-owned `UnitPart` and
connected to a narrowly observed starting-item grant.
