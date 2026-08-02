# Sprint 74 entry criteria: detached respec replacement commit

Use the installed `Player.RespecCompanion` replacement architecture without
invoking its global callback. Seed one detached source to Fighter level one,
create a second detached replacement, and commit Gunslinger level one in exact
`Respec` mode. Require the source to remain Fighter 1/Gunslinger 0, replacement
to become Fighter 0/Gunslinger 1, success callback, proficiency/grit facts, and
unchanged party/global/cross-scene/remote-companion/shared-inventory snapshots.

Do not call `PrepareRespec`, `Player.RespecCompanion`, save APIs, or UI events.
Require focused checks, full repository/domain/build/package gates, exact mod
load, and two fresh save-free PASS runs.
