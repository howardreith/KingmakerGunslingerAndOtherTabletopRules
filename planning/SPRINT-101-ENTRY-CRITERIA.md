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
Native replacement may destroy its source during the success callback; cleanup
must honor authoritative `EntityDataBase.Destroyed` and never dispose an entity
twice.
`PrepareRespec` transfers the native body and can leave the detached source with
a null body while `Destroyed` remains false; such a transferred shell is not a
valid `Dispose` receiver and is accepted only when all external snapshots prove
it is unregistered.
The broad success callback transforms the original entity to the committed
replacement state while retaining a distinct descriptor object. Acceptance
requires Gunslinger one through both source and replacement views; descriptor
aliasing is recorded diagnostically but is not the native replacement contract.
