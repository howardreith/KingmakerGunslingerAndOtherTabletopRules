# Sprint 35 Native Grit Persistence Qualification

## Qualified scope

Commit `1949d801e465c4e31be7b4a5a3caae05b3c49760` qualifies the native
serialized representation of a non-maximum grit value without writing a save.
It also fixes first-level fill ordering by using Kingmaker's exact unit-scoped
feature-reapply lifecycle, guarded to Gunslinger class level one.

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

- Focused persistence checks: 5 PASS.
- Focused grit blueprint checks: 9 PASS.
- Runtime scenario preflight checks: 40 PASS.
- Complete domain/reflection suite: 703/703 PASS.
- Repository validation, clean Release build, build-output validation, and
  strict standalone package validation: PASS.
- Package SHA-256:
  `519c5f99a430808895d1ea787f832088f51b2b861b19974409d6bb2a02715429`.
- DLL SHA-256:
  `0b37c7748cdca07c0015a730da3626b8a0a6a8d132c96e9ef72f64c325d60866`.

## Runtime evidence

Exact assembly mod load passed:

- `20260801T1935464087387Z-mod-load-smoke`

Two independent fresh-process guarded runs passed:

- `20260801T1937081415049Z-disposable-gunslinger-grit-persistence`
- `20260801T1938272020280Z-disposable-gunslinger-grit-persistence`

Both observed maximum two and partial current one on the Wisdom 14 original;
Kingmaker's exact default persistence JSON produced a nonempty 69-character
record, deserialized to a distinct object retaining the exact grit blueprint,
and reconstructed exactly one grit record with current one on the fresh
replacement descriptor. Both detached units were disposed with unchanged
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

Native grit persistence serialization and reconstruction are
`RUNTIME-QUALIFIED`. Continue with explicit multiclass/respec value behavior
and firearm-event recovery; this report is a checkpoint, not a stopping
condition.
