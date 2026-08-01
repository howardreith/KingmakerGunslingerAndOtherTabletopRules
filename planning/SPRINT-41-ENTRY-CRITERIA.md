# Sprint 41 entry criteria: Gunslinger bonus feats

Sprint 41 is a checkpoint in the autonomous completion mission, not a stopping
condition.

## Authority and adaptation boundary

- The base Gunslinger gains a bonus feat at levels 4, 8, 12, 16, and 20.
- Eligible choices are combat or grit feats and must continue to satisfy their
  ordinary prerequisites.
- Installed Kingmaker has a native Fighter combat-feat
  `BlueprintFeatureSelection` whose candidate arrays, feature groups,
  prerequisite evaluation, obligatory selection, and level-up UI are already
  the game's supported combat-feat contract.
- Kingmaker has no native grit-feat category, and this repository currently
  defines deeds as class features rather than selectable feats. Sprint 41 does
  not mislabel deeds or invent grit feats. If independently authorized grit
  feats are added later, they may be appended to the selection without changing
  the five stable progression entries.
- Optional dares that replace bonus feats are alternative deeds and remain
  outside the base-class matrix scope.

## Acceptance contract

- Resolve the exact installed Fighter combat-feat selection by stable GUID;
  fail closed if it is absent or has the wrong blueprint type.
- Add that same native selection reference exactly once at Gunslinger levels
  4, 8, 12, 16, and 20, and nowhere else.
- Do not clone the selection, copy its candidates, ignore prerequisites, or add
  a project-owned blueprint identifier.
- Preserve all existing progression facts and their order.
- Focused tests must prove the exact cadence, no early/intermediate grant, and
  rejection of invalid levels.
- Guarded runtime qualification must prove the exact native selection identity
  occurs once at each required level, zero times elsewhere, retains nonempty
  candidates and prerequisite enforcement, and introduces no save write.
