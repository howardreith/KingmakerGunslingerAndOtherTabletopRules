# Elemental Races journal

## 2026-09-01 - Mission start and authoritative baseline

- The requested adjacent directory
  `C:\Dev\KingmakerGunslingerLab\repo\KingmakerGunslingerAndOtherTabletopRules`
  did not exist. The provided checkout at
  `C:\Dev\KingmakerGunslingerLab\repo\KingmakerGunslinger` has origin
  `git@github.com:howardreith/KingmakerGunslingerAndOtherTabletopRules.git`, so
  it is the requested repository under its older local folder name.
- Starting branch: `master`.
- Starting commit: `06c4d998f160df75ad3be7bfcf3de7e415c631d4`.
- Exact starting status: `## master...origin/master`, with no tracked or
  untracked changes.
- Ran `git fetch origin`. `origin/master` remained exactly
  `06c4d998f160df75ad3be7bfcf3de7e415c631d4`; there were no intervening
  commits to reconcile.
- Created the required branch `codex/elemental-races` without rewriting or
  discarding any history.
- Read `AGENTS.md`, every runtime/build instruction it references,
  `docs/BUILD-AND-RELEASE.md`, the feature-module implementation, blueprint
  registry and complete bootstrap transaction, accepted Gunslinger appearance
  catalog/application, and the outfit scenario/test architecture.
- Current release identity is version `0.0.113`, informational version
  `0.0.113-save-load-hotfix`; current feature settings schema is 9; current
  module count is 10; the baseline runtime boundary count is 22.
- Ran `.\scripts\test-domain.ps1 -Configuration Release` before source work.
  Repository validation passed and the complete dependency-free suite completed
  **1,373/1,373 PASS**.

## Local runtime fingerprints

- `Assembly-CSharp.dll`: 7,262,208 bytes; SHA-256
  `3b6450ffec440e296e586f71c711b195aed144b28d53e1cbb29406d18fef5afb`;
  assembly version `0.0.0.0`; last-write UTC
  `2025-04-06T00:59:19.4022320Z`.
- `UnityModManager.dll`: version `0.32.4.0`; SHA-256
  `1387468bc3af41c50fe51859a3bb7af4922891aa8f13a6187e7a348ceaabfd88`.
- `0Harmony12.dll`: version `1.2.0.1`; SHA-256
  `aa1cd48317254985d8b700cc74953477d1b40c3022ce9aa4c95ed2b8327e1292`.
- Call of the Wild `1.14.4c-2.1`: DLL SHA-256
  `4ebf8e1ed3e66ffed72ea33ea325595629423dacd5bffa23e3c9109144b26915`.
- Races Unleashed `1.0.11`: DLL SHA-256
  `6d18168cb90ffe60931addc8ee11e42b3ef647ef0e6d4b7ce8980d44659f4cb0`.
- Tweak or Treat `1.1.0`: DLL SHA-256
  `a518324e15632aba46d6c467b156a31e9afd282e9827dee3e79ad14673852b92`.
- Favored Class (`ZFavoredClass`) `1.3.1`: DLL SHA-256
  `dcd3adf98d1a04c30d772381e7c56ce4beff35a98bcea165aff206a2f0aac26c`.
- Visual Adjustments was not found by the initial relevant-directory filter;
  an exact installed `Info.json` identity audit remains pending.

## 2026-09-02 - Engine, interoperability, and visual reconnaissance

- Exact installed-mod scan confirmed Visual Adjustments is not installed.
  Its final compatibility observation is therefore `NOT-RUN`, not PASS.
- Inspected the installed Races Unleashed assembly read-only. Its UMM identity
  is `RacesUnleashed`; it publishes during `LoadDictionary` by appending each
  owned race to the current `CharacterRaces` array. Elemental publication must
  consequently preserve the live third-party prefix, identify entries by
  blueprint identity rather than display name, and remain idempotent.
- Inspected Ebons Content Mod read-only at public commit
  `adc05da1b8962b1a9ac6d7f902e22e7756fa5c65`. It proves that Human-compatible
  race cloning, request-owned visual proxies, and a Bull Rush delivery path are
  possible. No source, GUID, asset, BlueprintCore pattern, or dependency was
  copied. Its caster-level affinity behavior is explicitly rejected because
  this mission authorizes DC only.
- Local engine metadata proves `BlueprintRace.RaceId` is a closed enum with no
  elemental members. A development clone using donor `RaceId.Human` therefore
  tests the approved donor-identity strategy without inventing an enum value.
- Guarded `gunslinger-outfit-audit` transaction
  `20260902T0300398535825Z-gunslinger-outfit-audit` passed with 49 classes, 163
  wrappers, 361 raw candidates, 1,206 entities, 3,816 matrix rows, 4,860 links,
  and zero unresolved links. Accepted male/female Magus-derived IDs and colors
  remain unchanged.
- Added one manifest-reserved, request-only diagnostic identity:
  `KMG.ElementalRaces.Diagnostics.ProbeRace` /
  `57005fca40ab4775ae2fea5613214054`. It is never production-published and is
  removed from both blueprint indexes after each request.
- Added `observe-elemental-race-blueprints`, a save-free guarded scenario that
  clones Human, performs native `LevelUpController.SelectRace`, renders male
  and female dolls, verifies accepted Gunslinger outfit links, serializes a
  hidden `BlueprintRace` reference with native JSON settings, and requires
  exact array/index rollback.

## Development-probe strategy changes

1. `20260902T0342307483942Z-observe-elemental-race-blueprints` - **FAIL**.
   Same-frame doll readiness was too strict (`avatarEntities=0`,
   `renderers=0`). Registration, collision scan, hiding, cleanup, and save-free
   invariants passed. Strategy changed to bounded multi-frame settling.
2. `20260902T0352481058858Z-observe-elemental-race-blueprints` - **FAIL**.
   Both roots acquired three renderers, but unattached diagnostic views
   correctly retained zero `CharacterAvatar.EquipmentEntities`. Strategy
   changed to audit the actual root renderers/materials/shaders and keep avatar
   attachment as a separate production-persistence surface.
3. `20260902T0358322341784Z-observe-elemental-race-blueprints` - **FAIL**.
   Rendering and outfit checks passed, but direct `SetRace` did not apply the
   clone-only feature, and a combined custom DollData persistence envelope hit
   a native dictionary-converter ambiguity. Strategy changed to native
   character-generation selection plus isolated blueprint-reference
   serialization; existing accepted doll persistence remains authoritative.
4. `20260902T0409422132157Z-observe-elemental-race-blueprints` - **PASS**.
   Seven exact native races resolved; no same-race collision was present;
   hidden registration resolved in both indexes; male/female views each had
   three renderable renderers, three valid materials, and no null shader;
   native race selection retained the exact race and applied the clone-only
   fact at rank 1; hidden identity serialization round-tripped; cleanup restored
   the exact `CharacterRaces` contents/reference and both library counts; no
   save, input, selector publication, or persistent unit was used. Evidence
   SHA-256: `edfdc13e786fe800851573e4339176b1fdac2116ed43141fd658627ccda40e04`.

## Current decisions and next action

- Preserve the accepted Gunslinger class-clothing IDs and colors unchanged.
- Use four distinct project-owned `BlueprintRace` references and the safest
  donor `RaceId`; the Human donor is mechanically viable but final
  Aasimar/Tiefling outsider semantics remain to be qualified before selection.
- Keep the diagnostic race permanently request-gated. Its successful probe is
  evidence for construction and persistence architecture, not production-race
  completion.
- Complete domain suite after the probe revision:
  `.\scripts\test-domain.ps1 -Configuration Release` - **1,376/1,376 PASS**.
- Phase B checkpoint command:
  `.\scripts\build.ps1 -Configuration Release -Clean -Package` - **PASS**.
  Repository validation, the complete 1,376-test suite, production compile,
  build-output validation, SoundBank validation, deterministic packaging, and
  strict standalone UMM validation all passed. Checkpoint package SHA-256 is
  `160b21230624d3ebc66f2a6c7f3da4e33b3abb0a2605bff250cded143ff6c8c9`;
  DLL SHA-256 is
  `a0887d6061a35b213f7e9ad8df6e65543de66c2fbd39250c13f27cfa3b209320`;
  DLL MVID is `c900ae62-326b-4ec8-a36a-b672122b4266`.
- Next: checkpoint Phase A/B, then implement schema-10 `elemental-races`
  feature-module integration and its 24-state runtime boundary catalog.

## 2026-09-02 - Phase C feature-module integration

- Added one module ID, `elemental-races`, one UMM checkbox, and bit 1024. The
  established ten modules retain default ON; Elemental Races defaults OFF.
- Advanced `FeatureModules.json` to schema 10. Schemas 0 through 9 preserve all
  explicit existing values, migrate an absent Elemental Races key to OFF, and
  preserve an explicit Elemental Races true or false value. Schema 11 is
  rejected as future input.
- Malformed recovery now reports the actual mixed defaults instead of claiming
  every module defaults ON. The malformed source bytes remain untouched and a
  diagnostic quarantine copy is retained.
- Extended active/pending snapshots, equality, hash code, `ToString`, ordered
  serialization, publication planning, UMM presentation, guarded request
  validation/writing, live settings observation, compatibility-profile
  settings transactions, and focused persistence fixtures.
- Ran `.\scripts\build.ps1 -Configuration Release -SkipDomainTests`.
  Repository validation and production compilation passed.
- Ran `.\scripts\test-domain.ps1 -Configuration Release`.
  **1,377/1,377 PASS**, including all 2,048 settings combinations and the new
  every-schema migration case.
- Executed `Get-KmgFeatureModuleCatalog` and
  `Get-KmgFeatureModuleConfigurations` directly. Observed 11 modules, 2,048
  exhaustive configurations, 24 unique boundary profiles, 1,024 exhaustive
  Elemental-ON states, and `elemental-races` as the final deterministic key.
- Ran `.\scripts\build.ps1 -Configuration Release -Clean -Package` on the
  exact Phase C tree. Repository validation, 1,377 tests, production compile,
  output/SoundBank checks, deterministic packaging, and strict UMM validation
  passed. Package SHA-256:
  `a3fa4c26704f59ce3bc8eed61325a4443a04b3d504a6f3d3518d3f26461b5d5a`;
  DLL SHA-256:
  `5875845dd31e1b4c6a5ea4f764df08d4e325df88b58069c933b174661e204eaf`;
  MVID: `1dbe88b3-acc6-45e0-b6c4-f981d9a135f4`.
