# Elemental Races blockers

## Active hard blockers

None established as of 2026-09-02. Production identities, base-rule
blueprints, resources, energy resistance, Keen Senses, affinity, total-level
SLA parameters, native donor delivery, Stone Fist and Feather Step expiry,
Hydraulic Push combat resolution, rest restoration, and request-local resource
persistence now pass guarded runtime checks. Native Oread movement,
Aasimar/Tiefling creature-type behavior, and module ON/OFF publication also
pass guarded runtime checks. The complete eight-race native visual donor and
palette inventory also passes guarded runtime checks. Production visual
proxies and the 56-case race/sex/option renderer matrix now pass guarded
runtime checks. The 128-state elemental Gunslinger outfit/equipment/rebuild
matrix also passes guarded runtime checks. Broader class clothing, remaining
equipment slots, motion, save-backed persistence, compatibility, and human
visual qualification remain incomplete;
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
- Actual medium and heavy armor and a calculated heavy equipped load left Oread
  and native Dwarf at 20 feet while the Human control fell from 30 to 20.
  Request-local +10 and -5 generic modifiers otherwise applied normally.
- Installed Human, Aasimar, Tiefling, and all four elemental races were tested
  with native Hold Person, Charm Person, Enlarge Person, Reduce Person,
  `PrerequisiteFeature`, and `PrerequisiteNoFeature`. The installed donor races
  and their first heritage facts do not grant `OutsiderType`; the production
  races now deliberately match that native behavior while retaining distinct
  exact-`BlueprintRace` prerequisite identities.
- Fresh module-ON and module-OFF processes proved 24 identities remain
  registered in both states. ON published one contiguous Ifrit/Oread/Sylph/
  Undine sequence without shared-catalog duplicates; OFF published none.
- The guarded donor inventory resolved all 358 declared head, hair, eyebrow,
  beard, horn, tail-palette, body, and preset links across Human, Aasimar,
  Tiefling, Elf, Dwarf, Half-Elf, Half-Orc, and Gnome. It proved at least two
  heads and four hair choices per sex, complete native fallback presets, and a
  common 256x1 RGB24/bilinear/clamp ramp contract. The initial fixture-only
  assumption that every donor supplies eyebrows was corrected after Half-Orc
  accurately reported none.
- All 16 production visual blueprints and 28 project-owned equipment proxies
  resolve exactly. Fifty-six production doll cases cover all races, sexes,
  body presets, customization options, seven skin indexes, and at least four
  hair-color indexes with complete baked renderers, materials, and shaders.
  Native body resolution through `RacePreset.Skin`, post-bake equipment-list
  clearing, and null-versus-empty optional choices were observed and encoded
  without weakening mandatory asset checks. No shared race or blueprint index
  changed during the save-free scenario.
- Eight production Gunslinger dolls (four races, both sexes) passed all 128
  accepted equipment/rebuild states: sex-specific Magus-derived class clothes,
  firearms held and stored, light/heavy armor, headgear/hair, cloak, backpack,
  class colors, repeated rebuild, exact fixture restoration, and unchanged
  production class/shared-unit state. The guarded scenario made no save call;
  generated images remain supporting evidence only.

## Open risks requiring evidence

- The selected donor remains `RaceId.Aasimar`. Exact person-spell and type/race
  prerequisite behavior now passes, but donor-dialogue observations, level-up,
  respec, and save-backed reload remain pending.
- Native Dwarf Slow and Steady passes the live armor/encumbrance matrix.
  Save-backed SLA reload and ordinary character persistence remain pending.
- Hydraulic Push combat mechanics and resource commitment are qualified. A
  safe native water projectile has not yet been selected, and save-backed
  persistence remains pending.
- Native head, hair, skin, eyebrow, beard, horn, body, preset, and color-ramp
  donors are inventoried and production proxies/curated combinations pass the
  complete option renderer matrix and the elemental Gunslinger equipment
  matrix. Kingmaker has no race-level eye-color field in
  CustomizationOptions; native head/material eyes and that limitation need
  human review. The nine additional base-game classes, medium armor, robes,
  accessory slots, and full motion matrix remain to be qualified.
- Complete Aasimar fallback arrays render and preserve Human skeleton/body
  compatibility. Production visuals will initially constrain geometry to the
  runtime-proven Human-compatible Human/Aasimar/Tiefling skeleton family;
  palettes from other donors may be reused through their audited native ramp
  resources without copying textures.

These are investigation items, not hard stops. Change strategy and continue
while a safe, reversible evidence path remains.
