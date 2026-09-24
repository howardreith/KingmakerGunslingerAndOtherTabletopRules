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

This section is updated as qualification evidence is recorded. Rows that are
not implemented, not tested, deferred or excluded are listed in
`FAVORED-CLASS-COVERAGE.md`; nothing listed there as NOT RUN has been observed
in game.

## Other compatibility

Other optional-mod compatibility is unchanged and was not re-tested by this
candidate. There is no static `CraftMagicItems.dll`, `ZFavoredClass.dll` or
`CallOfTheWild.dll` dependency; Favored Class and Call of the Wild are read
only by reflection after the exact binary gate passes.

## Source gates

Domain suite: 1,790 deterministic tests at this checkpoint (the count in the
repository validator is authoritative and is updated with every added case).
Historical domain checkpoints of 1,251, 1,288 and 1,325 cases remain archived
under their original releases.

## Installation, update and uninstall

Install like any Unity Mod Manager mod. Favored-class choices are made at
level-up through the Favored Class mod's normal flow; this candidate never
assigns historical favored-class rewards to existing characters.

Before you uninstall this mod or the Favored Class mod, back up your saves.
Characters that took these favored-class options keep them only while both
mods are installed; removing either leaves those saved choices unresolved.
Restore the removed mod rather than editing the save.
