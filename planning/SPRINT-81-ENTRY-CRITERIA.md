# Sprint 81 entry criteria: deterministic Menacing Shot acceptance

The second Sprint 80 comprehensive runtime run failed only the Menacing Shot
forced-failure assertion after its native Will save produced a natural 20.
Every other assertion passed. A -100 save modifier cannot force a failed save
because Kingmaker preserves natural-20 success.

Make the existing save-free observer deterministic by selecting native d20
seeds immediately before its two `AbilityEffectRunAction` applications: a
natural 1 for the failure branch and a natural 20 for the success branch. Reuse
the already-qualified native-d20 seed helper. Do not change production Menacing
Shot blueprints, delivery, transactions, balance, or save behavior.

Require a focused source invariant, inherited validation, the complete domain
suite, runner/preflight checks, clean Release build, strict packaging, exact
mod load, and two fresh-process comprehensive PASS runs.
