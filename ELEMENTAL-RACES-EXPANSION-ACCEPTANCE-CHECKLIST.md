# Elemental Races expansion acceptance checklist

Unchecked items are pending and must not be inferred from builds or historical
0.0.114 evidence.

## Baseline and foundation

- [x] Read repository policy and current Elemental Races, release,
  compatibility, runtime, manifest, and architecture records.
- [x] Fetch authoritative master and record the exact starting SHA.
- [x] Create a clean descendant branch with no unrelated local changes.
- [x] Baseline repository validation passes.
- [x] Baseline complete domain/reflection suite passes (1,390/1,390).
- [x] Baseline clean Release build and strict package validation pass.
- [x] Affinity affects one matching ordinary spell exactly once, including
  variants/parents, and affects no SLA, kinetic, supernatural, item, weapon, or
  arbitrary descriptor-bearing ability.
- [x] Every save-bearing racial SLA proves exact DC/CL, multiclass/current-stat
  behavior, cancellation/commit/zero/rest/load/level-up/module-OFF behavior.
- [x] Oread movement preserves native modifier layering.
- [x] Hydraulic Push preserves current total-level/best-mental formula and its
  native maneuver, immunity, movement, resource, and cancellation behavior.
- [x] Shared-catalog, cache, visual ownership, and rollback audits complete;
  any concrete defect has a failing regression first and a narrow repair.
- [x] New runtime scenarios are feature-specific; central runner changes are
  dispatch/registration only.
- [x] Foundation validation, full suite, clean build, and package validation
  pass before Release A source work begins.

## Release A / 0.0.115 elemental heritages

- [x] Four parent-race heritage selections exist with exactly General plus two
  alternate choices and no new top-level race.
- [x] Native selection and live reconciliation prove all twelve exact stat
  arrays and provider/resource uniqueness, including marker-first hydration,
  add-before-remove transitions, idempotence, spent-use preservation through
  reconciliation, level-up without refill, and ordinary-rest refill.
- [x] All twelve choices have exact final statistics, active affinity
  presentation/mechanics, SLA/resource, descriptions, and non-null icons.
- [x] Legacy General race/SLA/resource GUIDs and spent state remain exact;
  absence of an alternate marker behaves as General without duplicated stats.
- [x] Heritage SLA substitutions follow exact native spell, verified Owlcat
  precedent, or narrow faithful project-owned implementation order and are
  recorded in the deviation matrix.
- [x] Fresh character, native respec transitions, save/load, module-OFF/ON,
  rest, level-up, both-sex/all-preset visual and equipment reconstruction pass
  for all 24 race/sex/heritage fixtures.
- [x] Three-process persistence plus fourth-process absence passes with no stat
  drift, duplicate provider, lost fact, restored spent use, appearance drift,
  fixture leak, or selector corruption.
- [x] Death/resurrection, polymorph/return, optional profiles, and exact
  0.0.114 legacy migration pass.
- [x] Version/docs/manifest updated; repository validation, full tests, clean
  0.0.115 build, strict package validation, guarded runtime, compatibility,
  and persistence gates pass.
- [ ] Coherent Release A implementation and qualification commits were created;
  the exact mandated push was attempted after every checkpoint but remains
  blocked by the external branch allowlist.

## Release B / 0.0.116 elemental feats

- [x] A save-free guarded native-contract audit isolated KMG from optional
  mods and pinned exact maneuver, Wings, concealment, fire-enchantment, and
  Small Water Elemental identities/fields; the original environment was
  restored byte-for-byte.
- [x] The active version is 0.0.116 and all 25 Release B save-bearing
  identities have fixed manifest GUIDs, unconditional registration, exact
  blueprint types, and a passing repository/full-suite/clean-package
  checkpoint. This does not qualify their pending gameplay mechanics.
- [x] Feat identities register unconditionally; module-gated publication to
  exact universal/combat selectors is additive, deterministic, idempotent,
  conflict refusing, order preserving, and reversible.
- [x] Exact race/level/feat/SLA prerequisites and all-heritage eligibility pass.
- [x] Elemental Strike implements exact swift-action, one-round, race-energy,
  level-breakpoint, weapon-only, once-per-attack behavior.
- [x] Scorching Weapons and Inner Flame independently pass exact native-command,
  two-metal-weapon snapshot, swap/unequip, damage/replacement, resistance,
  nonstacking, save-bonus replacement, and request-local cleanup mechanics.
- [x] Blazing Aura and Firesight independently pass exact command,
  production turn-handler, friendly-creature, resistance, project-owned
  fire/smoke concealment, native exclusion, Mirror Image, and Dazzled-immunity
  contracts. A live campaign `TurnController` dispatch recheck and Scorching
  item persistence/death cleanup remain part of the release-wide gates.
- [x] Airy Step, Wings of Air, Cloud Gazer, and Inner Breath pass exact save,
  native-flight/armor, curated concealment, and respiration contracts.
- [x] Wings of Air independently passes its exact base-Owlcat native-flight
  semantics, melee/ranged AC, no/light/medium armor gating, legal armor-removal
  restoration, difficult-terrain/Ground behavior, and prone exclusion.
- [x] Hydraulic Maneuver and Triton Portal pass exact variant/formula/shared-
  resource/action/summon/module-boundary behavior; unsupported Dirty Trick is
  recorded honestly if no native path exists.
- [x] Intentionally deferred feats remain unimplemented and are documented as
  exact engine/value omissions without compensatory bonuses.
- [x] Three-process persistence covers all 24 race/sex/heritage fixtures,
  feat facts, granted abilities, resources, Wings buffs, a command-created
  Elemental Strike buff, two exact command-enchanted weapon references,
  module-OFF/ON loading, rest/respec cleanup, and fourth-process absence.
