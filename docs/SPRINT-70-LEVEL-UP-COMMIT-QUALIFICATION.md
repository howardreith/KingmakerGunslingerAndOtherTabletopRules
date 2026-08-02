# Sprint 70 level-up commit qualification

Exact installed IL showed that first-level `LevelUpController.Commit` can add a
custom companion to cross-scene state and remote companions. The guarded probe
therefore seeded a disposable native unit to Gunslinger level one and invoked
only the native `LevelUp`-mode commit to level two.

Source commit `73e4b20` added the observer. The first guarded observation,
`20260802T1102097358773Z-disposable-gunslinger-levelup-commit`, failed before
commit because the callback occupied the native `unitJson` argument. Commit
`d0b15f6` corrected the exact installed signature order. The second and final
materially different observation,
`20260802T1105339180432Z-disposable-gunslinger-levelup-commit`, passed:

- source Gunslinger level `0 -> 1 -> 2`;
- isolated preview reached level two before commit;
- native success callback ran;
- party, global units, cross-scene entities, remote companions, and shared
  inventory retained exact reference snapshots after disposal.

The exact committed-tree package and DLL SHA-256 are respectively
`50526cb1c6d5b4c82661717a9e6cac9ca7d9a4e9c644ba3e1beb9fba673941da` and
`5bf4bd967aab6131fdbbcceb22d0f4dc0ddc0dd8608371de304ab71733e9e012`.

This is strong single-run runtime evidence, not the planned two-run
reproduction. The two-attempt boundary prohibits a third assertion-only run.
Creation commit and broad native replacement callbacks remain unqualified.
