# PR #12 continuation review (head 038a9a24) — R1–R4

Persisted verbatim summary of the owner's continuation prompt. Full text is in
the conversation of record; the four acceptance items are:

- R1: Confine native activation to a valid destination request. Ordinary
  inventory/equipment use must not consume strategic scrolls (refusal before
  activation, not an inert success). Narrow request-bound authorization around
  the exact native activation; always released; no process-global boolean.
  Observer must attribute to the right reader/item/activation. Reproduce the
  boundary case in a disposable fixture first.
- R2: Preserve material variant identity. Explicit equivalence contract
  (canonical activated spell, caster level, item spell level, other
  activation-relevant properties). Snapshot carries caster level; rows and
  confirmation display it; Resolve binds the selected variant exactly;
  AssociatedSpell must not let a teaching-only association authorize an
  unrelated activated spell. Mismatched teaching/activation rejected before
  spending.
- R3: Complete fallback availability and saved-stock integration. Publication
  and migration must share one supplier decision; Migrate must recognize the
  fallback table and record fallback grants; no unconditional fallback stock
  while primary is active; priest independent; already-materialized fallback
  merchant gets stock exactly once; native shared-table diff one-time behavior
  proven where it applies.
- R4: Real gameplay persistence. Extend the existing bounded fresh-process
  harness: purchase/copy/prepare/rest/activate with real paths in phase A,
  record supplier identity/grant/stock/gold/items/learned spell/preparations/
  arrival + actual first-arrow action; fresh-process verify before any fixture
  can reconstruct; module OFF/ON without refill/item loss/free restoration.

Final: focused regressions that fail before and pass after; full validation,
domain tests, clean build/package, affected native regressions; correct final
artifact qualified and installed with the approved backup/rollback workflow;
state/handoff updated honestly (R1–R4 fixed/proven or precisely blocked);
commit and push through the approved wrapper; no merge or public release.
