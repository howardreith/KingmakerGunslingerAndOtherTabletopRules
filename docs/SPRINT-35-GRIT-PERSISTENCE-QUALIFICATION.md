# Sprint 35 Native Grit Persistence Qualification

## Qualified scope

Commit `4e543fd6c391e87754405fd575598fcff4e600eb` qualifies the native
serialized representation of a non-maximum grit value without writing a save.
It also fixes first-level fill ordering by using Kingmaker's exact unit-scoped
feature-reapply lifecycle. A stable hidden per-unit marker makes that
reconciliation one-time, including across later multiclass feature reapplies.

## Exact contracts

- `UnitAbilityResource` has a JSON constructor and JSON properties for exact
  blueprint and amount.
- `UnitAbilityResourceCollection.PersistantResources` is the serialized list;
  its setter reconstructs the blueprint-keyed resource dictionary.
- Kingmaker's native companion-respec path transfers this same persistent list
  to the replacement descriptor.
- `ApplyLevelup` invokes `ReapplyFeaturesOnLevelUp` after queued level actions;
  it does not raise the global gain-level event used by the first repair theory.

## Source gates

- Focused persistence checks: 6 PASS.
- Focused grit blueprint checks: 10 PASS.
- Runtime scenario preflight checks: 40 PASS.
- Complete domain/reflection suite: 703/703 PASS.
- Repository validation, clean Release build, build-output validation, and
  strict standalone package validation: PASS.
- Package SHA-256:
  `3f32686c52b0c6b21082a5966eb006032e616d15674d6eb73c2d14bf5f078421`.
- DLL SHA-256:
  `ca3c62f4b48abb2baba8217b78276282474c3ab5deeace7af9704fd42ce319a7`.

## Runtime evidence

Exact assembly mod load passed:

- `20260801T1945287324618Z-mod-load-smoke`

Two independent fresh-process guarded runs passed:

- `20260801T1946581078281Z-disposable-gunslinger-grit-persistence`
- `20260801T1948157306155Z-disposable-gunslinger-grit-persistence`

Both observed maximum two and partial current one on the Wisdom 14 original;
Kingmaker's exact default persistence JSON produced a nonempty 69-character
record, deserialized to a distinct object retaining the exact grit blueprint,
and reconstructed exactly one grit record with current one on the fresh
replacement descriptor. An exact later feature reapply retained current one,
proving the persistent initialization marker prevents multiclass refill.
Both detached units were disposed with unchanged
party/global-unit snapshots.

No save/load or save-writing API was invoked. This qualifies the exact native
save representation and reconstruction boundary without weakening the working
save's no-write rule.

## Diagnosed attempts

Runs `20260801T1927062718434Z` and `20260801T1932396799284Z` exposed the
initial-fill ordering defect at Wisdom 14. The first repair used a global
gain-level event not raised by detached `ApplyLevelup`. Exact IL changed the
architecture to the unit-scoped feature-reapply event that `ApplyLevelup`
actually invokes. The repaired assembly then passed twice.

## Disposition

Native grit persistence serialization/reconstruction and later-level
multiclass no-refill behavior are `RUNTIME-QUALIFIED`. Native respec uses the
same persistent resource-list transfer contract documented above. Continue
with firearm-event recovery; this report is a checkpoint, not a stopping
condition.