- Ran focused guarded configuration
  `on-on-on-on-on-on-on-on-on-on-off` through
  `Invoke-FeatureModuleRuntimeMatrix.ps1`. Transaction
  `20260902T0440201720486Z-observe-feature-module-settings` passed every
  existing publication assertion and the new
  `feature-module-elemental-races-restart-snapshot` assertion with observed
  `disabled`. The controller restored the exact original settings bytes;
  original SHA-256:
  `9c95e56da5713b0c9d040a918a270c117a8006b9fb8124b068a6a613d925f11e`.
- The complete 24-launch boundary matrix remains pending until production
  identities and selector publication exist.

## 2026-09-02 - Production base-race identities and rules checkpoint

- Allocated and registered 24 active, project-owned identities: each race has
  a distinct `BlueprintRace`, resistance feature, affinity feature, SLA
  feature, daily resource, and spell-like ability. Manifest arithmetic is now
  1,662 total, 1,660 active, and two reserved; exact GUID inventory is in
  `blueprints/blueprints.json` and the implementation report.
- Selected native Aasimar as the safest visual/RaceId donor. Every production
  race is a separate project object, remains Medium, uses `RaceId.Aasimar`,
  receives the exact native empty `OutsiderType` fact, and retains complete
  Aasimar Human-skeleton fallback options. Consequence: native Aasimar-style
  person-spell exclusion is preserved, while base-game dialogue and checks
  based only on `RaceId` may classify an elemental race as Aasimar.
- Implemented exact racial stat components, native Keen Senses, energy
  resistance 5, native Dwarf Slow and Steady for Oread, and a narrow
  `RuleCalculateAbilityParams` affinity component that adds DC once for the
  matching descriptor and never adds caster level.
- Cloned and sanitized native Burning Hands, Stone Fist, and Feather Step
  effects into once-per-rest `AbilityType.SpellLike` abilities. Optional-mod
  components and spell-list/resource/variant parameter carriers are excluded;
  each ability spends its single project resource through native
  `AbilityResourceLogic`. A feature-owned parameter component sets caster
  level to total character level and uses Charisma for the racial SLA DC.
- Reconstructed Hydraulic Push with a native `ContextActionCombatManeuver`
  Bull Rush using caster level as base attack and the best mental statistic,
  no saving throw, spell resistance enabled, and no unrelated attack roll or
  global patch. Native projectile selection remains a later presentation
  surface.
- Implemented atomic selector publication. It snapshots the exact shared
  array, validates all four races first, preserves every prior reference and
  order, appends only missing project races in Ifrit/Oread/Sylph/Undine order,
  rejects identity conflicts, is idempotent, and restores the exact prior
  array reference on failure. All identities register regardless of module
  state; only selector publication is gated.
- Added four focused production contract tests and updated all manifest/test
  arithmetic. `./scripts/test-domain.ps1 -Configuration Release` completed
  **1,381/1,381 PASS**. Direct Release compilation also passed, and canonical
  repository validation passed with the 1,662/1,660/2 manifest.
- First production guarded run,
  `20260902T0531395848382Z-observe-elemental-race-blueprints`, failed only
  because the pre-production collision assertion classified the four exact
  KMG-owned race GUIDs as foreign candidates. All other assertions passed.
  The check was narrowed to foreign identities; no blueprint or save state was
  changed by the failed request.
- Corrected guarded transaction
  `20260902T0538341591619Z-observe-elemental-race-blueprints` **PASS** (run ID
  `20260902T0538341802095Z-e2ecaa804cd146128423d6af33c7010e`). It resolved all
  24 production objects exactly, observed fixed race order, proved active
  Elemental Races OFF with selector counts `0,0,0,0`, found no foreign
  same-race blueprint, rendered both diagnostic donor dolls, applied facts,
  round-tripped a hidden race reference, and restored both indexes and the
  exact `CharacterRaces` reference. Evidence SHA-256:
  `e338ce2529f76a09fbe9b13f8bd9cf15b658c77b88f9692e4127a44692043159`.
- Runtime-build artifact for that transaction: package SHA-256
  `abab69dfa4d593421c6fb40ff72021da07ec064651bd2072630f0827045efd46`;
  DLL SHA-256
  `b95cb93e35bb4338673c0a532367346264de487cce775ebc024c8ce71df2a3c5`.
  It remains a development 0.0.113 artifact, not the final preview candidate.
- Required clean command
  `.\scripts\build.ps1 -Configuration Release -Clean -Package` passed on the
  recorded production-rules tree: repository validation, 1,381 tests,
  production compilation, output/SoundBank validation, deterministic package,
  and strict UMM validation. Clean package SHA-256:
  `bb05bc9ba75ebb596ba57d2eee36fe71b95d9b3baff4a079ec2cb1c44a8ab4d4`;
  DLL SHA-256:
  `8ee289d26d2754d394a570dc2dd3f0fee6cb3360a8f7163d7fcff2cacfefcfeb`;
  DLL MVID: `6ef0225c-8e4e-4a60-9853-84db65f331b9`.
- Next: checkpoint and push the cohesive production-rules graph, then add
  actual-unit mechanics/SLA scenarios, module-ON publication qualification,
  production visual inventories/proxies, persistence, and compatibility.

## 2026-09-02 - Guarded base-mechanics qualification

- Added `disposable-elemental-race-mechanics`, a request-local, save-free
  guarded scenario that creates actual `ChargenUnit` fixtures for all four
  production races. It applies the production racial facts, performs native
  Fighter/Wizard level-up, damage, ability-resource, rest, ability-parameter,
  and resource-serialization operations, then restores the exact global-unit
  sequence.
- The first transaction,
  `20260902T0605221766137Z-disposable-elemental-race-mechanics`, failed closed
  for two precise reasons. Its Perception assertion incorrectly compared the
  full skill delta even though Wisdom changes legitimately affect that total;
  and Unity 2018 rejected the `SpellDescriptor` enum field on the custom
  affinity component because that enum is backed by `Int64`. The latter was a
  genuine production defect: the matching-descriptor mask would not survive
  Unity component serialization.
- Changed the probe to inspect the exact typed racial modifier sourced from
  native Keen Senses. Changed `ElementalSpellAffinity` to store a validated
  Unity-safe 32-bit descriptor mask and cast it only while handling the native
  rule event. The four authorized descriptors are low-bit values, so no rule
  information is truncated. No global spell patch was introduced.
- Corrected transaction
  `20260902T0615360204789Z-disposable-elemental-race-mechanics` passed all 28
  assertions. A separate evidence file was initially reduced to an opt-in
  JSON shell by Kingmaker's global contract resolver, even though the complete
  observations were present in `runtime-result.json`; this was treated as an
  evidence-quality defect, not ignored.
- Added an explicit default JSON contract resolver for the request-owned
  mechanics evidence and reran through guarded Steam App ID 640820. Transaction
  `20260902T0626272331311Z-disposable-elemental-race-mechanics` **PASS** (run ID
  `20260902T0626272562123Z-f9463005dae440f0a17e4b6268bb1800`) with all 28
  assertions and four complete standalone race records. Evidence SHA-256:
  `902f8b81d87883230f344a67db017829c897ef3e74a55ce534b0674ba2934c65`.
- Actual observations per race proved exact stat components, Medium/Aasimar
  donor identity, base speed (Oread 20; others 30), the exact +2 typed racial
  Perception bonus, resistance 5 by resolving 8 matching damage to 3 and 8
  nonmatching damage to 8, one-use resources, no spellbook/material component,
  cancel-before-commit without spending, one exact spend, unavailability,
  native rest restoration, and spent-state blueprint/amount round-trip.
- A 2 Fighter / 3 Wizard fixture produced total caster level 5 for every SLA,
  spell level 1, and Charisma-based DC. Real
  `RuleCalculateAbilityParams` events changed matching spell DC by exactly one
  and nonmatching DC by zero for Fire, Acid, Electricity, and Cold.
- Runtime DLL SHA-256:
  `32c0a75c7f6c84331c1dea8e9acb2bc190d8a0b0fb1bfd44dc5805af54ebdf86`;
  MVID: `06f4414e-96b1-4679-8ba4-cdd50da37f1d`. The runtime used the dirty
  feature branch at committed base `e2d5946e6fb89a81cac0e266a5d4a3acac2c6bb6`;
  source fingerprints in the evidence identify the exact deployed binary.
- Added focused guarded/native scenario contract coverage and updated the
  authoritative deterministic count. The complete domain command
  `.\scripts\test-domain.ps1 -Configuration Release` is **1,382/1,382 PASS**.
- This checkpoint proves base rules, affinity, resource semantics, rest,
  multiclass total-level scaling, and request-local resource persistence. It
  does not yet prove native command delivery for the three donor spells,
  Stone Fist behavior/expiry, Oread armored or encumbered movement, or
  Hydraulic Push combat resolution; those remain separate required surfaces.
- Required clean command
  `.\scripts\build.ps1 -Configuration Release -Clean -Package` passed on the
  mechanics tree: repository validation, all 1,382 tests, production compile,
  build-output and SoundBank checks, deterministic packaging, and strict UMM
  package validation. Package SHA-256:
  `599e65d26fb92ae8146296c8265043849d7394d77482fe70084eb593793d3c44`;
  DLL SHA-256:
  `cab862592cb85a732c565c4811b44f77f39d68db6a7a57ed7f7a06419c8606b1`;
  DLL MVID: `284f7252-665f-417d-ba47-5786cfe95236`.

## 2026-09-02 - Native donor-SLA qualification

- Added `disposable-elemental-race-slas`, a save-free guarded scenario that
  constructs disposable native units and executes the production Ifrit,
  Oread, and Sylph spell-like abilities through the normal `UnitUseAbility`
  path. The scenario records cancellation, resource commitment and gating,
  ordinary rest, effect delivery, duration/expiry, save/damage behavior, and
  exact global-unit cleanup.
- Transaction
  `20260902T0659506655164Z-disposable-elemental-race-slas` failed closed. Native
  delivery worked, but the early fixture did not yet observe a distinct Reflex
  success path (`24/24` damage), and its command assertion conflated command
  result with zero-resource availability. It touched no save.
- Transaction
  `20260902T0711398412802Z-disposable-elemental-race-slas` failed closed. The
  initial saving-throw control changed target modifiers but did not control the
  actual request-local save event (`4/4` damage); direct reuse of an existing
  command object also reported success after depletion even though it delivered
  no second effect. The fixture was narrowed to a request-local rule probe and
  a fresh `AbilityData` availability check. It touched no save.
