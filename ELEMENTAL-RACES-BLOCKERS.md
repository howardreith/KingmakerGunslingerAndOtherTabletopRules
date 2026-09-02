# Elemental Races blockers

## Active hard blockers

None established as of 2026-09-02. The development probe passed, but production
rules, visuals, persistence, and compatibility qualification remain incomplete;
absence of a hard blocker is not a completion claim.

## Resolved reconnaissance risks

- A stable request-owned `BlueprintRace` can register in both indexes, remain
  absent from `CharacterRaces`, serialize by reference, and roll back exactly.
- Native character generation accepts the diagnostic race and applies its
  clone-only racial fact.
- Human-compatible male and female donor dolls render with complete
  materials/shaders while preserving the accepted Gunslinger outfit catalog.
- No live Ifrit, Oread, Sylph, or Undine blueprint was found in the guarded
  loaded compatibility stack. Production GUID collision checks are still
  required after those GUIDs are allocated.
- Races Unleashed identity and append-at-`LoadDictionary` publication behavior
  are known. Its races must be treated as an ordered live prefix.
- Exact installed-mod inventory found no Visual Adjustments installation, so
  its machine-local compatibility observation will remain `NOT-RUN` unless the
  local state changes.

## Open risks requiring evidence

- Final donor `RaceId` semantics for person-only spells, dialogue,
  prerequisites, level-up, and respec are not yet established. Human is proven
  doll/serialization-compatible; native Aasimar/Tiefling outsider behavior is
  still the authority for the final decision.
- Native Kingmaker authority for Stone Fist, slow movement, racial outsider
  facts, the four resistances, and SLA resource/rest behavior is not yet
  selected.
- Hydraulic Push requires an exact, narrow Bull Rush rule path and commit-time
  resource semantics; no implementation is selected yet.
- Production project GUIDs have not been allocated. Collision checks against
  the base cache and installed compatibility stack remain pending for them.
- Vanilla head, hair, skin, eye, eyebrow, beard, horn, body, color-profile, and
  class/equipment compatibility donors remain unaudited for the four races.
- Exact vanilla visual-option donor IDs, profiles, and fallback combinations
  remain to be audited beyond the Human diagnostic path.

These are investigation items, not hard stops. Change strategy and continue
while a safe, reversible evidence path remains.
