# Optional-mod compatibility mission

## Objective

Build a safe, reproducible, manifest-driven framework that determines whether
the standalone Kingmaker Gunslinger package remains mechanically and
structurally intact beside exact locally supplied optional-mod builds. Evidence
is bound to local hashes and versions and never creates a gameplay-mod runtime,
compile, package, assembly, or blueprint dependency.

Starting identity is `master` commit
`d03dfe9eae65f5cd1395df7337f21dfdb4357661`, version `0.0.71`. Work is isolated
on `codex/postbase-archetypes-compatibility` in
`.worktrees/postbase-archetypes-compatibility`.

## Frozen boundaries

- Preserve the complete base Gunslinger, Mysterious Stranger, production
  firearms, item-owned state, projectiles, Wwise lifecycle, feats, saves, and
  accepted native firearm rigs.
- Do not implement new archetypes, firearms, deeds, balance, transforms, art,
  audio, or other tabletop content.
- Inspect `C:\Dev\KingmakerGunslingerLab\examples` read-only. Never download,
  build in place, alter, repackage, or commit third-party content.
- Every real launch uses the guarded request mechanism through Steam App ID
  640820. The observer is save-free. Only named authorized working-save smoke
  may use `KMG_AUTOMATION_WORKING`; never access or overwrite
  `KMG_AUTOMATION_BASELINE`.
- Never work on or merge `master`, rewrite history, force-push, or remove another
  worktree.

## Phases

- [x] Verify clean integrated baseline and create the isolated branch/worktree.
- [x] Run unchanged repository, 911-test, exact-reference Release, SoundBank,
  build-output, and strict package gates.
- [x] Inventory and classify every immediate local reference child with exact,
  machine-local hashes and a committed logical catalog/schema.
- [x] Perform deterministic identity, GUID, Harmony-target, and bootstrap/static
  overlap analysis with explicit heuristic confidence.
- [x] Resolve committed compatibility profiles only to exact unambiguous local
  inventory records.
- [x] Fixture-qualify atomic Mods-directory staging, interruption recovery,
  managed SoundBank restoration, and exact original-state restoration.
- [ ] Add the guarded `observe-optional-mod-compatibility` scenario using exact
  UMM 0.32.4 and Harmony12 1.2.0.1 contracts.
- [ ] Run each eligible exact profile in fresh Steam-launched processes and
  verify restoration after every run.
- [ ] Qualify the base class and Mysterious Stranger as the first structural
  vertical slice, then all applicable firearm, projectile, Wwise, reload,
  Scatter, switching, comprehensive, and working-save gates.
- [ ] Apply only narrow standalone-safe Gunslinger repairs proven necessary by
  exact evidence.
- [ ] Finish exact-version claims, reports, matrices, package evidence, and
  pushed coherent checkpoints. Never merge.

## Dispositions

Use only `RUNTIME-QUALIFIED-EXACT`, `STATIC-AUDITED-ONLY`,
`UNAVAILABLE-LOCAL-REFERENCE`, `INVALID-LOADABLE-REFERENCE`,
`CONFLICT-OBSERVED`, `GUNSLINGER-REPAIR-REQUIRED`, `UNSUPPORTED-INVASIVE`, and
`NOT-TESTED`. Main-menu coexistence alone is not compatibility.

## Transaction safety contract

The live `Mods` directory is atomically renamed to a run-owned backup, never
merged, copied over, or selectively rewritten. A fresh allowlisted staged
`Mods` directory carries a transaction sentinel. Restoration quarantines the
staged directory, restores the original path, verifies the exact before/after
manifest and bounded external side effects, and only then removes owned staged
data. A mismatch preserves all directories and stops every later profile.
Commands refuse a running Kingmaker process and unresolved transactions.

## Hard stops

Stop only for a missing/unisolatable baseline, an original Mods backup that
cannot be restored and verified, an unrecoverable prior transaction, unsupported
game/Steam/UMM identity, prohibited Steam UI, an exact reference ambiguity that
prevents a required profile, required third-party modification, a new gameplay
or balance decision, a broad unsafe standalone-threatening patch, or the
approved publication helper blocking the required branch. Source-only or absent
optional references do not stop independent work.

## Definition of done

Done requires committed schemas/catalog/profiles; working inventory, static
audit, transaction and recovery tooling; proven exact restoration including
`KMG_Firearms.bnk`; a guarded observer; no third-party package leakage; two
standalone passes; every eligible individual and maximum combined profile
truthfully disposed; exact base/Mysterious Stranger/product integrity; required
combined and working-save evidence; complete exact-hash documentation; a
validated 0.0.72 package; pushed branch; and an explicit no-merge handoff.
