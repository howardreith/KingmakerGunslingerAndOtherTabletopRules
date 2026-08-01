# Sprint 32 entry criteria — scatter and close-range firearm attacks

## Authority

- `AUTONOMOUS-GUNSLINGER-MISSION.md` firearm-rule requirements.
- `planning/ROADMAP-SPRINTS-29-38.md` Sprint 32 contract.
- Authorized local `FIREARMS.md`, especially Scatter Weapon Quality and the
  early-firearm table.
- Existing item-owned firearm state, native attack pipeline, discharge,
  misfire, explosion, and exact-target architecture.

The rules source establishes that a scattering shot attacks every creature in
a cone with a separate attack roll at -2, confirms each threatened critical
separately, excludes precision/Vital Strike-style damage increases, misfires
only when every attack roll misfires, and triples explosion damage after that
misfire. The source labels Blunderbuss range `special` but the available local
material does not state a numeric cone length. No cone length may be invented.

## Observable behavior

- Only an exact equipped, loaded, non-Wrecked firearm with exactly one scatter
  marker can initiate the scatter path.
- Candidate enumeration and geometric filtering are separate. The pure target
  plan deduplicates by exact unit reference, excludes the wielder, produces a
  stable order, and contains no target outside the accepted cone contract.
- Each planned target receives one independent native firearm attack roll with
  a -2 scatter penalty. Critical confirmation remains per target.
- One valid scatter action consumes exactly one loaded chamber total, never one
  chamber per target. Rejection before delivery consumes nothing.
- Non-scatter firearms and unrelated native weapons never enter this path.
- The production Blunderbuss remains unavailable until the numeric/engine cone
  contract and the complete attack path are runtime-qualified.
- All-attack-roll misfire aggregation and triple explosion damage are explicit
  later sub-slices; ordinary second-misfire burst behavior must not be silently
  reused as proof of either rule.

## Deterministic tests

- Empty, singleton, multiple, duplicate, wielder, null, and stable-order target
  plans.
- Boundary-in, boundary-out, angular-edge, behind-wielder, nonfinite, and
  unresolved-range rejection once the exact cone contract is established.
- Exactly one discharge for zero, one, and many planned targets; rollback on
  pre-delivery rejection.
- Separate attack-roll decisions, -2 penalty, per-target critical identity,
  all-roll misfire aggregation, and non-scatter isolation.
- Triple explosion damage only for the qualified scatter-explosion condition.

## Required runtime evidence

- Current exact assembly first passes `mod-load-smoke`.
- A guarded working-save scenario grants only disposable in-memory fixtures,
  proves exact Blunderbuss identity and independent item state, and records
  target candidates, accepted targets, attack rolls, damage, discharge count,
  and save-write sentinels.
- Two consecutive PASS runs from fresh Steam-launched processes are required.

## Initial investigation gate

Before source execution work, inspect exact installed Kingmaker contracts for
native unit enumeration, position/facing or target-point geometry, ranged
weapon attack creation, per-target attack modifiers, and damage delivery. If
the cone length is absent from both authoritative material and an existing
explicit project decision, retain fail-closed availability and record the
smallest player-facing design question rather than choosing a balance value.

## Non-goals

- Advanced firearms, capacity greater than one, class progression, grit,
  deeds, vendors, custom models, enemy firearm AI, or broad area-effect
  infrastructure unrelated to the exact scatter path.
