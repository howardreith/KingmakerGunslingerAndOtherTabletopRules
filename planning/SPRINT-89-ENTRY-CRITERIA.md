# Sprint 89 battered firearm use boundaries

Effective condition governs native discharge and reload eligibility, while the
actual item-owned condition remains the only persisted state and the only input
to maintenance transitions. The shared equipped-firearm context carries both.
An actually Broken battered firearm used by a nonowner is effectively Wrecked
and cannot discharge; rejection consumes no chamber and mutates no state. An
actually Normal nonowner firearm is effectively Broken for use.

The discharge result explicitly records effective condition so validation does
not conflate rejection semantics with actual persistence. Focused invariants,
the battered policy cases, complete domain suite, repository validation, clean
Release build, and strict packaging qualify the source checkpoint. Exact mod
load remains the post-commit runtime gate.

Exact source commit `0410d21` passed guarded Steam mod load as
`20260802T1402557908977Z-mod-load-smoke`. The rebuilt package/DLL hashes were
`51f9a32e752edcba4b16449ea6438e8f8e84fe61a3af8d25abe34189b85b39ca` and
`1c9359b358e5292b799a98dd20ccf42d4c590af073c4c6c3270787239d832a1c`.
