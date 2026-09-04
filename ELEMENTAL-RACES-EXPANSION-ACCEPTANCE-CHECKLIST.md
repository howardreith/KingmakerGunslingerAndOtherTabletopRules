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
- [ ] Feat identities register unconditionally; module-gated publication to
  exact universal/combat selectors is additive, deterministic, idempotent,
  conflict refusing, order preserving, and reversible.
- [ ] Exact race/level/feat/SLA prerequisites and all-heritage eligibility pass.
- [ ] Elemental Strike implements exact swift-action, one-round, race-energy,
  level-breakpoint, weapon-only, once-per-attack behavior.
- [ ] Scorching Weapons, Inner Flame, Blazing Aura, and Firesight pass their
  exact action, item snapshot, damage/replacement, turn-start, resistance,
  cleanup, persistence, and concealment contracts.
- [ ] Airy Step, Wings of Air, Cloud Gazer, and Inner Breath pass exact save,
  native-flight/armor, curated concealment, and respiration contracts.
- [ ] Hydraulic Maneuver and Triton Portal pass exact variant/formula/shared-
  resource/action/summon/module-boundary behavior; unsupported Dirty Trick is
  recorded honestly if no native path exists.
- [ ] Intentionally deferred feats remain unimplemented and documented.
- [ ] Persistence covers feat facts, active buffs, temporary item
  enchantments, resources, and module-OFF loading.
- [ ] Version/docs/manifest updated; repository validation, full tests, clean
  0.0.116 build, strict package validation, guarded runtime, compatibility,
  and persistence gates pass.
- [ ] Coherent Release B implementation and qualification commits pushed.

## Release C / 0.0.117 alternate racial traits

- [ ] Deterministic replacement-slot policy covers Energy Resistance,
  Elemental Affinity, and Racial SLA with legal combinations and overlap
  exclusion independent of fact application order.
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
