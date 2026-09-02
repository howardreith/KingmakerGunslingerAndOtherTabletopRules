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
