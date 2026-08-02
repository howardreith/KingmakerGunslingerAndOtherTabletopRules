# Sprint 53 entry criteria: Evasive

## Authority

At Gunslinger level 15, while current grit is at least 1, Evasive grants the
Rogue Evasion, Uncanny Dodge, and Improved Uncanny Dodge class features. The
gunslinger uses Gunslinger level as Rogue level for Improved Uncanny Dodge.

## Installed-contract gate

The exact installed Kingmaker feature candidates are identified by stable game
GUIDs for Evasion, Uncanny Dodge, and Improved Uncanny Dodge. Before registering
the Gunslinger wrapper, a guarded save-free observation must resolve only those
three exact blueprints and record their concrete component types and relevant
serialized fields. Source work must not copy names, infer candidates from a
broad catalog sweep, or assume that native Improved Uncanny Dodge already
counts Gunslinger levels.

## Observable contract

- One Evasive feature appears exactly once at Gunslinger level 15.
- At zero grit none of the three native benefits is active.
- Crossing from zero to positive grit activates exactly the three verified
  native benefits for that unit; returning to zero removes or suppresses them.
- Evasion preserves the native Reflex-save/no-damage behavior.
- Uncanny Dodge preserves the native cannot-be-caught-flat-footed behavior.
- Improved Uncanny Dodge uses the exact installed native `CannotBeFlanked`
  mechanic. Kingmaker 2.1.7b exposes no attacker-level comparison, so the
  tabletop Gunslinger-level substitution has no remaining engine interaction.
- Resource updates, level changes, load reconstruction, and respec refresh the
  unit-local state without sharing facts between units.
- Failures remove only project-owned Evasive grants and fail closed to no
  benefit.

## Qualification

Pure tests cover grit thresholds, transitions, unit isolation, and level
substitution. A guarded save-free feature scenario must then prove progression,
zero/positive/zero transitions, exact native facts, representative native
mechanics, cleanup, and external isolation. The exact assembly requires mod
load and two independent feature PASS runs.

## Non-goals

Sprint 53 does not alter the native Rogue features, grant Improved Evasion,
change grit costs or recovery, or implement Menacing Shot or later deeds.