- Transaction
  `20260902T0723038687479Z-disposable-elemental-race-slas` failed only the
  damage/save assertion. The trace proved the attempted handler did not alter
  the real Reflex resolution (`24/24`, both saves natural 1 and failed). The
  probe was moved to the rule-event component path actually used by the native
  resolution. It touched no save.
- Corrected transaction
  `20260902T0727505151505Z-disposable-elemental-race-slas` **PASS** (run ID
  `20260902T0727505399772Z-38fc5469392f4871af7e367a5dd10f22`) with all 13
  assertions. Evidence SHA-256:
  `166c5fe7ed1846de64bbdf28ee113b9145b609859cf36621526f6f19322cd322`.
  Runtime-result SHA-256:
  `90d5cf0c1f27af048f0200bb4bab30d93f99862b7aeadc38ca8d7e7a4ae581ee`.
- Ifrit retained native 15-foot/5-foot cone delivery and Fire
  d6-per-rank/half-on-Reflex action structure. At total level 5 the real
  ability parameters were caster level 5 and DC 17. Identical natural roll 10
  produced 20 damage on forced failure and 10 on forced success.
- Oread applied native `StoneFistBuffMedium`
  (`af56c42a31a264648b42d725f362c18d`) for 60 seconds at level 1 and 300 at
  level 5, changed the EmptyHand weapon to native Stone Fist, and expired
  through the native duration path. Sylph applied native Feather Step
  (`c748cceadcab2614b942e56ff257cfbc`) for 600/3,000 seconds at levels 1/5 and
  expired normally.
- All three abilities observed resource `1 -> 1` after cancellation, `1 -> 0`
  on commit, a fresh unavailable second use, and `0 -> 1` after ordinary rest.
  Runtime DLL SHA-256:
  `7d3558f05a999542a15a8bd231e11367cf83f1b075e599dd745036ca15163e81`;
  MVID: `44c0f06d-569d-4226-8487-6fa8ec15c2e5`.

## 2026-09-02 - Hydraulic Push native combat qualification

- Added `disposable-hydraulic-push`, a save-free guarded scenario using
  isolated request-owned hostile native units and the production Hydraulic
  Push ability. It records actual `RuleCombatManeuver`, `RuleAttackRoll`, and
  `RuleSavingThrow` construction; command resource state; opportunity-command
  counts; and native force-movement use.
- The formula matrix covers each unique Intelligence/Wisdom/Charisma maximum,
  all-negative scores, deterministic two-way and three-way ties, and a 2
  Fighter / 3 Wizard multiclass. Ordinary success and failure and combat-
  maneuver immunity are separately observed.
- Transaction
  `20260902T0810509976976Z-disposable-hydraulic-push` failed only resource
  lifecycle. All mechanics passed, but the instant native action resolved
  synchronously before ordinary `UnitUseAbility` debit, leaving the resource
  `1 -> 1` at effect commitment. Runtime DLL SHA-256:
  `d6f9cf7349cf3443deed0d26dcac171ce5df6691af7880b05c3e5b6379ff73ca`;
  MVID: `ffa01811-b7f1-4753-bf8d-3ea2a8895f15`.
- Transaction
  `20260902T0821459403802Z-disposable-hydraulic-push` failed only the second-use
  availability gate. Moving debit entirely into the action produced an exact
  `1 -> 0` commit, but disabling native spend semantics also made a fresh
  zero-resource `AbilityData` appear available. That strategy was rejected.
  Runtime DLL SHA-256:
  `c0ed003e748c1800b4cccc39d505c45046ee020de096a2cbfd1254626ff5dba9`;
  MVID: `e046e656-f2a2-4eac-b759-69e4e9eac867`.
- Final production strategy retains ordinary `AbilityResourceLogic` spend and
  availability semantics and inserts a narrow idempotent commit action
  immediately before the native Bull Rush action. If normal debit happens
  first, the action sees zero and no-ops; if the instant effect happens first,
  it spends once and the later normal debit cannot spend again. Cancellation
  never reaches the action. No global patch was added.
- Corrected transaction
  `20260902T0828281705241Z-disposable-hydraulic-push` **PASS** (run ID
  `20260902T0828281945254Z-55efda7cf4584851b7bc869178dd9a8b`) with all 12
  assertions. Evidence SHA-256:
  `c5afc2e51c9227f0d9c8acb71a39d232ff721c9fc067fdbff256f99d4a3d1cb5`;
  runtime-result SHA-256:
  `b4740a9b637a5343a1750e57a423d956c900011b3ba02b433b110b123b71c883`.
- Command evidence was `1 -> 1` after cancellation, `1 -> 0` after commit,
  unavailable with a fresh ability at zero, and `0 -> 1` after rest. Native
  Bull Rush constructed exactly one maneuver event, no attack or saving-throw
  event, no opportunity command, and used `UnitPartForceMove.Push`. Immunity
  stopped before a roll; ordinary success/failure used native CMB/CMD.
- Runtime DLL SHA-256:
  `8f19b9528df358ae14f2e28fb945b7a9ea7041c72c38dfeeb962f4d806b85b82`;
  MVID: `d62cbb4f-bd4e-4dbc-a177-2034a04efb15`. The guarded launcher reran the
  complete **1,384/1,384 PASS** domain suite and strict package validation.
- Remaining Phase E mechanics surfaces are Oread armor/encumbrance movement,
  native-outsider/person-spell and prerequisite behavior, module-ON selector
  publication, and two-process save-backed persistence. A suitable native
  water presentation remains pending the visual/asset audit.
- Required clean command
  `.\scripts\build.ps1 -Configuration Release -Clean -Package` then passed on
  the exact donor-SLA/Hydraulic checkpoint tree: repository validation, all
  1,384 tests, production compilation, output and SoundBank checks,
  deterministic package creation, and strict standalone UMM validation.
  Package SHA-256:
  `da6efee6dd435b917cd2191bb6aebca6f255eaca18532f1a30dd10d3d342d816`;
  DLL SHA-256:
  `fee51842a8144e1e75324b968f322b2bbf5de7a181ad0c5bd2523443e31ced6b`;
  DLL MVID: `0670cdd2-e916-4d20-942d-ebcf8340cfec`.

## 2026-09-02 - Native identity, Oread movement, and selector publication

- Added `disposable-elemental-race-native-identity`, a guarded save-free
  scenario using native armor slots, encumbrance helpers, speed stats, person
  spell targeting, feature prerequisites, and exact race prerequisites.
- Transaction
  `20260902T0933001067567Z-disposable-elemental-race-native-identity` failed
  closed when a private party encumbrance updater entered a Harmony-patched
  party-context path for a detached unit. The strategy changed to the native
  per-unit `EncumbranceHelper` and `UnitPartEncumbrance.Init` path. No save was
  touched.
- Transaction
  `20260902T0937414169613Z-disposable-elemental-race-native-identity` failed
  closed after proving the Oread/Dwarf armor matrix. Raw heavy encumbrance
  penalty remained `-10` for all races because Slow and Steady suppresses the
  applied modifier, and personal inventory weight was not part of per-unit
  equipment encumbrance. The assertions were corrected and the load fixture
  changed to audited equipped full plate. No save was touched.
- The same run proved installed Aasimar and Tiefling, including their first
  heritage facts, do not grant `OutsiderType`; native Hold Person, Charm Person,
  Enlarge Person, and Reduce Person target them. Production was corrected to
  follow that exact engine precedent instead of adding the empty Outsider fact.
  Stable race identities and donor `RaceId.Aasimar` were unchanged.
- Corrected transaction
  `20260902T0947509229460Z-disposable-elemental-race-native-identity` **PASS**
  (run ID `20260902T0947509479461Z-478bab6aa9d44475ba40e7b36219205a`),
  16/16 assertions. Oread and native Dwarf stayed at 20 feet in real medium and
  heavy armor and under a calculated 50-pound heavy equipped load; Human fell
  from 30 to 20. Generic +10/-5 speed facts applied normally. All seven race
  fixtures matched person-spell/type prerequisites, while exact race
  prerequisites remained distinct. Cleanup restored the exact global-unit
  reference sequence and `saveStateTouched=false`.
- Evidence SHA-256:
  `b3378be2e628d3b7896bcaefb2dcb56498f2026118ba0b28ecdc89b021b68161`;
  runtime-result SHA-256:
  `265c4562ce2b04e12ed803c290f52f724d1981f0aaf994521739c5962ec2df06`;
  runtime DLL SHA-256:
  `442a5d85ab598f0477fb5ad608603fffaf6d099b1d4f2d9d0fa4dcae006d6298`;
  MVID: `d821f0b0-3165-4bf3-b994-72d5b12448e7`.
- Extended the feature-module runtime observer with a live shared-catalog
  reference/GUID inventory. Enabled transaction
  `20260902T0955212013652Z-observe-feature-module-settings` **PASS** (run ID
  `20260902T0955212264067Z-08636fcde0b14641af31e2d869b3577e`): 24 identities,
  one entry each at indexes 9/10/11/12, unique shared catalog, exact contiguous
  Ifrit/Oread/Sylph/Undine order. Runtime-result SHA-256:
  `3150a63f6e5f8eccaf16671900716caf3e40f02fbede0391b2266c378f1af05f`.
- Disabled transaction
  `20260902T0958454418742Z-observe-feature-module-settings` **PASS** (run ID
  `20260902T0958454608633Z-c7e4b8f7efc2414bbb1be23818bddd17`): the same 24
  identities remained registered, selector reference/GUID counts were all
  zero, and the 16-entry shared catalog remained unique. Runtime-result
  SHA-256:
  `5d251de2ab1f00bb8f86dfff18f1edb65efa301cf9fca03c44c933fa30e75df8`.
- Both module transactions restored the exact original settings hash
  `d07a06e1b67d35107ffd84da0e02453bfa0adcfaac59bcb68a4353444c7ec52e`.
  Their common package/DLL SHA-256 values were
  `7a6dde430145b71778877225b643c80a0f574506003750df3233dd3f38bdc286` /
  `63c75f122df292b51f5a0f5ccd67d74435620364b073e3b2d95b4cee5af53675`;
  DLL MVID: `43609df8-b22a-4374-bf3f-eb2b2ce44e3c`.
- Repository validation and the complete domain command passed at
  **1,385/1,385** before both guarded module launches. Save-backed persistence,
  visuals, compatibility profiles, and the remaining 22 boundary launches are
  still pending.
