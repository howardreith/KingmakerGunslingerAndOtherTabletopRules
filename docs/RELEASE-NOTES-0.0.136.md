# Kingmaker Gunslinger 0.0.136

Release: `0.0.136-rapid-reload-proficiency-gate`
Package: `KingmakerGunslinger-0.0.136-rapid-reload-proficiency-gate.zip`
Build label: Kingmaker Gunslinger 0.0.136.
Publication status: published under explicit owner authorization.
Version note: 0.0.135 was already published on 2026-09-21 from the Magic
Circle follow-up line, so this release advanced to 0.0.136 rather than
replacing it.

The qualified firearm SoundBank is unchanged, SHA-256
`0E9F88C562F4F937A8941ACE0F241BB31A7ED56B46FBCA549C98F764392EDF18`.

## Read this first

Native runtime qualification for this release is **NOT RUN**. The guarded
`disposable-rapid-reload-proficiency-gate` scenario was written, compiled and
registered, but it has never executed in game: every attempt was refused by the
runtime orchestrator's administrator-elevation guard, which was deliberately
left intact. The owner authorized publication anyway and explicitly **waived**
the in-game gate for this release, exactly as was done for 0.0.119.

What that means in practice:

- A clean Release build, the complete 1,702-case domain suite and strict
  package validation all pass. None of them is evidence of in-game behaviour.
- The gameplay change below has not been observed in a running game.
- Save/load compatibility has **not** been verified; its supported workflow has
  not been executed.

If you prefer only in-game-qualified builds, stay on 0.0.134.

## What changed

Rapid Reload now requires **firearm proficiency**.

- The Rapid Reload selection itself carries one OR-grouped
  (`Prerequisite.GroupType.Any`) firearm-proficiency prerequisite per currently
  published official firearm kind (Pistol, Musket, Blunderbuss). A character
  with no firearm proficiency can no longer acquire Rapid Reload through
  ordinary feat selection or Fighter combat-bonus-feat selection.
- Any valid firearm-proficiency source satisfies it, whatever granted it.
  Class-granted proficiency counts. **No Gunslinger class or archetype
  prerequisite was added**, and the retired Exotic Weapon Proficiency (Firearms)
  compatibility wrapper is neither required nor republished — existing owners of
  that wrapper still qualify through the proficiency it grants.
- The per-firearm restrictions are unchanged: one-handed proficiency unlocks
  only Pistol; two-handed proficiency unlocks only Musket and Blunderbuss.
- The feat's description now states the prerequisite and that class-granted
  firearm proficiency satisfies it.

## What deliberately did not change

- Reload action costs, the Rapid Reload runtime benefit lookup, equipment
  restrictions, class progression and archetype scope.
- Musket Master keeps its automatic level-one Rapid Reload (Musket) grant.
- Every production blueprint GUID and every owned fact.
- Rapid Reload stays published in both the ordinary and Fighter combat-feat
  catalogs. It was not moved into a Gunslinger-only list.

## Compatibility

Optional-mod compatibility is unchanged and was not re-tested for this release.
Craft Magic Items compatibility is unchanged; there is no static
`CraftMagicItems.dll` dependency, and its compatibility profile keeps its
existing NOT-TESTED disposition.

## Existing characters

This is a prospective acquisition change, not a save repair. No save migration
was added and nothing strips Rapid Reload from characters that already own it.
A character who acquired Rapid Reload before this release keeps the feat and its
reload behaviour. Because the change gates *acquisition*, a character without
firearm proficiency will no longer be offered it on new level-ups. A
nonqualifying feat may still appear in the native list with its prerequisite
shown unmet; it cannot be taken.

## Verification status

| Gate | Result |
| --- | --- |
| Version-aware repository validation | PASS |
| Complete domain suite | PASS, 1,702 of 1,702 |
| Clean Release build and build-output validation | PASS |
| Strict standalone UMM package validation | PASS |
| Guarded native scenario | NOT RUN, owner waived |
| Save/load compatibility workflow | NOT RUN |

Historical domain checkpoints of 1,251, 1,288 and 1,325 cases remain archived
under their original releases; the live suite for this release is 1,702 cases.

The scenario itself went through seven rounds of review (R1–R7) covering feat
slot reservation, exact-child tracking across pending build changes, fixture
proficiency validation at the scoring entry points, the native completion gate
before any claimed confirmation, and exception-safe controller ownership and
cleanup. Those corrections are source-reviewed and evaluator-tested; they are
not runtime-verified.

## Install and uninstall

Install with Unity Mod Manager as usual: select the package zip in UMM and let
it deploy, then launch through Steam.

To uninstall, remove the mod through Unity Mod Manager. Characters that own
Rapid Reload keep the feat entry in their build; removing the mod removes the
feat's blueprint along with the rest of the mod's content, so uninstall from a
save you are willing to keep testing, or roll back to 0.0.134 instead.
