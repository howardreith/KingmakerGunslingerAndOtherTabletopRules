# Elemental Races acceptance checklist

## Automated source and package gates

- [x] Four stable, noncolliding race GUIDs and every save-bearing child identity
  are active in the authoritative blueprint manifest.
- [x] Identities register with the module both ON and OFF.
- [x] Atomic publication appends Ifrit, Oread, Sylph, and Undine exactly once,
  preserves the complete prior array and order, and restores it exactly on
  failure.
- [x] Feature settings migrate schemas 0 through 9 to schema 10, preserve every
  explicit value, default absent Elemental Races OFF, round-trip schema 10, and
  reject future schemas.
- [ ] The eleven-module source matrix has 2,048 exhaustive states and the
  runtime boundary matrix has exactly 24 states.
- [ ] Focused rules, SLA, persistence, manifest, localization, rollback,
  package-content, and compatibility tests pass.
- [x] Complete repository validation and dependency-free domain suite pass at
  the current native-identity/publication checkpoint (1,385/1,385).
- [x] Clean Release/package command and strict package validation pass at the
  current base-mechanics checkpoint.
- [x] Guarded Steam App ID 640820 base-mechanics, native donor-SLA, Hydraulic
  Push, native identity/movement, and ON/OFF publication scenarios record exact
  build identity and structured evidence without touching protected saves;
  persistence, visuals, and compatibility remain separately unchecked below.
- [ ] KMG-alone, Call of the Wild, Races Unleashed, combined, highest-risk, and
  Visual Adjustments-if-installed profiles are qualified and restored exactly.
- [ ] Existing Gunslinger outfit, firearm visual/mechanical, feature-module,
  bootstrap, respec, save-hydration, and 0.0.113 repair regressions pass.

## Owner human acceptance for the exact candidate package

- [ ] Enable **Elemental Races: Ifrit, Oread, Sylph, and Undine** in UMM.
- [ ] Restart Kingmaker completely.
- [ ] Open character creation and confirm all four races appear exactly once.
- [ ] Inspect male and female models for every race and representative bodies.
- [ ] Cycle every offered head, hair, skin, eye, eyebrow, beard, and horn choice.
- [ ] Create one character of each race.
- [ ] Verify final ability scores, speed, resistance, Keen Senses, affinity,
  and racial SLA.
- [ ] Use the SLA once, confirm the second use is unavailable, rest, and use it
  again.
- [ ] Equip representative robes, light/medium/heavy armor, helmet, cloak,
  boots, gloves/bracers, belt, pistol, musket, and blunderbuss.
- [ ] Create or respec an elemental Gunslinger and inspect the accepted
  Magus-derived Gunslinger outfit without changing its existing presentation.
- [ ] Observe idle, walk, run, attack, firearm fire/reload, spellcasting, prone,
  death, rebuild/resurrection, polymorph return, level-up, and respec surfaces.
- [ ] Save and reload the authorized disposable elemental character.
- [ ] Repeat selector coexistence with Races Unleashed enabled and confirm all
  third-party and elemental races remain present once and in preserved order.
- [ ] Record clipping, aesthetics, unavailable combinations, or missing options.

Subjective visual acceptance remains **HUMAN REVIEW REQUIRED** until the owner
reviews the exact packaged artifact. Structural runtime checks cannot satisfy
that gate.