- Required clean command
  `.\scripts\build.ps1 -Configuration Release -Clean -Package` passed on this
  documented checkpoint: repository validation, all 1,385 tests, production
  compile, output and SoundBank checks, deterministic package creation, and
  strict standalone UMM validation. Package SHA-256:
  `abb0639d5942fb4692a6cb455671436a47de6d7ce31f5833976db7d37768dacd`;
  DLL SHA-256:
  `b25b2e76e9c10d900fa391c432f7838a7023cbeed9381de5338e0afe90756ce0`;
  DLL MVID: `57a10073-0765-4107-986e-de9ef987ca0b`. This remains a version
  0.0.113 engineering checkpoint, not the final preview candidate.

## 2026-09-02 - Guarded native visual-donor and palette inventory

- Extended the request-gated `observe-elemental-race-blueprints` probe to
  inspect Human, Aasimar, Tiefling, Elf, Dwarf, Half-Elf, Half-Orc, and Gnome.
  It records exact resource IDs/names, race and sex link context, equipment
  layers, body/outfit and hidden-body-part metadata, color profiles and ramp
  names, visual-preset wrappers and skeletons, DLC flags, and texture metadata.
  The diagnostic race remains unselectable, save-free, and removed exactly.
- Transaction
  `20260902T1026029043019Z-observe-elemental-race-blueprints` **FAIL** (run ID
  `20260902T1026029223078Z-0538f23fa0f84073b2f09d8efa6ffac3`) only because
  the first fixture required eyebrows from every donor. Half-Orc intentionally
  declares none; all 358/358 visual links had resolved and every preset was
  complete. Runtime-result SHA-256:
  `e144ea66316ae8d28a913c282cd0a06eff452a5ead537266afdcd871370aa2eb`;
  probe evidence SHA-256:
  `fe7a167cb11634e0d54f69de0c1a24ab57e3f10f4f44f041e7ac3c3b5d1f3ee4`.
  The test was corrected to make brows an audited optional category.
- Corrected transaction
  `20260902T1030175263514Z-observe-elemental-race-blueprints` **PASS** (run ID
  `20260902T1030175473872Z-8119193d15fe46f58b619ebe4cd71a41`): eight donors,
  358 declared and resolved links, 303 unique resources, 55 repeated
  references, zero unresolved links, complete presets, and required head/hair
  breadth for both sexes. Runtime-result SHA-256:
  `9da3723c1550e743c9cdc2da51c8d51ec0b3f4599d7e9239139fef4ba7739ee3`;
  probe evidence SHA-256:
  `dce1a2f1077a67c2cf0a13041256dd0b3eb0e30f2b349bd4c999ae042db1a8a2`.
- Added live texture metadata and reran transaction
  `20260902T1042298660656Z-observe-elemental-race-blueprints` **PASS** (run ID
  `20260902T1042298861108Z-ffb1e76c92564fda8db19251778e4ae5`). Every native
  ramp observed was 256x1 RGB24, bilinear, clamped, and non-readable; this is a
  consistent donor material contract for project-owned visual proxies. The
  probe identifies suitable native Tiefling warm/blue skin, Elf pale blue/
  green/gray skin and hair, Aasimar metallic hair, and Gnome blue/aquamarine/
  violet hair palettes without extracting or redistributing textures.
- Human, Aasimar, and Tiefling presets use the Human-compatible skeleton
  family. Production geometry will remain in that proven family initially;
  Elf, Dwarf, Half-Elf, Half-Orc, and Gnome geometry is not assumed compatible
  merely because its links resolve. Their audited native ramp textures remain
  usable as palette inputs subject to proxy rendering.
- Final donor-audit evidence SHA-256:
  `e1bca7ba7357d0aef9e964fd02e6c5f254ee6368331c5ace0a9776202f5c5733`;
  runtime-result SHA-256:
  `6227ad7e44a917d98adcfbaaa8fd84fab31efe3a2a28f4e5af8fcfd9aa04a0c7`.
  Runtime package/DLL SHA-256 values were
  `dc4164159f59954a32a794d6cdf33a29603343bed3fad530000aa6935919a663` /
  `efcc1d274d233991dfe357a304ec506f9441533bc1bce81b6d0b7e5c8d00ad0b`;
  DLL MVID: `8ed2482c-7e21-4afb-851d-fe6a14480ca1`. The guarded launcher reran the
  complete **1,385/1,385 PASS** domain suite and strict package validation.
- This checkpoint proves donor availability and palette contracts, not the
  final per-race appearance. Stable proxy registration, curated combinations,
  candidate rendering, class/equipment/motion compatibility, save persistence,
  and human aesthetic acceptance remain pending.
- Required clean command
  `.\scripts\build.ps1 -Configuration Release -Clean -Package` passed on the
  exact documented donor-audit tree: repository validation, all 1,385 tests,
  production compilation, output and SoundBank checks, deterministic package
  creation, and strict standalone UMM validation. Package SHA-256:
  `f15d67d2334e197f64bc0eb4f7edb580876e1a737ee6f75ae689416692c56323`;
  DLL SHA-256:
  `d4861724104b211cf800ba13c5617a8da60c170548a007807919c2d49a439e8c`;
  DLL MVID: `bcee4c97-da02-45e2-9c11-abaa8cf497f0`. This remains a version
  0.0.113 engineering checkpoint, not the final preview candidate.

## 2026-09-02 - Production vanilla-asset visuals and renderer matrix

- Added a production visual layer under `ElementalRaces/Visuals`: four stable
  body-wrapper blueprints, twelve stable standard/heavy/slender preset clones,
  and 28 stable raw `EquipmentEntity` proxies. The authoritative inventory is
  now 68 elemental identities (24 mechanics plus 44 visuals), and the project
  manifest is 1,706 total / 1,704 active / two reserved identities.
- Construction resolves every native donor before mutation, clones rather than
  mutates native entities, registers the raw proxies by project GUID, preserves
  the Aasimar preset's Human doll `RaceId`, and rolls every proxy back if any
  mandatory visual cannot validate. Each race has an Aasimar-compatible body
  fallback, three complete native preset/skeleton fallbacks, two heads per sex,
  four nonempty hair styles plus the native empty choice, two brows per sex,
  supported male beards, and seven native 256x1 skin ramps. Ifrit alone offers
  an empty horn choice plus two restrained Tiefling-derived horn proxies.
- Geometry remains in the runtime-proven Human/Aasimar/Tiefling skeleton
  family. Palette ramps are referenced from audited native resources; no
  texture extraction, copied asset, generated mesh, original body mesh,
  persistent VFX, or new runtime dependency was introduced. Kingmaker exposes
  no race-level eye-color selector in `CustomizationOptions`; eye appearance
  therefore remains part of the native head/material contract and is retained
  as a documented human-review limitation.
- Added guarded scenario `elemental-race-visual-audit`. Its deterministic 56
  cases cover all four races, both sexes, all three body presets, every head,
  hair, eyebrow, beard, and horn choice, all seven skin indexes, and at least
  four hair-color indexes. Every case uses the production race and accepted
  Gunslinger class, produces exact `DollData`, builds a native unit view, and
  requires a baked `Renderer_Character_*` renderer with non-null materials and
  shaders. It destroys each view before proceeding and proves the shared
  `CharacterRaces` reference/content and both blueprint indexes remain exact.
- Transaction
  `20260902T1237574226385Z-elemental-race-visual-audit` **FAIL** (run ID
  `20260902T1237574466387Z-65f300c9e62f4067bab10cc3cfd2997b`) because the
  first fixture required the race-preset body proxy inside
  `DollData.EquipmentEntityIds`. Evidence SHA-256:
  `11dd6ed2de444e1a2af4649cea31e685d9621d74a9c0495b042c62957e5fe98b`;
  runtime-result SHA-256:
  `6b28f10856dbb97bf64037dfd940121386aefbe63f9d79ecc20b67f529e78d85`.
- Instrumented transaction
  `20260902T1244048097940Z-elemental-race-visual-audit` **FAIL** (run ID
  `20260902T1244048308239Z-0d78b08d64ec444097e2580a967d6946`) while proving
  every selectable option was exact and resolvable; it confirmed the body is
  resolved separately through `BlueprintRaceVisualPreset.Skin`. The assertion
  was narrowed to the actual native envelope. Evidence/runtime-result hashes:
  `10238e8f272fda27a1c20b383085597f8638745833dda441220a9201bc9a8724` /
  `1e785d9ade93f6b9a1030afe4c592fc4e1457cf4d4082312f106755de3a4e807`.
- Transaction
  `20260902T1249193950321Z-elemental-race-visual-audit` **FAIL** (run ID
  `20260902T1249194230369Z-5e40330bfd4846d081618c626cec7737`) after the first
  complete view showed three valid renderers and materials. Native doll baking
  had consumed and cleared `CharacterAvatar.EquipmentEntities`; requiring the
  post-bake list was rejected in favor of the exact preset/body assertion plus
  the baked character-renderer/material contract. Evidence/runtime-result
  hashes:
  `80b73a86d5c868eea89c78c266c298c831611e7dc45e3f03c7baa8a7892d4fa1` /
  `75476bd8f4e97d3bebe9a417f3221298ed1a1ca5317d958c50cec78b5e50d4d4`.
- Transaction
  `20260902T1254143906397Z-elemental-race-visual-audit` **FAIL** (run ID
  `20260902T1254144156369Z-2a5f567fc76c456596fbde88bded3c84`) only in final
  aggregation after all 56 cases passed. Optional beard/horn absence can be a
  null link or the native empty-asset sentinel; the aggregator normalized those
  equivalent none choices without weakening mandatory option comparisons.
  Evidence/runtime-result hashes:
  `f80a6771461eb300134376384db3aafedb533a9a720a21097bc2d2d964810bfb` /
  `398a1001443b61340ae68da9278e4bc5e5f4132b02bb6a4ec0b4e31c5fc1e599`.
