# Kingmaker Gunslinger 0.0.114 Elemental Races preview

Candidate asset:
`KingmakerGunslinger-0.0.114-elemental-races-preview.zip`

This preview adds Ifrit, Oread, Sylph, and Undine as four distinct playable
races. Elemental Races defaults OFF. Enable **Elemental Races: Ifrit, Oread,
Sylph, and Undine (preview)** in Unity Mod Manager and restart Kingmaker
completely. The checkbox is restart-bound and never rebuilds a live blueprint
graph.

## Rules

- Ifrit: +2 Dexterity, +2 Charisma, -2 Wisdom, speed 30 feet, fire
  resistance 5, Keen Senses, +1 DC with Fire spells, and Burning Hands once
  per rest.
- Oread: +2 Strength, +2 Wisdom, -2 Charisma, speed 20 feet, native Slow and
  Steady, acid resistance 5, Keen Senses, +1 DC with Acid spells, and native
  Stone Fist once per rest.
- Sylph: +2 Dexterity, +2 Intelligence, -2 Constitution, speed 30 feet,
  electricity resistance 5, Keen Senses, +1 DC with Electricity spells, and
  native Feather Step once per rest.
- Undine: +2 Dexterity, +2 Wisdom, -2 Strength, speed 30 feet, cold resistance
  5, Keen Senses, +1 DC with Cold spells, and Hydraulic Push once per rest.

Every racial ability is spell-like, consumes no spell slot, and uses total
character level as caster level. The ordinary racial SLA save calculation is
Charisma-based. Hydraulic Push has no save and resolves an ordinary Bull Rush
using total character level plus the best Intelligence, Wisdom, or Charisma
modifier.

Kingmaker adaptations are deliberate: Keen Senses and exactly +2 racial
Perception replace darkvision; affinity grants DC only, not caster level;
Feather Step replaces Feather Fall; and Undine swimming clauses have no
mechanical effect. The races use `RaceId.Aasimar` for character-doll and
equipment compatibility, matching the installed Aasimar/Tiefling person-spell
precedent. RaceId-only dialogue can therefore call an elemental character
Aasimar, while exact race-blueprint prerequisites remain distinct.

## Save safety and visuals

All four race blueprints and their features, abilities, one-use resources,
active effects, visual blueprints, and equipment proxies register even when
the module is OFF. OFF hides the races from new-character and respec selectors
after restart; it does not strip an existing character. Selector publication
appends all four together in Ifrit/Oread/Sylph/Undine order, preserves every
existing entry, is idempotent, and rolls back to the exact prior array on
failure.

The visual set uses only referenced vanilla Kingmaker modular assets plus
project-owned stable proxies and deterministic native-contract color ramps.
It contains no original body mesh, extracted Owlcat texture, copied
third-party asset, custom particle system, or new runtime dependency. A
complete Aasimar-compatible donor appearance is the fail-safe fallback.

Do not uninstall the whole mod from a campaign containing KMG identities.
Visual Adjustments was absent from the qualification machine and is NOT-RUN.
Kingmaker exposes no race-level eye-color choice array. Optional proxied parts
may not appear as separate Visual Adjustments entries. Clipping and overall
appearance are subjective and remain HUMAN REVIEW REQUIRED.

## Qualification summary

The dependency-free suite contains 1,390 tests. Source qualification covers
all 2,048 eleven-module combinations and the runtime boundary policy contains
exactly 24 configurations. Guarded in-game checks cover rules and native SLA
delivery, Oread movement, Hydraulic Push, selector publication, 56 production
visual combinations, ten-class clothing, expanded equipment, native motion
and state transitions, three-process module-OFF save persistence, Races
Unleashed coexistence, and the existing Human Gunslinger outfit regression.
Exact final-candidate artifact hashes and transaction IDs are recorded in
`ELEMENTAL-RACES-IMPLEMENTATION-REPORT.md`.

The preview retains the 0.0.113 read-only paper-cartridge save-load repair and
the accepted Gunslinger Magus-derived class outfit. It also preserves the
owner-accepted firearm SoundBank byte identity:
`0E9F88C562F4F937A8941ACE0F241BB31A7ED56B46FBCA549C98F764392EDF18`.
Optional Craft Magic Items support remains reflection-only:
`CraftMagicItems.dll` is neither linked nor packaged. The retained 0.0.103
overhaul/summoning/fatigue checkpoint contained 1,288 deterministic tests; its
source contracts remain part of the inherited release gate. The later fatigue
authority release-note checkpoint recorded 1,325 tests and is likewise
retained.
