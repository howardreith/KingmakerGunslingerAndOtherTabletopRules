# Elemental Races blockers

## Active hard blockers

None established as of 2026-09-02. Production identities, base-rule
blueprints, resources, energy resistance, Keen Senses, affinity, total-level
SLA parameters, native donor delivery, Stone Fist and Feather Step expiry,
Hydraulic Push combat resolution, rest restoration, and request-local resource
persistence now pass guarded runtime checks. Oread movement, visuals,
save-backed persistence, and compatibility qualification remain incomplete;
absence of a hard blocker is not a completion claim.

## Resolved reconnaissance risks

- A stable request-owned `BlueprintRace` can register in both indexes, remain
  absent from `CharacterRaces`, serialize by reference, and roll back exactly.
- Native character generation accepts the diagnostic race and applies its
  clone-only racial fact.
- Human-compatible male and female donor dolls render with complete
  materials/shaders while preserving the accepted Gunslinger outfit catalog.
- No foreign live Ifrit, Oread, Sylph, or Undine blueprint was found after all
  24 production GUIDs were registered in the guarded loaded stack. Exact KMG
  identities resolved in both live indexes without collision.
- Races Unleashed identity and append-at-`LoadDictionary` publication behavior
  are known. Its races must be treated as an ordered live prefix.
- Exact installed-mod inventory found no Visual Adjustments installation, so
  its machine-local compatibility observation will remain `NOT-RUN` unless the
  local state changes.
- Actual native `RuleDealDamage` proved resistance 5 for all four energy types.
  Native ability-parameter events proved matching affinity +1 exactly once and
  nonmatching +0 after replacing an unsafe `Int64`-backed enum component field
  discovered by the first failed mechanics run.
- Actual 2 Fighter / 3 Wizard level-up, resource spend, native rest, and
  resource-record serialization proved total character-level scaling,
  once-per-rest accounting, and exact spent-state identity/amount round-trip.
- Native command execution proved Burning Hands geometry/save/damage, Stone
  Fist buff delivery/unarmed replacement/expiry, and Feather Step buff
  delivery/expiry. Cancellation, committed spend, second-use gating, and rest
  restoration passed for all three donor abilities.
- Actual Hydraulic Push command execution proved best-mental selection,
  all-negative and tie handling, total-level formula, ordinary Bull Rush
  success/failure, immunity, native force movement, and absence of an unrelated
  attack roll, saving throw, or Bull-Rush-created opportunity attack. The final
  resource strategy preserves ordinary availability gating and spends exactly
  once at synchronous effect commitment.

## Open risks requiring evidence

- The selected donor is `RaceId.Aasimar` with the exact native Outsider fact.
  Actual Hold/Charm/Enlarge/Reduce Person, prerequisite, level-up, respec, and
  donor-dialogue observations remain pending.
- Native Dwarf Slow and Steady and Outsider facts are selected. Save-backed
  SLA reload and armored or encumbered Oread movement remain pending.
- Hydraulic Push combat mechanics and resource commitment are qualified. A
  safe native water projectile has not yet been selected, and save-backed
  persistence remains pending.
- Vanilla head, hair, skin, eye, eyebrow, beard, horn, body, color-profile, and
  class/equipment compatibility donors remain unaudited for the four races.
- Complete Aasimar fallback arrays render and preserve Human skeleton/body
  compatibility. Distinctive per-race donor IDs, profiles, original ramps, and
  fallback combinations remain to be audited and implemented.

These are investigation items, not hard stops. Change strategy and continue
while a safe, reversible evidence path remains.
