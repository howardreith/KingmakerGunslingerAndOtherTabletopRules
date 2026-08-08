# Pistolero and Musket Master Implementation Report

Status: mission-planning checkpoint only; implementation has not begun.

The feature branch starts from exact merged compatibility baseline
`10b792735db5d685b46749dc08ea819f31fa8052`, version 0.0.72. The intended
architecture, adaptations, replacement rows, compatibility boundaries, and
verification gates are fixed in the durable mission and replacement matrix.

This report will be updated at every coherent implementation checkpoint with:

- exact shared refactors and preserved behavior;
- new blueprint symbols and stable identities;
- installed Kingmaker API/IL contracts used;
- starting-equipment and ownership transaction evidence;
- deterministic, Release, package, runtime, and compatibility results;
- explicit base-starting-firearm implement/defer decision;
- remaining uncertainty and human-only observations.

No completion claim is made at this checkpoint.

## Publication hard stop

Local durable-document commit `c962e33` is clean and ready to publish. The
required approved helper refused the feature branch because its external
allowlist does not contain `codex/pistolero-musket-master-archetypes`. No raw
push or policy workaround was attempted. Implementation has not begun, so no
partially registered blueprints or runtime behavior exist.

Required human action: update the repository push-helper allowlist for the
exact feature branch, then rerun the approved helper and verify the remote SHA.

This historical publication stop was resolved. The approved helper now accepts
the exact feature branch and subsequent coherent checkpoints are published.

## Canonical handedness foundation

Publication was restored. The first source phase adds the canonical project-
owned handedness abstraction used by every later archetype family gate. It
classifies the five current stable `FirearmKind` values exactly once and fails
closed for unknown/undefined values. `ProductionFirearmWeaponSpec` consumes the
same policy, eliminating the prior duplicate kind list. The 914-test and full
clean Release/package gate passes; runtime behavior is unchanged in this phase.

## Scoped proficiency and firearm feat foundation

The existing full firearm-proficiency identity remains unchanged for old saves.
Two stable scoped facts now enforce exact one- and two-handed equipment/action
contracts through a shared fail-closed policy. Production restrictions carry
the exact firearm kind rather than consulting donor weapon categories.

Exotic Weapon Proficiency (Firearms) is one combat feat that grants the preserved
full-proficiency fact, requires BAB +1, and rejects owners already holding it.
Custom firearm feat rows and appended native parametrized level-up rows consume
the same scoped policy. This phase passes 920 deterministic tests and the clean
exact-reference Release/package gate. Runtime qualification is the next
checkpoint; no archetype completion claim is made.

## Starting-firearm transaction foundation

The hard-coded Pistol observer is now an exact expected-starter transaction.
It resolves committed Gunslinger archetype state, observes only native inventory
deltas, rejects wrong/duplicate production starters, preserves detached chargen,
tops exact native ammunition deltas to 20/20, and binds the exact new weapon.
Repeated native callbacks are suppressed only after the same receiver already
owns the exact battered expected starter. The base default remains Pistol until
the archetype identities are wired. This shared phase passes 922 tests and the
full Release/package gate; mandatory Musket runtime proof still awaits the
Musket Master blueprint.
