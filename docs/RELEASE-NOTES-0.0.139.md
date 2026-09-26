# Kingmaker Gunslinger 0.0.139

Candidate: `0.0.139-favored-class-integration`
Package: `KingmakerGunslinger-0.0.139-favored-class-integration.zip`
Build label: Kingmaker Gunslinger 0.0.139.
Publication status: local candidate, **not published**. No merge, push, tag or
public release is authorized by this candidate. Its qualification status is
tracked in `FAVORED-CLASS-IMPLEMENTATION-REPORT.md`: **COMPLETE LOCALLY —
AWAITING OWNER REVIEW** (release qualification is the owner's decision).

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

**COMPLETE LOCALLY — AWAITING OWNER REVIEW** (PR #24 candidate). All 30
scheduled rows are implemented and pass their domain tests and guarded native
runs, except G08 and G20, whose optional races have no playable provider
(their counters are published and tested through their other routes). Every
charter scenario family is observed natively; none is partial or blocked. A
fresh-process save reload keeps one subject of every state and mechanic
family and the own persistence case of every remaining scheduled row. The
lifecycle lane keeps the affected families through death, polymorph and area
transition, a native respec removes the counters cleanly, and the
owner-authorized host and dependency states (a disabled host, simulated host
defects and a save made with a host that is no longer loaded) behave as the
charter requires. Details are in `FAVORED-CLASS-IMPLEMENTATION-REPORT.md`,
`FAVORED-CLASS-COVERAGE.md` and `docs/FAVORED-CLASS-TARGET-MANIFEST.md`.

What a player gets with the qualified Favored Class and Call of the Wild
profile:

- The Gunslinger favored-class options for every race that has them. The Jon
  Brazer Enterprises options stay off unless `thirdParty` is enabled.
- Ifrit, Oread, Sylph and Undine favored-class options in the Alchemist,
  Inquisitor, Rogue, Fighter, Monk, Cleric, Paladin, Ranger, Summoner,
  Sorcerer, Oracle and Bard menus. These include per-revelation Oracle
  counters (65 revelations), per-power Sorcerer counters (Elemental Ray,
  Elemental Blast and Elemental Resistance for the fire and air bloodlines)
  and per-performance Bard range counters (14 performances). An invested
  revelation or power follows its own effective level, including the
  abilities and forms the revelation gains at later oracle levels, Elemental
  Blast's extra daily uses and Elemental Resistance's step to resistance 20;
  nothing else is granted early (Spirit of the Warrior is not offered,
  because its possession would raise base attack bonus). A widened
  performance shows its bard's own range: the area, its ring and the
  performance's description agree, for that bard only. If a ring cannot be
  widened, that bard's live areas of the performance and its descriptions
  all keep the native range; in the rare case that a widened area cannot be
  restored exactly, that area ends and its performance toggle turns off.
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
Favored Class mod's own reward menus keep that mod's presentation.

Fixes to existing Gunslinger behavior in this candidate (the owner decided
D1 and D2 by the tabletop rules; see `FAVORED-CLASS-BLOCKERS.md`):

- A base Gunslinger who has trained all three firearm types completes 17th
  level: the Gun Training pick is no longer obligatory once nothing is left
  to train.
- Dead Shot can score a critical hit: it threatens from its natural roll and
  makes one confirmation at the highest attack bonus minus 5 (plus 1 per
  extra threat, at most 0) with every other confirmation bonus, against the
  target's touch AC when the firearm's touch rule applies.
- With True Grit, Gunslinger's Dodge is refused at 0 grit and free at 1.
- A companion's stored level plan (auto-level) keeps a favored-class reward
  whose target the same level chooses.

The one open owner question is a pre-existing deed behavior outside the
favored-class rows: Pistol-Whip does not add the firearm's enhancement bonus
to its attack roll (D6).

## Other compatibility

Other optional-mod compatibility is unchanged and was not re-tested by this
candidate. There is no static `CraftMagicItems.dll`, `ZFavoredClass.dll` or
`CallOfTheWild.dll` dependency; Favored Class and Call of the Wild are read
only by reflection after the exact binary gate passes.

## Source gates

Domain suite: 1,846 deterministic tests at this checkpoint (the count in the
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
choices incomplete. If you load a save whose characters need the Favored
Class mod (or Call of the Wild) while it is missing or disabled, the game
refuses to load it and nothing is changed; this mod then explains which mod
is missing and how to recover. Restore the removed mod rather than editing
the save, and do not save over the game until it is restored.
Turning the integration off in `FavoredClassIntegration.json` is the
supported way to suspend its effects without breaking saves.