- Corrected transaction
  `20260902T1301142529832Z-elemental-race-visual-audit` **PASS** (run ID
  `20260902T1301142730203Z-d7724f16516944cb960e95a5cec358cd`): 56/56 render
  cases, eight/eight exact coverage groups, 28/28 proxy registrations, 16/16
  visual blueprints, zero data failures, zero missing materials/shaders, exact
  graph cleanup, and `saveStateTouched=false`. Evidence SHA-256:
  `592e0d9f7c37bf850e9a79808102197d713a3e29b4655a765a66b1cdb5e8c699`;
  runtime-result SHA-256:
  `53879496629b6a8dac4cd5f41a4c99e41654c8b487736ee7525a1c6dde83a569`.
  Runtime package/DLL SHA-256 values were
  `757c0a708bdcb57249629c690c2783172977376072bd61a6f7a1c6984efe6da0` /
  `7bff93029f7d35dad8bda0d9d4bd967e888fd5e8ccb9bcdf849db261d0cbb0b6`;
  DLL MVID: `07958c59-ab7d-48db-83dc-6fb5d0c6f06c`.
- The guarded launcher rebuilt the candidate, passed repository validation and
  all **1,385/1,385** domain tests, validated the standalone package, launched
  only through Steam App ID 640820, and restored the pretest installation.
  Class/equipment/motion, two-process save persistence, compatibility profiles,
  and subjective aesthetic acceptance remain pending.
- Required clean command
  `powershell -NoLogo -NoProfile -ExecutionPolicy Bypass -File
  .\scripts\build.ps1 -Configuration Release -Clean -Package` then passed on
  the documented production-visual tree: repository validation, all 1,385
  tests, production compilation, build-output and SoundBank validation,
  deterministic package creation, and strict standalone UMM validation. Clean
  package SHA-256:
  `8167bc0b7294e7d6936b59e2f58ec9b6fb6954882ea6b25379129f2ac481f3f2`;
  DLL SHA-256:
  `89ba7f9cd2a3f5393c6034a6b1b99a416dae7117c0546ad6cd7f8679eab21a5e`;
  DLL MVID: `ef41cc19-3e5c-41a2-a9f1-f0eb51d29887`. This remains an
  engineering version 0.0.113 checkpoint, not the final preview artifact.

## 2026-09-02 - Elemental Gunslinger class/equipment qualification

- Generalized the accepted
  GunslingerOutfitProductionCompatibilityScenario transaction without
  changing its Human mode. The new guarded
  elemental-race-class-equipment mode resolves the four unconditional
  production race identities, creates one male and one female Gunslinger doll
  per race, and reuses the exact existing sixteen-state production matrix.
  It records race GUID and donor RaceId, verifies the accepted sex-specific
  Magus-derived class links, and uses distinct progress/index filenames.
- The matrix covers default and alternate class colors; held pistol, musket,
  and blunderbuss; stored musket; light and heavy armor equip/remove rebuilds;
  tricorn equip/remove with hair restoration; cloak equip/remove; backpack
  visibility/removal; and final rebuild. Every state requires exact body-slot
  contents, native character rebuild completion, non-null render materials and
  shaders, and reversible fixture cleanup. The original Human scenario retains
  its two Human fixtures, labels, assertion IDs, and sixteen cases.
- Added the scenario to the guarded working-save allowlist, exact preflight
  metadata, bounded 1,800-second collector window, runner dispatch, and
  exception routing. It names only KMG_AUTOMATION_WORKING, never calls a save
  API, and uses request-local actors/items/cameras/textures.
- Domain command
  .\scripts\test-domain.ps1 -Configuration Release was run four times
  during focused test construction. The first three completed all 1,386 tests
  with exactly one failure each in the new source-shape test: first a line-wrap
  assumption around BlueprintBootstrap.ElementalRaces, then a line-wrap
  assumption around the raceGuid indexer, then an assumption that the
  dynamically prefixed equipment assertion existed as one literal. These were
  test-only contract defects; no production behavior failed. The checks were
  narrowed to the semantic ordered-race, per-race aggregation, and dynamic
  assertion-suffix contracts. The final run passed **1,386/1,386**.
- Guarded command
  .\scripts\Invoke-KingmakerRuntimeTest.ps1 -Scenario
  elemental-race-class-equipment -ExpectedVersion 0.0.113 -SaveName
  KMG_AUTOMATION_WORKING -TimeoutSeconds 1800 -ExitAfterCompletion:$true
  -Confirm:$false -AllowDirtyGit completed transaction
  20260902T1346079130473Z-elemental-race-class-equipment **PASS** (run ID
  20260902T1346079340812Z-a1279a136b5c438dbaf91caddec0387b).
- All 11 assertions passed: exact guarded scenario and working-save boundary,
  exact Kingmaker 2.1.7b assembly identity, eight/eight production race/sex
  link rows, eight/eight native Gunslinger fixtures, 128/128 exact equipment
  states, 256 supporting PNGs and 640 labelled structural views, eight/eight
  exact fixture restorations, unchanged production class blueprint, unchanged
  party/global-unit snapshots, no save call, and loaded mod version 0.0.113.
  The runtime lasted 310,084 ms and removed all scenario hooks.
- Runtime result SHA-256:
  9f766d5224c6eb7a2aafccfdbb6fd38123e4e152bf94ba21d9f13d6bcb033e71;
  matrix index SHA-256:
  6af7737ebf9f1f46511b620e83f8ea2fee254c6e8dd438e924ffad551b99aa99;
  runtime package/DLL SHA-256 values:
  b7ceb091a8750a6eb2b5a5124d3bc6cca8b1985e8e615b45d31cbc6385e49749 /
  3c7a6f83bdeb2b378d87a474d7f1fa662a5927da098e134fe50986f4f4d91b1a;
  DLL MVID: c2a5c72c-05b2-4b4a-bf57-b1856c915211.
- Low foreground-density warnings were retained as framing diagnostics.
  Screenshots are supporting evidence only and do not establish clipping or
  aesthetic acceptance. This scenario proves static Gunslinger outfit,
  firearm, armor, headgear/hair, cloak, backpack, color, and rebuild
  compatibility. The broader class-clothing matrix, medium armor, robes,
  accessory slots, motion, save-backed persistence, compatibility profiles,
  and human visual review remain pending.
- Required clean command
  .\scripts\build.ps1 -Configuration Release -Clean -Package then passed:
  repository validation, all 1,386 tests, production compilation, build-output
  and SoundBank validation, deterministic package creation, and strict
  standalone UMM validation. Clean package SHA-256:
  1c8065ae6a0d0218930de556ef30ab5648922a18d82c3e2fabefc77fcc89bb45;
  DLL SHA-256:
  d1a45067f0636eb4df19818c1780ca3deca49cebae6f73ab5edc0acae6b9a1bd;
  DLL MVID: 93c4538b-07ad-43cd-93c1-b9fc977a9be3. This is a version
  0.0.113 engineering checkpoint, not the final preview artifact.

## 2026-09-02 - Elemental native-motion qualification

- Added guarded `elemental-race-motion` as an alternate mode of the accepted
  production motion transaction. It selects the four production elemental
  race identities and both sexes while retaining the original Human fixture
  path. Because all four races deliberately use `RaceId.Aasimar`, fixture and
  frame aggregation now records and validates each exact race blueprint GUID.
- The per-fixture matrix is unchanged: unarmed idle; musket slow walk, normal
  run, and body-relative turn; native pistol and musket attacks; production
  musket reload through update 240; and native shortsword melee. Eight fixtures
  therefore produce exactly 216 records, 216 PNGs, 216 structured sidecars,
  864 labelled views, 16 movement outcomes, 8 turns, 24 attacks, and 8 reloads.
  Every actor/target/faction/scene is request-local; inventory, avatar, player,
  combat, and published-class snapshots restore exactly; no save API is called.
- Added the scenario to the request catalog, working-save predicate, autonomous
  metadata, preflight contract, runner dispatch/area-load routing, and a bounded
  7,200-second collector. The public request timeout remains within its existing
  1,800-second validation limit. An initial command using 7,200 was rejected by
  PowerShell parameter binding before launch or transaction creation; the
  corrected command used 1,800 while the scenario-specific collector retained
  its bounded 7,200-second deadline.
- The first domain run stopped at repository validation because the newly added
  test made the pinned total 1,387 stale. The count was deliberately advanced
  to 1,388. The second run exposed one invalid quote escape in the focused test
  only; it was replaced with the existing `(char)34` pattern. The complete
  command `.\scripts\test-domain.ps1 -Configuration Release` then passed
  **1,388/1,388**, and the production Release build passed.
- Corrected guarded command
  `.\scripts\Invoke-KingmakerRuntimeTest.ps1 -Scenario elemental-race-motion
  -ExpectedVersion 0.0.113 -SaveName KMG_AUTOMATION_WORKING -TimeoutSeconds
  1800 -ExitAfterCompletion:$true -Confirm:$false -AllowDirtyGit` completed
  transaction `20260902T1520151111405Z-elemental-race-motion` **PASS** (run ID
  `20260902T1520151280510Z-1512e7f8e1fe41b4bc268018108d6941`). All 14
  assertions passed: 8/8 fixtures, 216/216 records, 864 views, exact action
  counts, 8/8 avatar restorations, 8/8 combat-boundary reconciliations,
  blueprint immutability, and no save call. Duration: 410,936 ms.
- Runtime-result/index/runtime-evidence SHA-256 values:
  `9fa5484e3b5c2eb96778305f785387c271783d7958945ab77bb4e7b49d744eab` /
  `7d2616e5a6c1647625ec3605720713dbfddf538542ad1627230256d558bb315d` /
  `04c0078e909f7aad3a0c4caaf20d62185f725841e4e7cb02ebc42351b2b6c8e3`.
- Required regression command
  `.\scripts\Invoke-KingmakerRuntimeTest.ps1 -Scenario
  gunslinger-outfit-production-motion -ExpectedVersion 0.0.113 -SaveName
  KMG_AUTOMATION_WORKING -TimeoutSeconds 1800 -ExitAfterCompletion:$true
  -Confirm:$false -AllowDirtyGit` completed transaction
  `20260902T1531223520715Z-gunslinger-outfit-production-motion` **PASS** (run
  ID `20260902T1531223770733Z-ed026c1eaa5240c5893cb70313f6c8e3`). Its
  original 14 assertions, 2/2 Human fixtures, 54/54 records, 216 views, action
  outcomes, restorations, combat boundaries, immutability, and no-save contract
  all remained exact. Duration: 192,181 ms.
