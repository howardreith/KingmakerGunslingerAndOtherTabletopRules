# Sprint 101 broad player replacement contract

Before invoking the remaining global respec boundary, extend the existing
metadata-only character-creation observer to record every exact installed
`Player.RespecCompanion` signature and its resolved IL call graph alongside the
already-qualified descriptor/entity/controller contracts.

The observer performs no construction, callback invocation, UI action, save
operation, or game-state mutation. Runtime evidence must establish the exact
callable contract used by the next reversible replacement fixture.

The resulting fixture subscribes one request-local
`ILevelUpInitiateUIHandler`, invokes `Player.RespecCompanion` on a detached
Fighter-one source, and commits the emitted replacement to Gunslinger one. It
must unsubscribe under `finally`, dispose both request-local entities, roll back
starting grants, and restore party, global-unit, remote-companion, cross-scene,
inventory, and money snapshots exactly.
