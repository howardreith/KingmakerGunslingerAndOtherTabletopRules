# Autonomous Gunslinger blockers

No genuine human-input hard stop is currently established.

## 2026-08-01 disposable respec cleanup investigation (resolved)

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
- Two materially different initiating attempts failed, so the mission requires
  a mode change. The next authorized action is a metadata-only observation of
  the exact body setter and descriptor/entity disposal call graphs. Do not
  launch a third initiating respec attempt until that narrower evidence supports
  a different cleanup architecture. Existing same-class and multiclass preview
  qualifications remain valid.
- Metadata-only run `20260801T1817013647054Z-observe-character-creation-contracts`
  on `07dd111` proved the body setter has no nested managed calls, while entity
  disposal delegates to descriptor disposal and descriptor disposal calls
  `UnitBody.Dispose`. Restoring the retained original disposable body before
  entity disposal is therefore the next evidence-supported architecture; this
  is not a blind third variation.
- Restored-body commit `4fdbfea` passed mod load at
  `20260801T1821061256490Z-mod-load-smoke`. Its reduced initiating run
  `20260801T1822203121648Z-disposable-gunslinger-respec-preview` preserved the
  real first failure as `start-respec-controller`; cleanup no longer masks it.
  Investigation has changed back to metadata-only inspection of controller
  start, construction, and preview call graphs before any further initiating
  attempt.
- Metadata PASS run `20260801T1826247826437Z-observe-character-creation-contracts`
  on `578d404` proved startup immediately constructs the controller; its
  constructor starts/requests the preview, and `RequestPreview` posts and turns
  on the preview entity. Destructive source preparation before construction is
  the invalid ordering. The next reduced scenario keeps the disposable source
  body intact and tests native `Respec` preview mode without `PrepareRespec` or
  `Commit`.
- The reduced single-unit run
  `20260801T1831121462577Z-disposable-gunslinger-respec-preview` completed safely:
  source/body isolation and cleanup passed, but preview retained Fighter 1 and
  added Gunslinger 1. Exact installed `Player.RespecCompanion` IL then proved
  native respec creates a fresh unit from the same blueprint, copies experience,
  and initiates respec on that replacement candidate. The next scenario uses a
  second detached `ChargenUnit` as the level-zero replacement and avoids the
  native global creation/replacement callbacks.
- Exact detached-replacement commit `3d4ba8f` passed mod load at
  `20260801T1836154433116Z-mod-load-smoke` and two independent respec preview
  runs `20260801T1837314150470Z-disposable-gunslinger-respec-preview` and
  `20260801T1838472989503Z-disposable-gunslinger-respec-preview`. The runtime
  investigation is resolved; broad replacement commit remains an engineering
  boundary, not a human-input hard stop.

## Active gates (not hard stops)

- Sprint 40 Utility Shot is disposition-complete and Stop Bleeding is
  runtime-qualified on `8270ade`. Bonus-feat selection is the next engineering
  gate; no human-input blocker was created.

- Sprint 41 Bonus Feats is runtime-qualified using the exact native Fighter
  combat-feat selection. Kingmaker's lack of a native grit-feat category is
  documented and does not block the base combat-feat progression. Sprint 42
  Gun Training is the next engineering gate.

- Sprint 42 Gun Training is runtime-qualified on `76ae9f9` at version `0.0.42`.
  The next incomplete base-class/deed row is an engineering gate, not a
  human-input blocker.

- Sprint 43 Dead Shot is source-qualified at version `0.0.43`. Exact mod load
  and two independent guarded mixed/all-misfire runs remain an engineering
  gate, not a human-input blocker.

- Most base-class and production-content rows are not started; they are planned
  engineering work, not blockers.
- Several deed adaptations require exact Kingmaker contract investigation.
  Existing project authority and reversible evidence gathering remain available.
- The authoritative firearm table labels blunderbuss range `special`; the
  immutable definition and marker vocabulary now represent that fact without a
  numeric value and ordinary-AC selection fails closed. Concrete scatter range
  execution remains assigned to Sprint 32 and is engineering work, not a
  human-input hard stop.