- Human runtime-result/index/runtime-evidence SHA-256 values:
  `da1da6742ab2852541928475f1f5375783df660f426f5976f289f7e7f1a4f886` /
  `5f04ac4cf1da0159cd98f5ddf6b692e02714349ddaffa9edccc62abfb052bf07` /
  `6731baa03fd3439542ba835ecefbbf0a3c1904ba9f9c969afaf6497487fdc3c9`.
  Both runs used package/DLL SHA-256
  `c5e6431d97a116d737584fe85a0d05bbf5e713b3be8f915dc9c4bc72f184815d` /
  `f7f69f3f0c96425497639bc7845e971205e28b652115c8773c11d57b0d5b4ec7`;
  DLL MVID: `39b4cd0b-71e9-470f-a032-33cec7ea7f14`. The loaded commit
  remains `d1b931d22e86d337385b1a3bdc33c2ac3466f9cc`, the immediately preceding
  committed checkpoint; the dirty-run source/package/DLL identities capture
  the exact tested build.

## 2026-09-02 - Ten-class elemental clothing qualification

- Added the guarded, save-free `elemental-race-class-clothing` scenario. It
  reuses the production visual session and covers exactly four races by two
  sexes by ten classes: Gunslinger, Fighter, Rogue, Ranger, Alchemist, Magus,
  Wizard, Cleric, Monk, and Kineticist. The scenario resolves direct and
  wrapper-linked class entities, then maps the authoritative native
  `LoadClothes` result back to stable resource IDs by exact reference rather
  than reconstructing or sorting the shared order.
- Each of the 80 planned cases validates exact race and sex identity,
  customization data, exact expected class-clothing references, native
  asynchronous character attachment and bake completion, and non-null
  materials and shaders. Cleanup destroys every request-local view and restores
  the exact `CharacterRaces` array plus both blueprint indexes. It never calls
  a save API and never mutates selector state.
- Added guarded launcher/preflight/catalog/runner wiring with a bounded
  600-second collection window and a focused `elemental-races.class-clothing`
  domain contract. The complete domain command
  `.\scripts\test-domain.ps1 -Configuration Release` passed
  **1,387/1,387**.
- Transaction
  `20260902T1424092517156Z-elemental-race-class-clothing` **FAIL** (run ID
  `20260902T1424092707494Z-7271d466b0694c398605788449b709c3`) because the first
  inventory reconstructed direct-plus-wrapper order and disagreed with the
  authoritative native `LoadClothes` order for male Ifrit Rogue. The fixture
  was changed to observe and map the exact native result. Evidence/runtime
  result SHA-256:
  `4269e3b08f3d8eeef982c4045a0fd4afd4ca4acd53404bd927bb388dc8199517` /
  `b52b2ae2b0cd84bd33e558496477a7c7b5faf49bbca22fe99ee06aee975c1c0e`.
- Transaction
  `20260902T1433563378448Z-elemental-race-class-clothing` **FAIL** (run ID
  `20260902T1433563598503Z-25e61a17aa16479a8a79d30dd0ddd3e2`) because the
  fixture incorrectly required native class clothing in
  `DollData.EquipmentEntityIds`. Native class clothes are supplied outside that
  customization envelope, so the assertion was separated without weakening
  the later exact native clothing check. Evidence/runtime-result SHA-256:
  `f3ad5f4aee9d3a000642c8d391beb6c08fe7020ab1d4d1d852a78096c5765cab` /
  `98348fe3f1b3de1ad2be21df08581fd0738427478a541e8293ce9d7302b1141a`.
- Transaction
  `20260902T1442416931078Z-elemental-race-class-clothing` **FAIL** (run ID
  `20260902T1442417131444Z-f5835f471899411284398363802ce11e`) because the
  fixture accessed the avatar synchronously before the request-local view had
  attached its native `Character`. Evidence/runtime-result SHA-256:
  `93d6467d6d88743f0b17b45666e8eecf1f87f26365104accd4c6c92e14f760f1` /
  `75059db1e8625cf07c33f23ae993ae27d1b44b5966305ba0096d250f2db06df6`.
- Transaction
  `20260902T1447240186139Z-elemental-race-class-clothing` **FAIL** (run ID
  `20260902T1447240426141Z-5dfa73748aa14213aeb7f55e30a849f1`) because the
  settle predicate recognized the attached `Character` while one remaining
  path still read the null `UnitEntityView.CharacterAvatar`. The path was
  unified on the attached component. Evidence/runtime-result SHA-256:
  `93d6467d6d88743f0b17b45666e8eecf1f87f26365104accd4c6c92e14f760f1` /
  `ff2a0bb22e840b9d0a8ce9aa4dc3de6c05f1e7a41264e886c88b99dee0991145`.
- Corrected guarded command
  `.\scripts\Invoke-KingmakerRuntimeTest.ps1 -Scenario
  elemental-race-class-clothing -ExpectedVersion 0.0.113 -SaveName
  KMG_AUTOMATION_WORKING -TimeoutSeconds 600 -ExitAfterCompletion:$true
  -Confirm:$false -AllowDirtyGit` completed transaction
  `20260902T1451545064731Z-elemental-race-class-clothing` **PASS** (run ID
  `20260902T1451545269918Z-bd479eeecec14d81b202649ee013234b`). All five
  assertions passed: exact 16-blueprint/28-resource/four-race/ten-class
  inventory, exact 80-case plan, 80 rendered cases in ten exact groups, exact
  graph cleanup, and `saveStateTouched=false` /
  `selectorStateTouched=false`; warnings were zero.
- Runtime-result/evidence/runtime-evidence SHA-256 values:
  `d4432e55fc47f503bab4167c79c7920ec01fedf1934e6be94435e360df2c84c5` /
  `846b5d05ed6f573006856d9595310c1b0af7cc6ce7bf360f56a745f25e89fcc4` /
  `da636404476081bc32256fe511cedb9aa1ebe58a32a95ab6347c65e28cc34053`.
  Runtime package/DLL SHA-256 values:
  `49415761e4abe0d68fbb82fd7e23a12aabc01850529b07c5fd5f4faf6751d498` /
  `c450e8019d92cd70a365ef8611965e217525483cb8192bb2c5e5e10f8daab3db`;
  DLL MVID: `232218ab-4a89-4447-ac65-465500f73cf6`. The loaded commit field
  remains the committed checkpoint `fea328d59912c316868adbe36618a5104130fb17`
  because this was an authorized dirty-tree run; source, package, DLL, and MVID
  fields identify the tested build exactly.
- Required clean command
  `.\scripts\build.ps1 -Configuration Release -Clean -Package` then passed:
  repository validation, all 1,387 tests, production compilation, build-output
  and SoundBank validation, deterministic package creation, and strict
  standalone UMM validation. Clean package SHA-256:
  `a60b04d199d3b20c3e63342ae896275a8e98b8117fecf106c891bfd5c5521382`;
  DLL SHA-256:
  `18be7cb6e5d5923471bf5d702eda8aa206f4b4a8ac2a72cc709d1c589f72fc56`;
  DLL MVID: `5cd6ca86-8a25-42d9-a900-95ef2257ef65`. This is a version
  0.0.113 engineering checkpoint, not the final preview artifact.

## 2026-09-02 - Elemental motion clean package checkpoint

- After both elemental and original-Human production-motion scenarios passed,
  ran the required clean command
  `.\scripts\build.ps1 -Configuration Release -Clean -Package` on the exact
  motion source tree. Repository validation, all **1,388/1,388** tests,
  production compilation, build-output and SoundBank validation, deterministic
  packaging, and strict standalone UMM validation passed.
- Clean package:
  `artifacts/packages/KingmakerGunslinger-0.0.113-save-load-hotfix.zip`,
  22,940,247 bytes, SHA-256
  `497625066222aa5c08fe2323e7134e8c257a8dddf58b193687c20a7d0f05c279`.
  Clean DLL: 5,294,080 bytes, SHA-256
  `7cf1181feec82b606189fd22a3bffbd15d9db8ae44ee76c519e2c6ef325145a8`,
  MVID `399e6d2a-4593-4e90-b59d-44a49ca9058a`. Packaged `Info.json` remains
  `0.0.113`; this is not the final preview candidate.

## 2026-09-02 - Three-process save-backed persistence qualification

- Added three guarded working-save scenarios:
  `elemental-race-persistence-prepare`,
  `elemental-race-module-disabled-persistence`, and
  `elemental-race-persistence-verify-absent`. A transactional orchestration
  script pins the package and deployment manifest, writes an exact schema-10
  module-ON configuration for prepare, writes module-OFF for reload, restores
  the original settings bytes in `finally`, and requires the absence check as
  an independent third fresh launch after restoration.
- The fixture set is exactly Ifrit, Oread, Sylph, and Undine by male/female.
  Phase 1 uses native `LevelUpController.SelectRace` and `ApplyLevelup` to
  make eight level-1 Gunslingers, applies deterministic production
  customization, renders the accepted class outfit, spends each SLA from one
  use to zero through native `AbilityResourceLogic`, promotes the fixtures to
  the party, and saves only `KMG_AUTOMATION_WORKING`. Phase 2 reloads with
  Elemental Races OFF and proves hidden selectors plus registered identities,
  exact race/facts/stats/ability/resource-zero/DollData/render/outfit state,
  native rest restoration to one, level-up to two, and total-level caster
  level two before exact fixture cleanup and save. Phase 3 proves zero fixture
  residue and the original working-save routine count.
- Stable request-owned fixture IDs are
  `a9be3b86-9d80-472a-93e6-71fcfb3a827a`,
  `2fc7d5a4-5dab-4bb9-bee1-da1fdfa2a337`,
  `f4933068-5824-46fa-a330-25b78764503e`,
  `27a98188-4106-419d-8897-64ccd6f63305`,
  `d532ec12-a328-4afb-8cbf-7f3ddf41f072`,
  `08e1cd1d-4512-4c52-a9fa-6dd8d815499a`,
  `043d4fc2-c26c-4e72-9d11-219d0ff74b43`, and
  `91472289-c1d7-4558-b7ed-a5e8c06345fb`.
- The complete domain command
  `.scripts	est-domain.ps1 -Configuration Release` passed
  **1,389/1,389** after adding the focused guarded/exact-orchestration
  contract. Runtime preflight passed **193/193** immediately before the final
  launch sequence.

### Fail-closed iteration evidence

- `20260902T1703470958910Z-elemental-race-persistence-prepare` (run
  `20260902T1703471269277Z-e852816c413845ce85b4c769868919fe`) failed because
  assigning the target race before native character generation made
  `SelectRace` a no-op and omitted the SLA. The clone now retains its donor
  race until native race selection commits.
- `20260902T1713296584828Z-elemental-race-persistence-prepare` (run
  `20260902T1713296825293Z-9e2c5a35a55047f0a8633ca8720edb73`) failed because
  reflected zero-argument `AbilityData.Spend` did not commit the resource.
  The fixture now uses the ability's exact native resource-cost component.
