# Sprint 70 entry criteria: safely reversible level-up commit

## Existing evidence

Disposable native previews already qualify first-level selection, preview
application, same-class level-up, multiclass, and respec behavior. The remaining
independent creation/progression gap is an actual native commit outcome.

## Safety boundary

Do not invoke first-level `LevelUpController.Commit`. Installed native IL proves
that its `SetupNewCharacher` path can add a custom companion to cross-scene state
and remote companions. Exercise only a detached unit already seeded to level one,
using `CharBuildMode.LevelUp`; that path skips `SetupNewCharacher`.

## Required observer

- Construct only a native disposable `ChargenUnit`.
- Seed Gunslinger level one through the already-qualified isolated apply path.
- Select Gunslinger level two in exact `LevelUp` mode and invoke native `Commit`.
- Require the preview and committed source to reach exact Gunslinger level two.
- Require the native success callback.
- Snapshot party, global units, cross-scene entities, remote companions, and
  shared inventory before the operation; require exact reference equality after
  disposal and require the disposable unit to be absent.
- Do not load or write any save.

## Qualification

Focused source validation, repository validation, the complete domain suite,
clean exact-reference Release build, strict package validation, exact mod load,
and two fresh guarded save-free commit observations must pass.
