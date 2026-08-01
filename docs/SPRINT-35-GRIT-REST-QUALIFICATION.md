# Sprint 35 Native Grit Rest Qualification

## Qualified scope

Commit `b0ca3f36501962c01b489bee770e25140028c2fe` proves that
Gunslinger grit participates in Kingmaker's ordinary daily-rest resource path.
Exact installed IL shows public static `RestController.ApplyRest` enumerates
registered unit resources and fully restores them; the supported-build
resource-rest eligibility helper returns true.

## Source gates

- Focused detached grit-rest checks: 5 PASS.
- Runtime scenario preflight checks: 40 PASS.
- Complete domain/reflection suite: 703/703 PASS.
- Repository validation, clean exact-reference Release build, build-output
  validation, and strict standalone package validation: PASS.
- Package SHA-256:
  `9211a4cd8dfb0e9b2dc9c2092673f9b4fb947cf3849aa176685e13d5a4608694`.
- DLL SHA-256:
  `997ad369b55856321a3c1ce8593dc219864a0a38857df086e46c5a8902f8e8d6`.

## Runtime evidence

Exact assembly mod load passed:

- `20260801T1916580082273Z-mod-load-smoke`

Two independent fresh-process guarded runs passed:

- `20260801T1918185563653Z-disposable-gunslinger-grit-rest`
- `20260801T1919385521658Z-disposable-gunslinger-grit-rest`

Both runs observed maximum one, initial current one, post-spend current zero,
and post-rest current one. Both canceled their level-up controller, disposed
the detached entity, and preserved the exact party/global-unit reference
snapshots.

The scenario invokes `RestController.ApplyRest` only on a detached disposable
descriptor. It loads and writes no save, performs no UI input, and does not
invoke the global camping controller.

## Disposition

Daily-rest grit refill is `RUNTIME-QUALIFIED`. Save/load and
multiclass/respec persistence remain the next checkpoint; this report is not a
mission stopping boundary.
