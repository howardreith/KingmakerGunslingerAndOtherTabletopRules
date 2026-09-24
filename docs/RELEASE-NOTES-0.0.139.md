# Kingmaker Gunslinger 0.0.139

Candidate: `0.0.139-favored-class-integration`
Package: `KingmakerGunslinger-0.0.139-favored-class-integration.zip`
Build label: Kingmaker Gunslinger 0.0.139.
Publication status: local candidate, **not published**. No merge, push, tag or
public release is authorized by this candidate. Its qualification status is
tracked in `FAVORED-CLASS-IMPLEMENTATION-REPORT.md`; until that report says
otherwise it is **PARTIAL — NOT RELEASE QUALIFIED**.

The qualified firearm SoundBank is unchanged, SHA-256
`0E9F88C562F4F937A8941ACE0F241BB31A7ED56B46FBCA549C98F764392EDF18`.

## What changes

An optional integration with Holic75's **Favored Class** mod, following the
Gunslinger Favored Class Integration Charter:

- Gunslinger favored-class options for the races that have them (for example
  Human grit, Dwarf misfire reduction, Elf firearm confirmation), added to the
  Favored Class mod's own Gunslinger reward menu.
- Favored-class options for the Ifrit, Oread, Sylph and Undine races in the
  classes their published tables cover.
- A **Mostly Human** alternate racial trait for the four elemental races.

Favored Class stays optional. Without it, or with any binary other than the
exact qualified Favored Class 1.3.1 / Call of the Wild 1.14.4c-2.1 profile,
the Gunslinger class, firearms and elemental races work as before and no new
favored-class choices appear.

## Status

**PARTIAL — NOT RELEASE QUALIFIED** (local candidate, owner review pending).
All 30 scheduled rows are implemented and pass their domain tests and guarded
native runs, except G08 and G20, whose optional races have no playable
provider. Every required infrastructure item is implemented; the lifecycle
policy is still partial and the migration guidance is documented only (see
the report). A fresh-process save reload keeps one subject of every state and mechanic
family: partial and full investment, spent grit with its raised maximum, the
selected firearm, performance, revelation and bloodline targets, Nimble, the
Undine Monk, owner-local performance and aura areas, companion armor and its
replacement, and Mostly Human. The lifecycle lane keeps the affected families
through death, polymorph and area transition, and a native respec removes
the counters cleanly. Fourteen rows have no persistence case of their own yet
(see the report). Details are in
`FAVORED-CLASS-IMPLEMENTATION-REPORT.md`, `FAVORED-CLASS-COVERAGE.md` and
`docs/FAVORED-CLASS-TARGET-MANIFEST.md`. Nothing listed there as NOT RUN has
been observed in game.

What a player gets with the qualified Favored Class and Call of the Wild
profile:

- The Gunslinger favored-class options for every race that has them. The Jon
  Brazer Enterprises options stay off unless `thirdParty` is enabled.
- Ifrit, Oread, Sylph and Undine favored-class options in the Alchemist,
  Inquisitor, Rogue, Fighter, Monk, Cleric, Paladin, Ranger, Summoner,
  Sorcerer, Oracle and Bard menus. These include per-revelation Oracle
  counters (52 revelations), per-power Sorcerer counters and per-performance
  Bard range counters (14 performances). A widened performance shows its
  bard's own range: the area, its ring and the performance's description
  agree, for that bard only.
- **Mostly Human**: an optional Heritage-phase choice for the four elemental
  races. The character counts as both a human (humanoid) and its elemental
  race (native outsider) for effects related to race, such as the Favored
  Class mod's human options and human race traits. Its race, racial traits
  and ability scores are unchanged, and it still receives one favored-class
  bonus per level.
- An optional `FavoredClassIntegration.json` (restart-required) that can turn
  the integration or any profile off; see
  `docs/FAVORED-CLASS-COMPATIBILITY.md`.

Every published KMG favored-class choice and the Mostly Human selectors use
an appropriate existing icon (original art is optional later polish); the
Favored Class mod's own reward menus keep that mod's presentation. Open
questions for the owner are limited to two pre-existing KMG defects and are
listed in `FAVORED-CLASS-BLOCKERS.md`.

## Other compatibility

Other optional-mod compatibility is unchanged and was not re-tested by this
candidate. There is no static `CraftMagicItems.dll`, `ZFavoredClass.dll` or
`CallOfTheWild.dll` dependency; Favored Class and Call of the Wild are read
only by reflection after the exact binary gate passes.

## Source gates

Domain suite: 1,807 deterministic tests at this checkpoint (the count in the
repository validator is authoritative and is updated with every added case).
Historical domain checkpoints of 1,251, 1,288 and 1,325 cases remain archived
under their original releases.

## Installation, update and uninstall

Install like any Unity Mod Manager mod. Favored-class choices are made at
level-up through the Favored Class mod's normal flow; this candidate never
assigns historical favored-class rewards to existing characters.

Before you uninstall this mod or the Favored Class mod, back up your saves.
KMG's own favored-class choices stay registered while this mod is installed,
even without the Favored Class mod, but the favored-class progression itself
belongs to the Favored Class mod. Removing either mod leaves those saved
choices incomplete. Restore the removed mod rather than editing the save.
Turning the integration off in `FavoredClassIntegration.json` is the
supported way to suspend its effects without breaking saves.