- `20260902T1722475679831Z-elemental-race-persistence-prepare` (run
  `20260902T1722475919837Z-74e3d634281342e19d5a39bc71d10837`) failed because
  native `AbilityResourceLogic.Spend` correctly exempts a blueprint marked
  `IsCheater`. Production-realistic fixtures are now explicitly non-cheater.
- `20260902T1733547857625Z-elemental-race-persistence-prepare` (run
  `20260902T1733548137604Z-4a3847d3aeee4835b3ced8fe710ed053`) failed because
  independently ordered options selected a Sylph female head and incompatible
  eyebrow that native DollData validation normalized. Eyebrows are now paired
  by the production head-array index.
- The corrected prepare
  `20260902T1745349991483Z-elemental-race-persistence-prepare` passed, but
  module-disabled reload
  `20260902T1748093001497Z-elemental-race-module-disabled-persistence` (run
  `20260902T1748093021519Z-50540d31eb004138911ee2184b8645df`) failed when a
  female fixture inherited the male dynamic donor blueprint gender after
  reload and therefore lost female class clothes. Native
  `UnitDescriptor.CustomGender` is now set and asserted for every fixture.
  Fail-safe cleanup completed, and independent recovery transaction
  `20260902T1751460976119Z-elemental-race-persistence-verify-absent` (run
  `20260902T1751461216779Z-b35a2be3879c43e9bb1c25ae871db43b`) passed 6/6,
  proving the working save was clean before the final sequence.

### Final persistence evidence

- Prepare transaction
  `20260902T1759420290804Z-elemental-race-persistence-prepare`, run
  `20260902T1759420540785Z-9764141caa2645208e08ba64e1870d23`,
  **PASS 10/10** in 138,457 ms. It created all eight exact fixtures, captured
  16 supporting images/40 structural views, and persisted each SLA at zero.
  Runtime-result/index SHA-256:
  `ac8d16c4d8d2fa7bdc382e5c3836ce3190371c7008b26b9042ccb6ec6a54921a` /
  `ac1ae1a5748a4fc97e21da297824ca9269dd7a262b04fc486f5b784f6e25a17d`.
- Module-disabled transaction
  `20260902T1802161725612Z-elemental-race-module-disabled-persistence`, run
  `20260902T1802161745656Z-11fbec67bb7e4939b4de818d6695e538`,
  **PASS 10/10** in 140,975 ms. All eight identities loaded while selectors
  remained hidden; race, facts, statistics, spent resource, production
  DollData, sex, render, and Gunslinger outfit were exact. Native rest restored
  each resource to one; promotion produced level/caster level two. Cleanup
  removed every fixture and saved the working save. Runtime-result/index
  SHA-256:
  `f16a93ab55be1bbbf87622bdeae4b927b65a352fe637add66d050e1a3bf697d4` /
  `f285ce0e8d762748cbfa63055300e3333114c089457a9f501b11b8d81ddd2ee8`.
- Post-restoration transaction
  `20260902T1805097536949Z-elemental-race-persistence-verify-absent`, run
  `20260902T1805097767152Z-7e0adc8b36e343e8b2f9891c5a7713fa`,
  **PASS 6/6** in 121,652 ms. It observed zero project fixtures, exact baseline
  absence, exact party/global-unit structure, and the original one working-save
  routine. Runtime-result/index SHA-256:
  `27d69f12cba8e77482fd012122fe1b5d47c115763d886fc6ffb9c36e8aacc12f` /
  `d285ec257df1910a437c81f8eb635f94a86833cadb6496def2fb5d82d5bbcd02`.
- All three final launches used guarded Steam App ID 640820 and deployment
  `20260902T1758236782720Z` (manifest SHA-256
  `662b53bb99d24477922b88a9cb267bb2455c77d3574e486587ac4ea931ac1fbf`).
  Runtime package/DLL SHA-256:
  `b4a1762c3dfd2d91c025a8f2ed9ce6cac8dc49a443ce5f404c601e60c731a843` /
  `17f85ab3142fcf4aea91ec96870d00a09411a8121a929deac9bed97eaef4cf47`;
  DLL MVID `b9eb12f4-f96b-4374-b046-bd5c78e88127`; source-state SHA-256
  `baf7fefcd9076982a0fe3a41b7ebab549f4df733bb0df0c3f6367f49b095b3eb`.
  Exact original/restored FeatureModules bytes both hash to
  `d07a06e1b67d35107ffd84da0e02453bfa0adcfaac59bcb68a4353444c7ec52e`.
  The protected baseline and the existing human-persistence evidence were
  never selected, written, or altered.
- Required clean command
  `powershell.exe -NoLogo -NoProfile -ExecutionPolicy Bypass -File
  .\scripts\build.ps1 -Configuration Release -Clean -Package` then passed:
  repository validation, **1,389/1,389** dependency-free tests, production
  compilation, build-output and SoundBank validation, deterministic package
  creation, and strict standalone UMM validation.
- Clean engineering checkpoint package
  `artifacts/packages/KingmakerGunslinger-0.0.113-save-load-hotfix.zip` is
  22,957,333 bytes, SHA-256
  `4e2faaa7a0671ef4463750434e07b54057766a97e4478b3141262da47de65ecd`.
  Clean DLL is 5,349,376 bytes, SHA-256
  `ac846bde870dfe7c2bd2355aa0e8bf15fe69cd77b42bcf5a9f6e3eaeea1f2770`,
  MVID `5b9fc86a-0d95-4a75-b2cd-8a9237b4e516`. Packaged `Info.json`
  remains version `0.0.113`; this persistence checkpoint is not the final
  preview candidate.

## 2026-09-02 - Compatibility profiles and eleven-module boundary

- Ran the repository compatibility-profile orchestrator with an immutable
  installed 0.0.113 engineering artifact. Every transaction restored the
  exact prior Mods tree and module settings. Profile transactions were
  `compat-20260902T185342Z-5973350fa3fa` and
  `compat-20260902T190239Z-2c56b50edfe4` (Races Unleashed development
  passes), `compat-20260902T191139Z-a79767a2b667` (KMG-only observer
  assumption failed closed), `compat-20260902T192425Z-9c8fcf9cf74a`
  (KMG only PASS), `compat-20260902T192829Z-9426b83997b1` (Call of the Wild
  PASS), `compat-20260902T193430Z-4acfea65674b` (Races Unleashed PASS),
  `compat-20260902T193842Z-5d94f722d0a9` (Call of the Wild plus Races
  Unleashed PASS), `compat-20260902T194505Z-11348d409953` (highest-risk
  stack; Elemental Races passed but the inherited Bodyguard fixture failed),
  and corrected `compat-20260902T200855Z-82edd58b91b7` (highest-risk focused
  Bodyguard and archetype reconciliation PASS). Every listed transaction
  reports `Restored=true`.
- Guarded nested runs were
  `20260902T1854195574141Z-mod-load-smoke` /
  `20260902T1855309761198Z-observe-optional-mod-compatibility` /
  `20260902T1856401598132Z-elemental-races-races-unleashed-compatibility`,
  `20260902T1903016073083Z-elemental-races-races-unleashed-compatibility`,
  `20260902T1911578815262Z-mod-load-smoke` /
  `20260902T1913083417906Z-observe-optional-mod-compatibility` /
  `20260902T1914171401193Z-elemental-races-races-unleashed-compatibility`
  (FAIL: the first absence-mode fixture still required Races Unleashed),
  `20260902T1924432889909Z-mod-load-smoke` /
  `20260902T1925525300940Z-observe-optional-mod-compatibility` /
  `20260902T1927013264534Z-elemental-races-races-unleashed-compatibility`,
  `20260902T1928524725963Z-mod-load-smoke` /
  `20260902T1930423853288Z-observe-optional-mod-compatibility` /
  `20260902T1932270230606Z-elemental-races-races-unleashed-compatibility`,
  `20260902T1934530377201Z-mod-load-smoke` /
  `20260902T1936027275089Z-observe-optional-mod-compatibility` /
  `20260902T1937125003792Z-elemental-races-races-unleashed-compatibility`,
  and `20260902T1939106634916Z-mod-load-smoke` /
  `20260902T1940579559576Z-observe-optional-mod-compatibility` /
  `20260902T1942443858679Z-elemental-races-races-unleashed-compatibility`.
  Highest-risk nested runs
  `20260902T1945384439064Z-mod-load-smoke`,
  `20260902T1947291785242Z-observe-optional-mod-compatibility`,
  `20260902T1949161204578Z-elemental-races-races-unleashed-compatibility`,
  and `20260902T1951042340323Z-observe-aid-another-compatibility-contracts`
  passed. `20260902T1952522577770Z-disposable-helpful-bodyguard` failed
  closed, drove the narrow Bodyguard fixture repair in commit `133abc47`,
  and was superseded by PASS runs
  `20260902T2009276441078Z-disposable-helpful-bodyguard` and
  `20260902T2011198037139Z-disposable-archetype-reconciliation`.
- Visual Adjustments was not installed in the authorized local mod inventory;
  its profile is **NOT-RUN**, not PASS.
- Ran `.\scripts\Invoke-FeatureModuleRuntimeMatrix.ps1` with the immutable
  installed artifact and generated exactly 24 `2 + 2N` configurations.
  Every `observe-feature-module-settings` launch passed and restored the
  exact original settings bytes. Evidence directories and run IDs are recorded
  in the following section.

### Boundary transaction ledger

All entries are `observe-feature-module-settings` PASS:

