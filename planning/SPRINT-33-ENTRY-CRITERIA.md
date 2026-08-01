# Sprint 33 entry criteria — capacity, partial reload, and advanced firearms

## Authority

- `AUTONOMOUS-GUNSLINGER-MISSION.md` firearm capacity, partial loading,
  ammunition transaction, persistence, and advanced-firearm requirements.
- `planning/ROADMAP-SPRINTS-29-38.md` Sprint 33 contract.
- Authorized local `FIREARMS.md` capacity, loading, penetration, misfire, and
  advanced firearm tables.
- Existing exact-item state, atomic inventory, discharge, misfire, persistence,
  and production-catalog architecture.

Advanced firearms load all chambers with one move action. Early multi-barrel
firearms load one barrel per normal reload action. Each valid projectile still
consumes exactly one chamber. Advanced misfires cause Broken but never explode.
The roadmap's advanced one-handed/pistol-form slot maps to the authoritative
table's Revolver entry; no separate generic "advanced pistol" statistics are
invented where the local table provides none.

## First observable slice

- A reload action adds the lesser of its defined batch size and the exact
  firearm's remaining capacity.
- A partially loaded firearm may be topped up only with the same ammunition
  identity. A full, Wrecked, incompatible, or under-resourced request mutates
  neither item state nor inventory.
- Successful batch loading consumes exactly one powder charge and projectile
  per added chamber and performs one verified exact-item state replacement.
- Any failure after mutation attempts to restore both state and inventory to
  their exact pre-operation snapshots.

## Deterministic tests

- Empty-to-full advanced capacity, partial-to-full top-up, already-full and
  insufficient-component rejection, mixed-ammunition rejection, exact consumed
  counts, and rollback after state-write failure.
- Later slices add advanced definition/catalog exactness, finite persistence,
  repeated discharge/misfire at every capacity, and two-item isolation.

## Runtime evidence

- Current exact assembly passes guarded `mod-load-smoke` before feature tests.
- A guarded working-save scenario proves exact firearm identity, partial and
  full round counts, repeated discharges, reload inventory deltas, save-write
  sentinels, and independent identical firearms.
- Two consecutive fresh-process PASS runs are required for runtime qualification.

## Non-goals

- Scatter cone distance, Gunslinger class progression, grit, deeds, vendors,
  custom assets, magical firearms, and enemy firearm AI.
