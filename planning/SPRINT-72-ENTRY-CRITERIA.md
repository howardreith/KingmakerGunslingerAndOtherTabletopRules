# Sprint 72 entry criteria: save-free level-20 progression

## Existing gap

The class blueprint and level-one/two native paths are qualified, but the
coverage matrix still lacks evaluated native level-20 class statistics and
proof that direct progression-entry facts install across the full class.

## Required observer

- Construct only a native detached `ChargenUnit`.
- Apply level one in exact `CharGen` mode and levels two through twenty in exact
  `LevelUp` mode using the native controller action pipeline.
- Do not invoke `Commit`, load a save, touch shared inventory, or publish the
  disposable unit into party/global state.
- Require exact Gunslinger and character level 20.
- Require native base BAB 20, Fortitude 12, Reflex 12, and Will 6.
- Require every distinct direct feature in progression entries 1-20 to be an
  installed fact on the disposable descriptor.
- Dispose the unit and require exact party/global-unit reference snapshots.

## Qualification

Focused source validation, repository validation, complete domain tests, clean
exact-reference Release build, strict package validation, exact mod load, and
two fresh guarded save-free observations must pass.