```text
20260902T2014022268316Z / 20260902T2014022478726Z-648c5a0f976c48448f20b91a016409c8
20260902T2015527171354Z / 20260902T2015527191369Z-e6f9364e3d7c4e2fbda33a339dd99557
20260902T2017441260505Z / 20260902T2017441280725Z-e81bf9b45d1c4982a939fa8282c373ad
20260902T2019356615673Z / 20260902T2019356635285Z-d6144b907de147f2b2aa563858b2dd8d
20260902T2021261946788Z / 20260902T2021261967036Z-189c0d97913d4e899276630b6cd8c7e0
20260902T2023197732418Z / 20260902T2023197762072Z-5d30a2201c9f483cb58e177e7f3add5d
20260902T2025134271593Z / 20260902T2025134291633Z-de304636f17943e8a89726db2c909efd
20260902T2027044821598Z / 20260902T2027044841048Z-d8f296d3904c4193a1c4dbd762ecf0be
20260902T2028585751265Z / 20260902T2028585781063Z-4eeca35f284f4e6888398b5edb9497ed
20260902T2030528022234Z / 20260902T2030528042060Z-f183a23a66a84c7896147e36363a065c
20260902T2032438408466Z / 20260902T2032438438503Z-51c87583c27247fbab6cb8a32c7a8078
20260902T2034361734679Z / 20260902T2034361754263Z-89848a0f3a964d0dbb336bcae8fb900d
20260902T2036278173078Z / 20260902T2036278193055Z-05cd5912a4744c0ca7669210634e83bd
20260902T2038239597329Z / 20260902T2038239627504Z-628ade32173c448c9b572a048c92a0de
20260902T2040148357921Z / 20260902T2040148377916Z-a8e0a2b1499c4755adfe9b59c7467e59
20260902T2042070973121Z / 20260902T2042071003309Z-d50008fdf04747b4afec671b16d87a5f
20260902T2043596368782Z / 20260902T2043596388656Z-72af620fad77447ba1d45215ae4a4c5f
20260902T2045504373082Z / 20260902T2045504393085Z-cf61e34e6efe4371a7f85852dd5a73ed
20260902T2047415474773Z / 20260902T2047415484734Z-7877bdfbe6eb45a4b829695504f3030b
20260902T2049316093982Z / 20260902T2049316113998Z-269616c490524796922a43b4710d74b9
20260902T2051259790176Z / 20260902T2051259810192Z-2adceb5cb5b3455498da850d95913956
20260902T2053158397066Z / 20260902T2053158427277Z-8e705ae2576c42c2b037b8a818516a76
20260902T2055067867187Z / 20260902T2055067897237Z-903ae7339a814a4189dfd90ffa181cbb
20260902T2056561928027Z / 20260902T2056561948051Z-63d97a60506a48f2860a59d2eac423a7
```

## 2026-09-02/03 - Expanded equipment and native state transitions

- Generalized the existing Gunslinger outfit compatibility transaction from
  16 to 28 reversible states per fixture: default/alternate colors; held
  pistol, musket, and blunderbuss; stored musket; light, medium, and heavy
  armor with removal/rebuild; robe; tricorn/hair restore; cloak; boots; gloves;
  bracers; belt; and backpack. The Human/Gunslinger catalog and accepted Magus
  links were not changed.
- Transaction `20260902T2112330884762Z-elemental-race-class-equipment`
  (run `20260902T2112331124769Z-de315475ab184cafa00ac2e15b2bf1e0`)
  failed closed because one initialization assertion still required sixteen
  unique states. After replacing that obsolete literal with the catalog
  count, transaction
  `20260902T2120481218981Z-elemental-race-class-equipment` (run
  `20260902T2120481448986Z-18bb6ad3f477460791d5ae063af2dd91`)
  passed: eight fixtures, 224 records, 448 PNGs, 1,120 structural views, eight
  exact restorations, exact production blueprint immutability, and no save API.
- Extended `elemental-race-motion` with eight exact records per fixture for
  one native race-owned SLA command, prone/stand, lethal
  damage/death/resurrection, and Beast Shape II polymorph/return. The following
  attempts failed closed and retained structured JSON:
  - `20260902T2205348818690Z` /
    `20260902T2205349008729Z-784c4c89a569418c9ffa442d63e55a8f`:
    lethal damage did not settle inside the first 600-update fixture contract.
  - `20260902T2219121894201Z` /
    `20260902T2219122084179Z-8b9aca9fbadb48d69fb20a742eb602a0`:
    1 to -10 HP was not the engine's actual death threshold.
  - `20260902T2227361476475Z` /
    `20260902T2227361656677Z-4c02e659853e485d8446adeeb101bc97`:
    a later detached fixture's racial resource had not reached native
    availability before command construction.
  - `20260902T2247443557556Z` /
    `20260902T2247443787624Z-fce0e61d109444b490ad26ec83253f6e`:
    the resource record existed at one, but the detached native availability
    boundary still reported false.
  - `20260902T2305499397631Z` /
    `20260902T2305499606476Z-2aa494cd00c04b609bcaa96f4110473d`:
    instant Undine delivery completed without remaining in running state.
  - `20260902T2331073581683Z` /
    `20260902T2331073761915Z-96bc43ea77184ff49bf502e6dfb18dc3`:
    the fixture queried `IsUnitEnoughClose` before completing the target
    command envelope.
  - `20260902T2340002984237Z` /
    `20260902T2340003184267Z-e8029219c7644373a7b54cf2ee67fc50`
    and `20260902T2354525126645Z` /
    `20260902T2354525342215Z-86e3394d6dab499a8f1341ba08be2d5c`:
    female Undine reached all preceding 56 transition records but still
    reported `spellCastingForbidden=True` until native scene readiness.
- Strategy changes were narrow and fixture-only: use the engine death
  threshold, initialize detached resources once, accept synchronous command
  completion, build/settle a legal hostile target before proximity reads, and
  defer target assignment until native spellcasting readiness instead of
  forcing unit state.
- Corrected transaction
  `20260903T0012214812700Z-elemental-race-motion` (run
  `20260903T0012215042665Z-b5c60eb2cdb44439a2d540969ec684d8`)
  passed on commit `651c05ae2c676fee6ad4c085195c818a9a01613a`:
  216 motion plus 64 transition records, 1,120 views, eight SLA, prone,
  death/resurrection, and polymorph/return outcomes, exact cleanup, and no save
  call. Runtime package/DLL SHA-256:
  `485da937d4b7eb4188aad01f1dda0553ca667b5d7e938533cff0acaee81d4f80` /
  `c03d35848e89a9938a88fb66b7fa2cdc51e311391ca212a83db0203fe2a5d34c`;
  DLL MVID `70f1ac87-fb53-4f13-9d7d-32eacd7df860`.
- The unchanged Human mode then passed transaction
  `20260903T0022352926189Z-gunslinger-outfit-production-motion` (run
  `20260903T0022353126156Z-16f4669473d848d1af5acb1aec4fa829`):
  two fixtures, 54 records, 216 views, exact cleanup, and the same artifact
  hashes/MVID.

## 2026-09-02 - Final preview identity selection

- Ran `git fetch origin`. `origin/master` remains exactly
  `06c4d998f160df75ad3be7bfcf3de7e415c631d4`, tag `v0.0.113` remains the
  newest published version, and there is no competing release. Selected the
  owner-authorized next identity: assembly/UMM version `0.0.114`,
  informational/package identity
  `0.0.114-elemental-races-preview`, and candidate filename
  `KingmakerGunslinger-0.0.114-elemental-races-preview.zip`.
- Updated active build, assembly, package, local-runtime, guarded-preflight,
  compatibility-profile, persistence-orchestrator, documentation, and test
  identity surfaces. Historical 0.0.113 evidence and release notes remain
  intact.
- Added `tools/validate_elemental_races114.py`. It inherits every accepted
  0.0.113 paper-mode/save-load invariant and additionally validates 1,706
  manifest entries (1,704 active, two reserved), 69 Elemental Races entries
  (68 active plus one request-gated diagnostic), four active race blueprints,
  schema 10, 11 modules, 24 runtime boundary states, atomic publication,
  mechanics, visuals, scenarios, compatibility packages, documentation, and
  1,390 deterministic cases.
- The first `python .\tools\validate_repository.py` run failed because the
  UI banner had placed the new marker before the historical
  `URBAN-BARBARIAN` prefix required by inherited gates. Reordering those two
  labels preserved both. Subsequent runs exposed two historical suffix maps;
  each received an explicit 0.0.114 `elemental-races-preview` mapping without
  changing older mappings.
- The inherited Eastern/Favored gate initially conflated an unknown new
  version with its immutable 0.0.93 checkpoint status. The records now keep
  that historical PASS unchanged while
  `compatibilityRuntimeQualificationPending=true` records the current
  candidate honestly and is derived from all five required profile
  dispositions. No 0.0.114 runtime PASS was borrowed.
- Release notes now record the unchanged accepted firearm SoundBank checksum,
  no-`CraftMagicItems.dll` boundary, and inherited 1,288/1,325 checkpoint
  counts required by version-aware validation.
- Final command `python .\tools\validate_repository.py` passed the complete
  inherited Sprint 29-through-113 chain and the new 0.0.114 Elemental Races
  gate. The exact-version domain suite, clean package, guarded respec,
  24-state matrix, and five profiles remain pending at this point in history.

## 2026-09-02 - Native Respec source qualification

- Generalized the existing save-backed elemental persistence prepare phase
  without changing its guarded scenario identity or save boundary. Each of
  the eight race/sex fixtures now starts as an exact native-created elemental
  Gunslinger, settles, and then passes through a distinct level-zero
  replacement descriptor using
  `LevelUpState.CharBuildMode.Respec`, `SelectRace`, `SelectClass`, and
  native `Commit`. The temporary source is retired before promotion.
- Promotion now fails closed unless every replacement retains its exact race,
  racial facts, one-use SLA, deterministic `DollData`, stable marker
  identity, and accepted Gunslinger class presentation. The existing
  persistence phases will then spend the resource, save, reload with selector
  publication OFF, verify facts/appearance/resource state, rest, level up,
  clean up, save, and prove absence.
- Updated the focused `elemental-races.persistence` contract in
  `GunslingerOutfitRenderTests.cs`; the deterministic case count remains
  1,390.
- `.\scripts\test-domain.ps1 -Configuration Release`: **PASS,
  1,390/1,390**.
- `.\scripts\build.ps1 -Configuration Release -SkipDomainTests`:
  production compile and build-output validation **PASS**.
- `.\scripts\build.ps1 -Configuration Release -Clean -Package`:
  repository validation, all 1,390 domain tests, production compilation,
  build-output validation, SoundBank validation, deterministic preview ZIP,
  and strict standalone UMM validation **PASS**.
- To preserve enough disk for exact-version guarded runs, removed only 782
  optional PNGs totaling 1,552,082,960 bytes from the recorded PASS
  directories `20260903T0012214812700Z-elemental-race-motion`,
  `20260903T0022352926189Z-gunslinger-outfit-production-motion`, and
  `20260902T2120481218981Z-elemental-race-class-equipment`. All structured
  JSON evidence remains. The deleted images are recoverable only by rerunning
  those guarded scenarios.
- No 0.0.114 runtime PASS is claimed at this checkpoint. A committed/pushed
  artifact is required before the guarded Steam persistence transaction.
