# Kingmaker Gunslinger 0.0.137

Release: `0.0.137-rapid-reload-combat-feat`
Package: `KingmakerGunslinger-0.0.137-rapid-reload-combat-feat.zip`
Build label: Kingmaker Gunslinger 0.0.137.
Publication status: published under explicit owner authorization.

The qualified firearm SoundBank is unchanged, SHA-256
`0E9F88C562F4F937A8941ACE0F241BB31A7ED56B46FBCA549C98F764392EDF18`.

## What changed

Rapid Reload is now a **combat feat**.

- The Rapid Reload selection is classified as both a feat and a combat feat,
  like its Pistol, Musket and Blunderbuss choices and native combat feats such
  as Combat Reflexes. Before this release its own classification was empty.
  The groups it did carry only describe what a selection offers, not what the
  selection is.
- A character with firearm proficiency can take Rapid Reload with a Fighter
  bonus combat feat. For example, a Gunslinger 1 taking a first Fighter level
  finds Rapid Reload in that level's Fighter bonus-feat menu and takes it with
  the Fighter slot, not an ordinary feat.
- The firearm-proficiency requirement from 0.0.136 is unchanged. A character
  without firearm proficiency still cannot take Rapid Reload, including
  through a Fighter bonus feat. One-handed proficiency still unlocks only
  Pistol, and two-handed proficiency only Musket and Blunderbuss.

## What deliberately did not change

- The proficiency requirement, per-firearm prerequisites, ranks and
  duplicate-ownership rules.
- Which feat lists Rapid Reload appears in. It stays once in the ordinary feat
  list and once in the Fighter combat-feat list. Curated class or style lists
  were not touched.
- Musket Master's automatic Rapid Reload (Musket), reload action costs, icons,
  text and every blueprint GUID.
- Rifle and Revolver choices and the legacy Exotic Weapon Proficiency
  (Firearms) wrapper stay unpublished.

## Verification status

The guarded `disposable-rapid-reload-proficiency-gate` scenario ran in game on
the candidate and passed all eleven of its assertions. This is also the first
native PASS for the 0.0.136 proficiency gate. The owner authorized this
release.

| Gate | Result |
| --- | --- |
| Version-aware repository validation | PASS |
| Complete domain suite | PASS, 1,704 of 1,704 |
| Clean Release build and build-output validation | PASS |
| Strict standalone UMM package validation | PASS |
| Combat-feat classification in game | PASS |
| Gunslinger 1 → Fighter 1 bonus-slot acquisition in game | PASS |
| Guarded native scenario, all 11 assertions | PASS |
| Save/load compatibility workflow | NOT RUN |

Historical domain checkpoints of 1,251, 1,288 and 1,325 cases remain archived
under their original releases; the live suite for this release is 1,704 cases.

The first native runs showed that three checks written with the 0.0.136
scenario expected things the game does not do. At the owner's direction they
were corrected before the passing run:

- Choosing Rapid Reload puts the selection on the preview character before a
  firearm is picked, as for any selection feat. The check now requires that no
  firearm is granted and that the level cannot be finished until a legal
  firearm is chosen.
- A deliberate test probe that forces an unfinished level through now only has
  to show that no firearm is banked. A player cannot reach that state.
- The class-change test now refunds skill points that became overspent when
  the pending class changed, as a player would, before confirming the level.

## Compatibility

Optional-mod compatibility is unchanged and was not re-tested for this release.
Craft Magic Items compatibility is unchanged; there is no static
`CraftMagicItems.dll` dependency, and its compatibility profile keeps its
existing NOT-TESTED disposition.

## Existing characters

This is a classification change, not a save repair. No migration was added,
and characters keep every feat they own. A Rapid Reload already taken now
reports itself as a combat feat.

## Install and uninstall

Install with Unity Mod Manager as usual: select the package zip in UMM and let
it deploy, then launch through Steam.

To uninstall, remove the mod through Unity Mod Manager. Characters that own
Rapid Reload keep the feat entry in their build. Removing the mod removes the
feat's blueprint along with the rest of the mod's content, so uninstall from a
save you are willing to keep testing, or roll back to 0.0.136 instead.
