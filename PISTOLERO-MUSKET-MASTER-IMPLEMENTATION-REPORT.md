# Pistolero and Musket Master Implementation Report

Status: Pistolero and Musket Master independently implemented and qualified.

The complete feature set is present at version 0.0.73. It includes stable
archetype identities, canonical handedness, scoped proficiency and firearm EWP,
the generalized exact starter transaction, family training, shared reload and
per-attack range policies, both archetype progressions and deeds, archetype-
aware True Grit, truthful presentation, reconciliation, and guarded fixtures.
Existing identities and the merged optional-mod compatibility transaction and
current-donor validation architecture are preserved.

The optional ordinary-Gunslinger starter selection was safely deferred: the
installed 2.1.7b API exposes only static class/archetype starting arrays and an
`AddStartingItems(UnitDescriptor)` transaction with no committed selection
input. Implementing it would require the post-hoc inventory surgery prohibited
by the mission. Base Gunslinger, Mysterious Stranger, and Pistolero therefore
retain the backward-compatible exact Pistol; Musket Master uses the native
archetype replacement contract for the exact Musket.

The full Gunslinger aggregate is not claimed green. It remains blocked by the
inherited detached Gunslinger's Dodge missing-buff defect and an unchanged-
source Targeting Torso cached-threat defect reproduced during the combined
profile. Neither failure is caused by the archetypes, and no assertion was
weakened.

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

## Explicit base starting-firearm choice decision

Deferred as the authorized non-blocking outcome. Exact installed Kingmaker
2.1.7b reflection shows that `BlueprintCharacterClass` exposes only one static
`BlueprintItem[] StartingItems`, while `BlueprintArchetype` exposes the static
`ReplaceStartingEquipment`, `StartingGold`, and `BlueprintItem[] StartingItems`
contract used by Musket Master. The exact native grant surface is only
`LevelUpHelper.AddStartingItems(UnitDescriptor)`; it accepts no selection or
selected-item argument. `LevelUpController` can commit feature selections and
archetypes, but there is no native conditional starting-item mapping.

Consequently an ordinary-Gunslinger Pistol/Musket feature selection could only
affect inventory through a Harmony-side synthesized grant, delayed replacement,
or global/static starting-array mutation. Those designs violate the mission's
no-synthesis/no-post-hoc-surgery and save-safety requirements. Weapon Focus was
also rejected: the installed surface does not cure its ambiguous timing,
multiplicity, later-level, and respec semantics. Base Gunslinger and Mysterious
Stranger therefore retain the exact backward-compatible Pistol default;
Pistolero resolves Pistol and Musket Master uses the mandatory native archetype
Musket array.
