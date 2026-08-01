# Autonomous Gunslinger blockers

A runtime hard stop is active for the disposable native respec preview.

## 2026-08-01 disposable respec cleanup hard stop

- Exact metadata observation on `dd85431` passed twice and proved
  `UnitEntityData.PrepareRespec` delegates to `UnitDescriptor.PrepareRespec`,
  whose direct call graph only sets `Body`.
- Source-qualified scenario commit `25d2da1` passed 691/691 tests, clean Release
  build, and strict packaging. Exact mod-load PASS evidence is
  `20260801T1805094993616Z-mod-load-smoke`.
- First save-free attempt
  `20260801T1806257874513Z-disposable-gunslinger-respec-preview` failed closed
  with `NullReferenceException`; no save was loaded.
- Materially different stage-labeled commit `f2fbcc5` passed all source gates
  and mod-load evidence `20260801T1809469616185Z-mod-load-smoke`. Its run
  `20260801T1811040970058Z-disposable-gunslinger-respec-preview` also failed
  closed with `NullReferenceException`. Mandatory cleanup masked the labeled
  inner exception, consistent with disposal after native body replacement but
  not sufficient to claim the exact call site.
- The mission requires stopping after two materially different attempts at the
  same runtime implementation fail. Do not launch a third respec attempt until
  a human authorizes renewed investigation or supplies an exact native cleanup
  contract. Existing same-class and multiclass preview qualifications remain
  valid.

## Active gates (not hard stops)

- Most base-class and production-content rows are not started; they are planned
  engineering work, not blockers.
- Several deed adaptations require exact Kingmaker contract investigation.
  Existing project authority and reversible evidence gathering remain available.
- The authoritative firearm table labels blunderbuss range `special`; the
  immutable definition and marker vocabulary now represent that fact without a
  numeric value and ordinary-AC selection fails closed. Concrete scatter range
  execution remains assigned to Sprint 32 and is engineering work, not a
  human-input hard stop.