- [x] Version/docs/manifest updated; repository validation, full tests, clean
  0.0.116 build, strict package validation, guarded runtime, compatibility,
  and persistence gates pass.
- [ ] Coherent Release B implementation and qualification commits were
  created; the exact mandated push was attempted after every checkpoint but
  remains blocked by the external branch allowlist.

## Release C / 0.0.117 alternate racial traits

- [x] Dedicated read-only Crystalline native audit: actual ray/non-ray delivery
  inventory, public attack-result API, KMG-only/combined process qualification
  (9,816 assertions), zero save access and independently exact restoration.
  This explicitly does not qualify Crystalline Form's actual mechanic.
- [x] Efreeti Magic native slice passes all three Ifrit heritages, both exact
  commands, total-level/Charisma parameters, temporary stats, shared use,
  cancellation, zero/rest/level/removal/reactivation, KMG-only/combined and
  exact cleanup/restoration (current 1,421 tests; 9,796 native assertions).
- [x] Efreeti Magic actual native-selected fresh-process persistence:
  six legal two-trait Ifrit fixtures, twelve accepted native casts, spent
  shared uses, exact active size-buff identity/context/expiration across both
  OFF/ON boundaries, native rest/level/respec/cleanup and fresh absence.
  Four processes pass 242 assertions; all 168 trait and 84 Efreeti observations
  are exact. Retain 47 visual/DollData warnings. Renewed pinned 0.0.114 migration
  adds three processes and 28 assertions with exact restoration.
- [x] Fire/Earth/Air Insight actual native spell commands and per-creature
  duration/count/pool boundaries pass incrementally (not full trait qualification).
- [x] A failing native component identity regression precedes the narrow
  factory-only repair; current 114-fact names, stats and spent-use tests pass.
- [x] Current repaired heritage/feat save cycle passes four processes and
  43 mechanical assertions; retain-base fixtures only, with rendering
  diagnostics explicitly retained.
- [x] Requalify component-name repair mechanics against the exact pinned
  0.0.114 producer: all eight race/sex fixtures, exact stats/facts/spent uses/
  appearance data, three processes, 28 assertions and fresh absence pass.
  All 13 warnings are retained; subjective visual review remains separate.
- [x] Complete current observer-lifetime rerun, including initialization,
  level-up, commands and teardown, with explicit installation/release witnesses.
- [x] Fire/Stone/Storm in the Blood pass focused native damage/healing/rest/
  multiclass/cap/exclusion checks in KMG-only and highest-risk combined profiles;
  82 observations per profile, no native errors, exact fixture cleanup.
- [ ] Qualify actual trait-bearing saves and complete lifecycle transitions.
- [x] Six-trait incremental save cycle passes: 24 fixtures, 18 native-selected
  traits, nine active partially spent blood buffs, four fresh processes,
  230 assertions and 168 exact state observations across OFF load, level-up,
  ordinary rest, re-spending, ON load, native base-trait respec and fresh absence.
  Retain all 46 visual/DollData warnings; this is not the other fifteen traits
  or full death/polymorph/multi-trait lifecycle qualification.
- [x] Correct active-buff loss from ambiguous native provider deactivation;
  real Deactivate/Activate and exact marker-removal regressions pass for all
  three blood traits in KMG-only and combined profiles (9,660 assertions total).
- [x] Deterministic replacement-slot policy covers Energy Resistance,
  Elemental Affinity, and Racial SLA with legal combinations and overlap
  exclusion independent of fact application order.
- [x] Incremental framework gates pass: 1,413 domain tests, clean build and
  strict package, full 4,333-assertion live matrix, KMG-only/combined ON/OFF,
  exact restoration, retain-base persistence/respec and fourth-process absence.
  This does not qualify actual trait mechanics or trait-bearing persistence.
- [x] First eight passive traits pass complete source/build/package and focused
  native runtime gates: 1,415 tests, eight processes, 13,397 assertions,
  zero warnings, KMG-only/combined profiles including OFF, exact restoration.
  This does not complete trait-bearing persistence or the whole release.
- [ ] Reconciler is idempotent, project-owned-only, resource-preserving,
  duplicate-free, respec-reversible, and correct across save/load, module-OFF,
  level-up, death/resurrection, and polymorph/return.
- [ ] All required Ifrit, Oread, Sylph, and Undine traits implement their exact
  mechanically relevant rules; omitted tabletop clauses are documented
  without compensatory bonuses.
- [ ] Pure exhaustive policy matrix asserts exact providers, stats, abilities,
  resources, and feat-prerequisite outcomes for every heritage/trait/order.
- [ ] Guarded runtime covers every implemented trait mechanic.
- [ ] Persistence covers every race, both sexes, all heritages, each trait,
  representative legal combinations, spent/capacity/active state,
  module-OFF/ON, rest, level-up, respec cleanup, and fresh-process absence.
- [ ] Version/docs/manifest updated; repository validation, full tests, clean
  0.0.117 build, strict package validation, guarded runtime, compatibility,
  and persistence gates pass.
- [ ] Coherent framework, content, qualification, and final-documentation
  commits pushed.

## Final handoff

- [ ] Final report records starting/final SHA, branch cleanliness, corrections,
  inventories/adaptations/omissions, manifest identities/counts, migration,
  every test/build/package/runtime/compatibility/persistence result and hash,
  blockers/NOT-RUN limitations, commits, and PR URL if created.
- [ ] No generated release ZIP is committed.
- [ ] Nothing is merged, tagged, or publicly released.
