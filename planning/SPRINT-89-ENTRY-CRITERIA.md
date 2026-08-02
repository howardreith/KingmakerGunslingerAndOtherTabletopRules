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
