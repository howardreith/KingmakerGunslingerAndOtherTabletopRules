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
- [x] The eleven-module source matrix has 2,048 exhaustive states and the
  runtime boundary matrix has exactly 24 states.
- [x] Focused rules, SLA, persistence, manifest, localization, rollback, and
  package-content tests pass.
- [x] Focused compatibility tests pass.
- [x] Complete repository validation and dependency-free domain suite pass at
  the final engineering transition checkpoint (1,390/1,390).
- [x] Clean Release/package command and strict package validation pass on the
  exact version-0.0.114 native-Respec persistence candidate.
- [x] Guarded Steam App ID 640820 base-mechanics, native donor-SLA, Hydraulic
  Push, native identity/movement, and ON/OFF publication scenarios record exact
  build identity and structured evidence without touching protected saves;
  compatibility is qualified separately below.
- [x] All 16 production visual blueprints and 28 stable equipment proxies
  resolve; 56/56 production race/sex/option cases cover every preset, offered
  option, seven skin indexes, and four or more hair colors with complete baked
  materials and exact graph cleanup.
- [x] Eight elemental Gunslinger fixtures (four races, both sexes) pass all
  224 expanded outfit/equipment/rebuild records across 28 states each with 8/8
  exact restorations, unchanged production class/shared-unit state, and no
  save API call.
- [x] All 80 race/sex/class combinations across Gunslinger, Fighter, Rogue,
  Ranger, Alchemist, Magus, Wizard, Cleric, Monk, and Kineticist resolve their
  exact native class clothes and render complete materials/shaders without
  touching selector or save state.
- [x] Eight elemental Gunslinger fixtures pass 216/216 native idle, walk, run,
  turn, pistol/musket attack, production reload, and shortsword-melee records;
  the unchanged two-Human mode passes its 54/54-record regression matrix.
- [x] The same eight fixtures pass 64/64 additional native racial-SLA,
  prone, death/resurrection, and Beast Shape II/return transition records.
- [x] Three fresh guarded launches prove eight save-backed race/sex fixtures,
  spent SLA state, module-OFF identity/fact/visual/outfit reload, rest
  restoration, level-up and caster level two, exact fixture cleanup, and exact
  FeatureModules byte restoration.
- [x] KMG-alone, Call of the Wild, Races Unleashed, combined, and highest-risk
  profiles pass on the version-0.0.113 engineering artifact with exact
  transaction restoration. Visual Adjustments is NOT-RUN because it is not
  installed.
- [x] All 24 boundary states pass again on one exact version-0.0.114 candidate
  artifact, with expected/active equality, zero warnings, and exact settings
  restoration.
- [x] All five compatibility profiles pass again on one exact version-0.0.114
  candidate artifact: 18/18 nested guarded runs, zero warnings, and exact
  mod/settings restoration.
- [x] Existing Gunslinger outfit, firearm visual/mechanical, feature-module,
  bootstrap, save-hydration, and 0.0.113 repair regressions pass.
- [x] The implemented eight-fixture native replacement/respec gate preserves
  race, facts, SLA, appearance, and the accepted Gunslinger outfit in an
  exact-0.0.114 guarded three-process persistence run, including module-OFF
  reload, rest, level-up, cleanup, and fresh-process absence.
- [x] The final clean Release/package pipeline and a separate strict UMM
  validation pass for the 135-entry human-review package. The exact-reference
  rebuild from `b19bc04f` is byte-identical to the guarded deployment and
  passes fresh core-load and Elemental/Races Unleashed observations: ZIP
  SHA-256
  `bd2edc600916f636bee9e5a3640e1a82e175fffdfea1ba82367d37458ab5d334`;
  DLL SHA-256
  `670d0ef39b2ede7b28741a1e260f5c63a2728655939c1c494e93bd709fe95273`;
  MVID `5ecac105-15ca-4b48-becd-789fee85c144`.

## Owner human acceptance for the exact candidate package

Install
`artifacts/packages/KingmakerGunslinger-0.0.114-elemental-races-preview.zip`
with the exact ZIP hash recorded above, then:

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
