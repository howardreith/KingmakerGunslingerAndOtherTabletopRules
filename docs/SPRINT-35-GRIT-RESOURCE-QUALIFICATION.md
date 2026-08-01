# Sprint 35 Native Grit Resource Qualification

## Qualified scope

Commit `cd22f3d683129314c26f910a9e99097c062c9bfe` binds the bounded grit
domain to a stable native `BlueprintAbilityResource` owned by the Gunslinger
level-one feature. Its maximum is the Wisdom modifier with a minimum of one;
initial feature activation restores the pool, ordinary level-up does not.

This checkpoint qualifies native first grant, one-point spend, level-two
non-refill, capped restore, and detached-unit cleanup. Daily-rest, save/load,
multiclass/respec persistence, and firearm recovery remain separate required
slices.

## Source qualification

- Sprint 35 blueprint checks: 7 PASS.
- Sprint 35 disposable-runtime structure checks: 5 PASS.
- Runtime scenario preflight checks: 40 PASS.
- Complete dependency-free domain/reflection suite: 703/703 PASS.
- Repository validation, clean exact-reference Release build, build-output
  validation, and strict standalone package validation: PASS.
- Exact package SHA-256:
  `4ddea7d37d08cd1255562cc8d21678ea686c01d1dc3a48ecaade6630d18c8fbd`.
- Exact DLL SHA-256:
  `da0d1e20a51dc288daa3383fbd0fff628b79b76194b7946c1e60a774d6d1543b`.

## Runtime evidence

Exact repaired assembly mod load passed:

- `20260801T1907337714075Z-mod-load-smoke`

Two independent fresh-process guarded runs passed:

- `20260801T1908491510715Z-disposable-gunslinger-grit-resource`
- `20260801T1910149815825Z-disposable-gunslinger-grit-resource`

Both observed:

- initial `maximum=1`, `current=1`;
- spend changed current to zero;
- Gunslinger level two retained current zero;
- native restore changed current to one, equal to maximum;
- party and global-unit reference snapshots remained unchanged after both
  controllers were canceled and the disposable entity was disposed;
- exact loaded mod version `0.0.35`.

The scenario creates only a detached `ChargenUnit`. It does not load, select,
or write any save and performs no UI input.

## Diagnosed first attempt

`20260801T1904500891309Z-disposable-gunslinger-grit-resource` failed safely
inside native `BlueprintAbilityResource.GetMaxAmount`. Runtime construction did
not receive Unity's serialized empty-array defaults, leaving `Amount.Class`
null. Commit `cd22f3d` initializes all four native class/archetype arrays to
their correctly typed empty values. The exact repaired assembly then produced
the mod-load PASS and two feature PASS runs above.

## Disposition

Native per-unit grit grant/spend/level-up/restore behavior is
`RUNTIME-QUALIFIED`. Continue with daily-rest and save/load persistence; this
report is a checkpoint, not a mission boundary.
