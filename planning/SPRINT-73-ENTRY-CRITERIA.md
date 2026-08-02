# Sprint 73 entry criteria: save-free multiclass commit

Seed a detached native unit to Fighter level one, then use exact `LevelUp` mode
to commit Gunslinger level one. Require Fighter 1/Gunslinger 1, the native
success callback, and exact Gunslinger proficiency and grit facts. Snapshot
party, global units, cross-scene entities, remote companions, and shared
inventory; all must remain reference-identical after disposal. Do not load a
save or invoke first-level `Commit`.

Qualification requires focused checks, repository validation, 831/831 tests,
clean exact-reference Release build, strict package validation, exact mod load,
and two independent fresh save-free PASS runs.
