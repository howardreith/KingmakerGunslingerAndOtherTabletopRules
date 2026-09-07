# Changelog

## 0.0.117-elemental-traits

- Integrates the separately published 0.0.115 Brown-Fur direct-cast fix from
  authoritative master without changing elemental identities or mechanics.
  All 1,431 tests, clean build/package, 30 compiled API checks and six guarded
  processes pass; native-command and strict working-payload controls remain
  distinct from NOT-RUN direct-consumer Instant gameplay. See the
  [integration checkpoint](docs/ELEMENTAL-RACES-0.0.117-MASTER-INTEGRATION-CHECKPOINT.md).

- Qualifies native RTWP breath action commitment and the append-only ten-trait
  save matrix: all 1,428 tests, clean build/package and nine guarded processes
  pass 11,325 assertions, including renewed pinned 0.0.114 migration.
  Exact saved Sickened context/duration, spent uses, OFF/ON/rest/level/respec
  and fresh absence pass. No new identities or production rules. Retained
  warnings and turn-based/full-release limits are in the
  [action/save checkpoint](docs/ELEMENTAL-RACES-0.0.117-BREATH-PERSISTENCE-CHECKPOINT.md).
- Adds Acid Breath and Ooze Breath with four fixed ability/resource identities,
  native five-foot acid cones, exact current-CON parameters and Reflex halving.
  Ooze applies three-round non-poison Sickened only on a failed save. All 1,427
  tests, clean build/package and 10,936 assertions in two native profiles pass.
  The later action/save checkpoint above supplies new RTWP and save proof. See
  the [focused breath checkpoint](docs/ELEMENTAL-RACES-0.0.117-UNDINE-BREATHS-CHECKPOINT.md).
- Avoids attempted feat-transient restoration inside native disabled level-up
  previews, preserving exact original buffs, item effects and absolute expiry.
  All 1,424 tests, clean build/package and nine guarded processes pass 10,594
  assertions, including renewed eight-trait saves and pinned 0.0.114 migration.
  No identity/schema changes. This repairs the inherited service in 117, not
  the retained 116 ZIP. See the [preview checkpoint](docs/ELEMENTAL-RACES-0.0.117-FEAT-PREVIEW-CHECKPOINT.md).
- Strengthens Crystalline native-command coverage for delayed multi-ray casts,
  duplicate impact notifications, non-damage effects and native hand states.
  All 1,424 tests, clean build/package and 10,242 guarded assertions pass.
  No production rule or identity changes; the remaining-trait donor audit is
  read-only. See the [controls checkpoint](docs/ELEMENTAL-RACES-0.0.117-CRYSTALLINE-CONTROLS-CHECKPOINT.md).
- Preserves exact owned providers during native whole-unit save suspension,
  preventing Crystalline consent from being silently reset before serialization.
  The eight-trait save matrix and renewed 0.0.114 migration pass alongside
  1,424 tests and native KMG-only/combined regressions. Exact failures, warnings
  and remaining gates are retained in the
  [persistence checkpoint](docs/ELEMENTAL-RACES-0.0.117-CRYSTALLINE-PERSISTENCE-CHECKPOINT.md).
- Adds Crystalline Form's exact-ray racial AC and opt-in daily native impact
  deflection with three stable resource/buff/mode identities. Full 1,423 tests,
  clean build/package and two guarded KMG-only/combined processes pass 10,168
  assertions. Complete ray/catalog and save-backed gates remain separate; see
  the [core checkpoint](docs/ELEMENTAL-RACES-0.0.117-CRYSTALLINE-CORE-CHECKPOINT.md).
- Adds incrementally qualified Efreeti Magic: Enlarge Person
  and Reduce Person wrappers share one persisted daily resource and retain
  native person targeting. All 1,421 tests and clean build/package pass.
  Nine guarded processes pass 10,066 assertions: native KMG-only/combined,
  actual multi-trait OFF/ON saves, shared spent uses and active size buffs,
  rest/level/respec/cleanup, and renewed pinned 0.0.114 migration. Visual
  diagnostics and full Release C lifecycle/compatibility gates remain explicit.
- Adds 62 fixed manifest identities for ten explicit replacement-slot
  selections, ten retain-base markers, 21 visible alternate-trait markers,
  and 21 separate hidden mechanic providers.
- Adds an exhaustive pure replacement policy for Energy Resistance,
  Elemental Affinity, and Racial Spell-Like Ability slots across every parent
  race, heritage, legal combination, invalid overlap, and fact-order
  transition.
- Extends the owned heritage reconciler to compute one desired provider state,
  preserve spent racial-SLA amounts, and remove only exact project-owned
  resistance, affinity, SLA, ability, and alternate-trait provider facts.
- Adds each relevant slot selector to the existing parent race without adding
  top-level races, a module toggle, a schema revision, or dynamic identities.
- Implements Wildfire Heart, Brazen Flame, Forge-Hardened, Granite Skin,
  Like the Wind, Secretive, Thunderous Resilience, and Whispering Wind with
  native stat/resistance components and narrow save/damage rules.
- Makes Scorching Weapons' existing fire nonstacking policy independent of
  later-acquired Brazen Flame by removing only its exact owned damage packet.
- Gives Acadamae's self-fatigue save the existing native fatigue reason, with
  unchanged DC, command, resource, and fatigue-resolution behavior.
- Qualifies this first passive slice with 1,415 tests, clean build/package,
  eight guarded processes, 13,397 assertions, zero runtime warnings, and
  exact KMG-only/combined profile restoration, including module-OFF.
- Implements Fire, Earth and Air Insight through exact native spell-family
  and creature-subtype checks; focused native command and rule tests pass.
- Adds three fixed blood-trait buff identities and a shared native reactive-
  healing implementation that counts only HP actually received against the
  persisted daily cap. Focused native checks pass in KMG-only and highest-risk
  combined profiles; trait-bearing persistence remains pending.
- The current component-name repair passes pinned 0.0.114 migration mechanics
  in three fresh processes (28 assertions), preserving spent uses and appearance
  data; subjective visual review is separate.
- Release C remains in progress: three mechanics remain to implement;
  complete Crystalline, all-trait persistence/lifecycle and final release-wide
  gates are pending. Earlier incremental results retain their historical scope.

## 0.0.116-elemental-feats

- Adds 25 fixed manifest identities for eleven elemental racial feats and
  their nine abilities, four buffs, and one exact-item weapon enchantment.
- Registers every Release B identity unconditionally so module-OFF loads can
  resolve feat-bearing saves, while publication remains controlled by the
  existing `elemental-races` setting with no schema revision.
- Publishes all eleven feats to the universal selector and only Elemental
  Strike, Scorching Weapons, Inner Flame, and Blazing Aura to the Fighter
  combat-feat selector through deterministic, exact-GUID-aware, reversible
  transactions.
- Binds prerequisites to exact project race and provider blueprints and records
  exact level/feat requirements, action economy, honest Kingmaker limitations,
  and approved native adaptations.
- Implements and independently qualifies every required Release B feat
  mechanic through native command, attack, damage, save, concealment, buff,
  equipment, combat-maneuver, summon, targeting, and rest paths. Hydraulic
  Maneuver exposes native Bull Rush, Disarm, Trip, and Dirty Trick (blind),
  while Triton Portal summons 1d3 exact native Small Water Elementals using
  the shared racial Hydraulic Push use.
- Persists command-created one-round Elemental Strike and exact-item Scorching
  Weapons state through direct stable item references plus absolute game-time
  expiry, including module-OFF hydration, without retargeting swapped gear or
  restoring expired/ineligible effects. A 24-fixture three-process transaction
  and fourth-process absence pass.
- Qualifies the final candidate with 1,408/1,408 deterministic cases, a clean
  exact-reference Release build, strict 135-entry package validation, a
  five-process 73/73 integrated mechanics regression, and 31/31 guarded
  compatibility processes spanning all six required installed profiles in
  both module states. All 12 profile transactions restored the original mod
  tree and settings exactly. Visual Adjustments was absent and is NOT-RUN.

## 0.0.115-elemental-heritages

- Adds one obligatory three-choice heritage selection to each existing Ifrit,
  Oread, Sylph, and Undine race without adding top-level races or changing any
  0.0.114 race, General affinity, General SLA, or General resource GUID.
- Implements General plus Lavasoul/Sunsoul, Gemsoul/Ironsoul,
  Smokesoul/Stormsoul, and Mistsoul/Rimesoul with exact net racial-stat
  overlays, one active affinity provider, and one active racial SLA provider.
- Uses audited native Firebelly, Flare Burst, Color Spray, Expeditious Retreat,
  Shocking Grasp, and Blur behavior. Unerring Weapon and Chill Touch use narrow
  project-owned native-rule implementations with stable child and item
  enchantment identities.
- Treats a missing heritage marker on an existing 0.0.114 character as General.
  Provider reconciliation is exact-identity scoped, idempotent, and preserves
  remembered daily-resource amounts instead of restoring a spent use.
- Retains the hardened spell-only elemental-affinity predicate and exact
  Charisma-based racial SLA DC/total-character-level caster-level contract.
- Qualifies Release A through the complete 1,407-case suite, clean build and
  strict package validation, dedicated blueprint/mechanics/SLA runs,
  24-fixture ON/OFF/ON persistence, exact 0.0.114 migration, native visual
  state transitions, and twelve restoration-verified transactions spanning
  all six required installed compatibility profiles. Visual Adjustments was
  absent and remains NOT-RUN.

## 0.0.115-share-transmutation-instant

- Adds a versioned, provider-owned Brown-Fur direct-cast transaction API for
  callers that already own spell-source spending. The API validates and binds
  the exact `AbilityData`, recipient, Share/Powerful selection, spellbook, and
  reservoir reservation before the effect-producing `RuleCastSpell` runs.
- Reuses the native Brown-Fur targeting, modifier, commit, Supremacy, and
  cleanup machinery without creating or queuing a synthetic
  `UnitUseAbility`. Arcane Reservoir debit remains provider-owned.
- Retains each direct transaction until its native execution process is
  terminal, isolates rejection and cleanup to that transaction, and adds
  delayed-process, four sequential cast, and AbilityData-reuse regressions.
- Exposes `BrownFurDirectCastApi.ContractVersion = 1` as an optional boundary;
  this assembly does not depend on Kingmaker Buff Planner.

## 0.0.114-elemental-races

- Adds one restart-bound, default-ON Elemental Races module containing stable
  Ifrit, Oread, Sylph, and Undine race blueprints. All identities register
  unconditionally for save resolution; selector publication is atomic,
  additive, idempotent, and preserves native and third-party ordering.
- Implements the approved base rules: exact ability modifiers and speeds,
  energy resistance 5, Keen Senses, matching-descriptor +1 spell DC affinity,
  and one total-character-level racial spell-like ability per rest. Oread uses
  native Slow and Steady and Stone Fist, Sylph uses Feather Step, and Undine's
  Hydraulic Push resolves a native Bull Rush with the best mental modifier.
- Adds deterministic vanilla-asset appearances using 16 visual blueprints and
  28 stable equipment proxies. No original body mesh, extracted Owlcat asset,
  copied mod asset, persistent elemental VFX, or new runtime dependency is
  included.
- Migrates feature-module settings to schema 10 and eleven modules. Earlier
  explicit values survive, Elemental Races migrates ON when absent, malformed
  recovery enables all modules, and the runtime boundary matrix contains
  exactly 24 states.
- Adds guarded rules, publication, visual, class/equipment, motion,
  compatibility, and three-process save-persistence scenarios while preserving
  the accepted Gunslinger outfit and the 0.0.113 paper-mode save-load repair.
- Qualifies all 24 eleven-module boundary states and five exact compatibility
  profiles through 18 fresh-process guarded scenarios with zero warnings and
  exact restoration of the prior mod tree and settings. Visual Adjustments was
  unavailable and remains explicitly NOT-RUN.
- Records owner acceptance and explicit authorization to finalize, merge, and
  publish the exact qualified 0.0.114 release.

## 0.0.113-save-load-hotfix

- Removes paper-cartridge marker reconciliation from mode reads and all
  save-deserialization hooks. The native activatable ability is authoritative;
  stale markers are ignored mechanically rather than deleted during hydration.
- Keeps the `set_IsOn` hook cache-only, preserving toggle-off loose-ammunition
  reload behavior without changing persistent unit facts.
- Restores concise local descriptions for generic Nodachi and every named
  eastern weapon, preventing blank local copy from inheriting donor text
  containing `Brace` during blueprint validation.

## 0.0.112-ammunition-cmi-copy-notifications

- Uses one positive, 10%-of-retail ammunition policy across native Gunsmithing
  and KMG's scoped Craft Magic Items route: 22 gp for the loose 20/20 batch and
  24 gp for 20 Paper Cartridges. Native transactions verify and deduct gold
  once, roll back owned state on failure, and now display the actual paper
  price in every project-authored card and runtime assertion.
- Makes Use Paper Cartridges authoritative from the current native activatable
  state, reconciles its hidden marker safely, invalidates stale presentation,
  and keeps queued reload action, ammunition source, and inventory commitment
  coherent.
- Replaces synthetic Craft Magic Items lifecycle cycling with targeted,
  idempotent KMG-owned finalization. KMG never toggles, reloads, or changes
  Craft Magic Items' UMM lifecycle state.
- Audits project-authored item descriptions, records the review in
  `docs/ITEM-DESCRIPTION-AUDIT.md`, and maps project-owned reload/crafting
  diagnostics to concise player-facing messages while retaining structured logs.

## 0.0.111-gunslinger-class-outfit-kitbash

- Replaces the Gunslinger's inherited generic Fighter clothing with a coherent
  native Magus base-and-accessory presentation for male and female characters,
  using guarded defaults 2/22 without adding custom game assets or dependencies.
- Preserves supported race/gender fallback, user color ramps, armor, headgear,
  cloak, backpack, firearm/melee animation, rebuild, save/load, and native
  respec behavior. The installed-game qualification covers the complete
  nine-race/two-gender grid, static equipment overrides, motion/fire/reload/
  melee, persisted reconstruction, and cleanup of its disposable save fixture.

## 0.0.110-protection-from-alignment-control-immunity

- Protection from Evil, Good, Law, and Chaos now prevent new registered
  domination and comparable mental-control effects from matching-alignment
  sources, following Wrath of the Righteous behavior. Existing control effects
  are not removed or suppressed when protection is applied. This is not a
  complete tabletop implementation.
- Updates the ten generic, individual, and communal protection spell cards and
  all five patched protection-buff tooltips so players can see the scoped
  immunity and the existing-control limitation. Description publication is
  feature-gated, transactional, and idempotent; disabling the independent UMM
  module retains vanilla descriptions and mechanics after restart.
- Adds focused policy, catalog, publication-idempotence, Kingmaker event-seam,
  wrapper-inventory, lifecycle-boundary, and optional-compatibility coverage.
  The complete deterministic suite contains 1,359 tests.

## 0.0.109-martial-performance-repair-notifications

- Publishes all seven active custom weapon categories to Call of the Wild's
  exact Martial Performance selector without disturbing native choices. The
  native proficiency gate, including level-up preview proficiency, remains the
  authority for visibility and selection.
- Allows ordinary repair of a loaded Broken firearm. A successful repair
  consumes one repair kit, restores Normal condition, and destroys every round
  and loaded-ammunition identity held by that exact firearm; every rejected or
  rolled-back attempt restores the complete prior state.
- Adds one concise native transient warning after each committed Normal-to-
  Broken or Broken-to-Wrecked degradation while retaining the existing combat
  log entry. Recovery, prevention, unchanged, hydration, and failed-commit
  paths do not notify.
- Adds focused selector, repair-transaction, state-machine, notification,
  rollback, and guarded runtime coverage. The complete deterministic suite is
  1,348 tests.

## 0.0.108-icon-art-polish-round-2

- Removes the baked inset frame from the shared Blunderbuss, Musket, and Pistol
  parameter icons and reduces their decorative glyph mass to native-category
  scale while preserving one centralized sprite per firearm kind.
- Replaces the Cord of Stubborn Resolve's top-down circular silhouette with an
  original, horizontally dominant oblique braided-cord belt with a visible
  front/rear relationship, knot, and hanging ends.
- Adds exact-size deterministic comparisons, protected-asset hash enforcement,
  six curated 1920x1200 guarded live-sprite renders, and focused runtime
  assertions for shared selector references and native-belt isolation.
- Retains the accepted Rapid Reload, firearm item, Eastern weapon, and Elven
  Branched Spear art byte-for-byte; Rifle and Revolver remain retired from
  ordinary supported selections. The deterministic suite remains 1,325 tests.

## 0.0.107-icon-art-overhaul

- Makes the official firearm set exactly Blunderbuss, Musket, and Pistol across
  Rapid Reload, native weapon feats, Gun Training, starting access, vendors,
  loot, and crafting. Stable Rifle/Revolver blueprints remain hidden and
  recognized only for old-save and deliberate Toy Box compatibility.
- Rebuilds Rapid Reload as a larger transparent muted-red native-style glyph
  and replaces firearm parameter cards with full-square decorative B/M/P
  monograms shared by every supported selector.
- Replaces the three supported firearm item icons with transparent,
  diagonal-fill artwork and re-renders all six Eastern category/capstone
  textures plus the Elven Branched Spear used by 30 and 12 item variants.
- Adds deterministic high-resolution art, downsampling, alpha/dimension
  validation, a guarded live-sprite visual-evidence scenario, five curated
  1920x1200 frames, and a before/after contact sheet. The complete deterministic
  suite contains 1,325 tests.

## 0.0.106-fatigue-authority-repair

- Restores native authority for ordinary canonical fatigue: successful
  Fatigued refreshes, merges, extensions, and reapplications remain Fatigued
  instead of being globally reinterpreted as Exhausted by KMG.
- Scopes the accepted Acadamae Graduate failed-save escalation to one exact,
  synchronous `BuffCollection` and canonical Fatigued blueprint request using
  a thread-local, nested-safe, one-shot intent that is always disposed.
- Preserves rule-first immunity handling, native duration/reference behavior,
  direct Exhausted idempotence and no-downgrade behavior, plus global Cord of
  Stubborn Resolve substitution and exactly-once damage routing.
- Replaces unconditional escalation tests and runtime fixtures with explicit
  native-refresh, Acadamae, Cord, immunity, duration, persistence, rest, and
  intent-leak coverage. The complete deterministic suite contains 1,323 tests.

## 0.0.105-player-facing-presentation-item-discoverability

- Moves Brown-Fur Transmuter before the installed combined Arcanist
  archetypes while preserving all foreign order and rollback guarantees.
- Replaces developer-facing item, enchantment, and Finesse Training text with
  direct player rules, backed by a live 56-item/12-enchantment text census.
- Moves Cord of Stubborn Resolve to a persistent capital-inn chest and
  redistributes every project-added magic item across 30 exact containers in
  29 persistent campaign areas, with explicit retired-target cleanup.
- Adds focused policy/source tests and guarded runtime assertions for exact
  archetype order, tooltip cleanliness, location safety, density, module
  gating, and fixed-loot uniqueness.

## 0.0.104-summon-same-turn-activation

- Corrects accelerated summoning in turn-based combat so a genuine summon
  created during its caster's active turn receives its lawful current-round
  opportunity instead of remaining locked by Full-Round appearance state.
- Correlates the exact real `UnitUseAbility`, `RuleCastSpell`, and
  `RuleSummonUnit` invocation without changing initiative, turn order, summon
  AI, caster resources, ordinary Full-Round timing, RTwP, or non-summon spawns.
- Preserves native duration by removing only the six-second Full-Round grace
  misapplied to Standard/Swift invocations; duplicate callbacks are idempotent
  per summoned unit, including multi-creature KMG summons.
- Adds guarded save-backed and save-free real-player-path coverage for
  Quickened, Acadamae, KMG multiple, native, RTwP, negative, duration, and
  exact optional-mod compatibility profiles. The deterministic suite contains
  1,307 passing tests.

## 0.0.103-overhaul-summon-menu-fatigue-escalation

- Removes Overhaul Firearm's explicit 60-second `GameTime` delivery loop. The
  full-round command now revalidates and commits the existing exact-item atomic
  transaction promptly at ordinary delivery, without advancing world time.
- Bounds exact published Expanded Summoning action-bar variant menus in the
  rendered canvas-safe rectangle. Oversized runtime lists retain every native
  slot in one reusable scroll viewport; short lists keep native presentation.
- Coordinates exact canonical Fatigued and Exhausted applications only after
  Kingmaker's native rule accepts the incoming buff. Repeated Fatigued now
  becomes one Exhausted condition, Acadamae uses the shared path, and Cord
  substitution remains once per resolved incoming effect.
- Adds focused geometry, condition-policy, installed-contract, and guarded
  actual-path coverage; the complete dependency-free suite contains 1,288
  passing tests.

## 0.0.102-starter-bokken-combat-log-acadamae-toggle

- Grants the exact owner-bound battered Pistol or Musket plus the established
  starter supplies after a committed first Gunslinger multiclass level, with a
  durable per-unit receipt, exact rollback, and native chargen/respec
  idempotence.
- Consolidates Black Powder, Lead Ball, Paper Cartridge, Repair Kits, Overhaul
  Kits, and one exact Gunsmith's Kit at Bokken while removing every exact
  project-owned firearm-supply row from Oleg.
- Replaces routine warning overlays with concise native combat-log entries and
  retains detailed event diagnostics in the mod log.
- Makes Acadamae Graduate command construction authoritative on the native
  activatable's current `IsOn` state while preserving already-armed command
  snapshots through later deactivation.
- Adds focused transaction/policy tests and guarded native lifecycle coverage;
  the complete dependency-free suite contains 1,251 passing tests.

## 0.0.101-craft-magic-items-compatibility

- Promotes the owner-accepted 0.0.100 Craft Magic Items candidate to a public
  release without changing its qualified gameplay, crafting, persistence,
  optional-dependency, or IMGUI architecture.
- Retains exact 20-unit ammunition batches with target 5 and scale-1.0 prices
  34/4/40 gp; exactly three firearm creation bases and five recognition
  identities; ordinary Martial/Exotic then Arms and Armor workflow; and exact
  marker-only tooltip suppression.
- Records final human acceptance and advances all active version, runtime,
  package, validation, and publication contracts to 0.0.101.

## 0.0.100-craft-magic-items-post-human-refinement

- Preserves the human-tested 0.0.99 inner ammunition UI interception while
  setting every exact 20-unit KMG ammunition timed project to target 5.
  Scale-1.0 prices remain 34/4/40 gp, and exact legacy target-50/60 projects
  migrate idempotently without changing progress, gold, result, or ordering.
- Separates firearm creation policy from mechanical recognition. Pistol,
  Musket, and Blunderbuss remain from-scratch bases; Advanced Rifle and
  Advanced Revolver remain loadable, upgradeable, indexed, and
  Reliable-compatible but are not newly published campaign acquisition bases.
- Removes the standalone Eastern and Elven Weapons magic category. Nodachi
  remains in Martial, Wakizashi/Katana/Elven Branched Spear remain in Exotic,
  and owned results use CMI's ordinary Arms and Armor upgrade workflow.
- Filters only exact KMG state-token and battered-origin marker enchantments
  from native player-facing weapon-quality text. The markers remain on the
  item and real qualities such as Anarchic, Enhancement +5, and Reliable are
  unaffected.
- Expands deterministic and guarded real-CMI qualification for project
  targets, migration, cancellation refunds, acquisition/recognition indexes,
  owned custom-family upgrades, tooltip structure, UI routing, and package
  isolation.

## 0.0.99-craft-magic-items-ammunition-ui-repair

- Removes the rejected conditional whole-method CMI mundane renderer prefix.
  CMI retains its outer category and parent/subtype selection; a
  capability-probed Harmony 2 transpiler intercepts only the finalized exact
  Firearm Ammunition data object before `NewItemBaseIDs` and returns to CMI's
  common money footer.
- Preflights an immutable lower-panel plan, recursively unwraps reflected UI
  failures, rethrows without invoking the ordinary body after partial custom
  rendering, and defers bridge disable/graph rollback to `OnUpdate`.
- Adds deterministic patch-shape, event-route, and deferred-failure coverage,
  plus a guarded real-CMI observer for Layout/Repaint routing, immediate and
  timed ammunition crafting, exact costs/counts/projects, KMG consumption, and
  request-local state restoration.
- Retains every qualified 0.0.98 graph, Reliable, weapon, module-state,
  persistence, optional-dependency, SoundBank, and packaging contract.

## 0.0.98-craft-magic-items-compatibility

- Rejected by first human ammunition UI testing: switching to Firearm
  Ammunition alternated whole-method ownership across IMGUI passes and caused
  `SelectionGrid`, layout-group, and GUIClip failures. Superseded by 0.0.99.
- Adds an automatic optional reflection/Harmony bridge for active Craft Magic
  Items 2.1.0 installations without a static or packaged CMI dependency.
- Publishes dedicated CMI Firearms and Firearm Ammunition categories, all five
  authorized production firearm bases, the canonical Eastern and Elven
  Branched Spear bases, exact 20-unit ammunition recipes, and the existing KMG
  Reliable enchantment.
- Keeps named campaign weapons upgrade-only, observes every owning module and
  firearm availability gate, restricts Reliable by the canonical firearm
  marker at final creation, and transfers item-owned firearm state across CMI
  upgrades.
- Adds capability probing, both-load-order lifecycle integration, exact-once
  registration, transactional rollback, bounded diagnostics, a read-only UMM
  status, ten deterministic tests, and a guarded real-CMI qualification
  scenario.

## 0.0.97-compatibility-attribution-audit

- Completes controlled attribution matrices for Favored Class/Helpful,
  polymorph and view teardown, and KMG's firearm, Elven Branched Spear, and
  Eastern Weapons AssetBundles. No KMG production defect was found, so this
  release contains no speculative compatibility repair or asset rebuild.
- Classifies the four observed Favored Class JSON startup failures and all
  reproduced shader, particle-mesh/read-write, missing-script, lightmap, and
  missing-material-property warning fingerprints as external to the tested
  KMG behavior or bundles. The reported level-up, polymorph/view, zero-area
  mesh, and `UnitFxVisibilityManager` failures were not reproduced.
- Retains a guarded, request-only, process-local asset-family suppression and
  inventory scenario. It is unavailable during ordinary gameplay, writes no
  save or module state, and leaves normal asset behavior unchanged on the next
  launch.
- Adds four focused attribution contract tests. The complete dependency-free
  suite contains 1,228 passing tests, with all established blueprint identities
  and firearm SoundBank bytes preserved.

## 0.0.96-firearm-audio-restoration

- Restores custom firearm reports by parsing the canonical schema-1 manifest
  with an exact private `JsonTextReader` contract isolated from process-global
  `JsonConvert.DefaultSettings` and third-party contract resolvers.
- Rejects missing, duplicate, malformed, mistyped, unknown, and unsupported
  manifest fields with stage-specific diagnostics while preserving the
  fail-soft boundary between audio and committed firearm mechanics.
- Adds production-loader coverage for the checked-in and copied manifest,
  hostile global serializer defaults, strict failure cases, source/package/live
  parity, bank/package allowlists, retry behavior, and process-lifetime Wwise
  readiness. The complete suite contains 1,224 passing tests.
- Adds concise `manifest.read`, `manifest.validated`, `bank.staged`, and
  `bank.ready` startup evidence and extends the save-free guarded audio scenario
  across all five Event families, live and global emitters, committed hit/miss,
  misfire/rejection, Scatter, deed, and native-crossbow boundaries.
- Preserves the existing `KMG_Firearms.bnk` byte-for-byte. The repository owner
  listened to the installed qualified candidate, reported that the sound effect
  was working, and explicitly approved release.

## 0.0.95-immediate-action-economy

- Replaces the incorrect current-instant `HasSwiftAction()` gate with a
  turn-aware immediate-action adapter. An idle off-turn protector may now use
  In Harm's Way even though Kingmaker cannot issue a normal swift command on
  another unit's turn.
- Charges an off-turn interception against the protector's next actual turn,
  blocks native and third-party swift commands throughout that turn, and
  refreshes only after that turn completes. Delay preserves the debt; global
  round changes do not clear it.
- Uses Kingmaker's native six-second shared swift cooldown for own-turn and
  real-time-with-pause reactions, and rejects genuinely flat-footed or
  incapacitated protectors with a persistent, exact combat-log reason.
- Adds a guarded native-turn scenario covering the human off-turn hit, a
  second-interception denial, next-turn swift denial, post-turn refresh, and a
  confirmed critical while preserving Bodyguard, Helpful, full-delivery
  redirection, Shield Other, and the 0.0.93 compatibility work.

## 0.0.94-in-harms-way-runtime-repair (qualification candidate)

- Replaces the former aggregate `immediate-unavailable` outcome with a bounded,
  per-protector In Harm's Way gate snapshot covering exact feat, activatable,
  marker, ability-state, native swift cooldown, action availability, delivery
  contract, arbitration, and target-redirection decisions.
- Makes both persistent reaction modes deactivate immediately, preventing an
  activatable that is visibly off from retaining its hidden consent marker
  until a later turn boundary. Activation remains free, off by default,
  save-stable, and independent for Bodyguard and In Harm's Way.
- Adds a concise combat-log explanation when an enabled In Harm's Way reaction
  is rejected specifically because the native shared swift/immediate budget is
  unavailable; mode-off attacks remain silent.
- Qualifies real native normal and confirmed-critical weapon delivery after a
  Helpful +4 Bodyguard contribution. The original roll and confirmation remain
  unchanged, the native swift cooldown advances once, the victim loses no HP,
  the interceptor receives physical damage and attack-linked riders once, and
  both target fields restore after delivery.
- Preserves the complete 0.0.93 Eastern Weapons, Favored Class, Tweak or Treat,
  Call of the Wild Aid Another, Helpful, AC-attribution, In Harm's Way, and
  Shield Other contracts.

## 0.0.93-eastern-favored-compatibility (qualification candidate)

- Defers Nodachi publication into broad Martial Weapon Proficiency facts until
  the first UMM update after the complete `LoadDictionary` postfix chain. This
  keeps Favored Class 1.3.1 from treating KMG's runtime-only category value
  `4934986` as a foreign Heirloom Weapon blueprint name during trait creation.
- Applies the late martial mutation transactionally and exactly once across
  every verified broad-martial grant, with exact-array rollback and native
  category authority checks. Standalone Eastern Weapons and existing-save
  proficiency behavior remain intact.
- Validates the exact installed Favored Class and Tweak or Treat Heirloom
  contracts. When traits and Eastern Weapons are enabled, publishes one
  save-stable KMG **Heirloom Weapon: Nodachi** choice with the installed
  three-option proficiency, attack-of-opportunity, and wielded-CMB structure.
- Preserves the canonical Call of the Wild Aid Another resolver, combat and
  halfling Helpful replacement semantics, variable Bodyguard AC contributions,
  native AC attribution, In Harm's Way delivery, and Shield Other ordering.

## 0.0.92-helpful-aid-another (qualification candidate)

- Adds the KMG combat **Helpful** trait to compatible Favored Class Combat
  Traits and shares one canonical Call of the Wild Aid Another grant resolver
  with ordinary Aid Another and Bodyguard.
- Resolves combat Helpful as +3, Favored Class halfling Helpful as +4, and
  invalid dual ownership as the better +4 replacement while preserving
  independent canonical contributors such as Benevolent.
- Keeps both optional integrations late-bound, transactional, idempotent, and
  fail-closed; KMG remains fully standalone when either external mod is absent.

## 0.0.91-bodyguard-ac-breakdown (qualification candidate)

- Preserves the qualified Bodyguard and In Harm's Way mechanics while adding
  truthful native expanded attack-detail attribution for successful Bodyguard
  AC contributions.
- Adds one attack-scoped `Bodyguard +2` `RuleCalculateAC.BonusSource` per
  successful protector, sourced from that protector's actual Bodyguard feat
  fact; failed attempts and ineligible/disabled cases add no source.
- Retains the single post-firearm `TargetAC` write, so ordinary AC 13 becomes
  exactly 15 for one protector and 17 for two protectors without double-counting.
- Extends pure, installed-assembly contract, shared-postfix, and guarded runtime
  evidence for one/two protectors, failed Aid, duplicate callbacks, firearm touch
  AC ordering, In Harm's Way delivery, and module-disabled behavior.

## 0.0.90-bodyguard-in-harms-way (qualification candidate)

- Adds the selectable Bodyguard and In Harm's Way combat feats with exact
  Combat Reflexes and Bodyguard prerequisites, native donor icons, and separate
  persistent automation modes that default off.
- Adds the default-enabled `bodyguard-feats` module, schema-7-to-8 migration,
  nine-module publication gating, and the 20-state runtime boundary contract.
- Uses native attack-of-opportunity availability and expenditure semantics,
  native reach/threat and target-aware melee attack calculations, stackable
  attack-scoped AC, and deterministic multi-protector arbitration.
- Uses the shared immediate/swift action budget and redirects the original
  attack delivery after its roll is resolved but before damage and on-hit
  recipients are consumed; it does not clone, replay, reroll, heal, or perform
  a RuleDealDamage-only transfer.
- Adds focused policy, blueprint, publication, installed-assembly, Harmony 1.2,
  nested-frame, associated-rider, Shield Other, and guarded disposable-runtime
  qualification coverage.

## 0.0.89-weapon-presentation-calibration (qualification candidate)

- Calibrates every production pistol, revolver, musket, blunderbuss, and rifle
  from authored grip/muzzle/butt/up frames while preserving projectile and
  muzzle semantics.
- Aligns long-gun dominant grips, shoulder stocks, support-hand fore-ends, and
  independent back models against native crossbow presentation controls.
- Corrects all Elven Branched Spear head polarity, branch roll, hand placement,
  thrust direction, and independently authored stored frames.
- Canonicalizes Wakizashi, Katana, and Nodachi grip/tip/blade-normal frames and
  replaces detached inherited donor sheaths with independent custom storage.
- Deliberately hides stored handguns where no plausible native holster exists;
  held, firing, transition, and inventory-preview visibility remain intact.
- Adds guarded Steam evidence for 22 production variants and six native
  controls across held/stored, attack, firing, thrust, reload, transition,
  locomotion, male/female Medium, Small, Enlarged, heavy-armor, and cloak
  configurations.
- Preserves all stable blueprint identities, native donor blueprints, combat
  rules, sounds, trails, attachment slots, and the complete `0.0.88` nonvisual
  repair set.

## 0.0.88-overnight-gunslinger-bugfixes (qualification candidate)

- Repairs Acadamae Graduate's accelerated-cast lifecycle, one-save terminal
  handling, ordinary fatigue persistence, and duplicate prerequisite prose.
- Makes Focused Aim's damage marker and shared Grit debit one verified
  transaction, including zero-Grit, duplicate-callback, and True Grit cases.
- Requalifies early/advanced firearm Touch AC boundaries using authoritative
  attack distance and adds truthful item plus battle-log penetration feedback.
- Stocks exact maintenance kits at Oleg and ammunition at Bokken through
  bounded idempotent rollback-owned vendor transactions.
- Moves Border Sentinel later and distributes all 30 scoped named/unique
  project magic items across 30 distinct deterministic base-campaign targets.
- Restores once-only native Wwise event routing for all firearm discharge paths
  while retaining fail-soft mechanics and exact packaged SoundBank validation.
- Adds distinct native-style firearm monograms and Rapid Reload art.
- Normalizes and rigs the Elven Branched Spear, Musket, and Blunderbuss assets
  with deterministic scale-one held frames, native support IK, exact muzzle
  markers, and independent back presentations.
- Preserves every existing stable blueprint GUID and records remaining human
  visual, audible, ordinary-UI, merchant/container materialization, and pacing
  checks without claiming automated proof.
- Final candidate gates passed: version-aware repository validation, all
  1,160 dependency-free tests, clean installed-reference Release build,
  build-output and SoundBank validation, strict standalone package validation,
  compatibility profile/schema suites, the exact 30-item guarded acquisition
  observer, and canonical `KMG_AUTOMATION_WORKING` smoke.

## 0.0.87-urban-barbarian-human-review-repair-4 (development candidate)

- Supersedes immutable 0.0.86 after focused, persistence, and all three CotW
  profiles passed but the first all-ON module boundary exposed the same stale
  legacy-selector assumption in the generic module observer.
- Repairs that packaged observer to validate 73 identities, an inert legacy
  save identity, and exact 6/10/15 live tier parents in every boundary state.
- Extends regression validation across both Urban inventory observers and
  removes the remaining stale 70-identity presentation text.
- Immutable candidate `0.0.87` passed all 1,150 dependency-free tests, a clean
  Release build, standalone package validation, focused Urban mechanics,
  two-launch active-Rage/module-OFF persistence, CotW normal/balance/absent
  profiles, and the authoritative 18/18 module boundary. It is installed
  unchanged for renewed human presentation/play review; it is not yet human
  accepted or release complete.

## 0.0.86-urban-barbarian-human-review-repair-3 (development candidate)

- Supersedes immutable 0.0.85 after focused gameplay and module-OFF
  persistence passed but its packaged CotW inventory observer crashed on the
  now-inert legacy selector before producing compatibility assertions.
- Updates the packaged observer to validate all 73 Urban identities, the
  hidden/inert legacy save identity, and the three actual player-facing
  selector parents with exactly 6, 10, and 15 variants.
- Adds source regression coverage that rejects any observer which again treats
  the legacy selector as the live allocation parent.
- Runtime result: focused mechanics, module-OFF persistence, and CotW normal,
  balance-fixes, and absent profiles passed. The first all-ON boundary state
  failed before assertions because the separate generic module observer still
  called `.Single()` on the inert legacy selector. The exact artifact is
  superseded and is not human accepted or release complete.

## 0.0.85-urban-barbarian-human-review-repair-2 (development candidate)

- Preserves 0.0.84 as a failed immutable runtime candidate after its focused
  run proved the exact Crowd Control attack and AC deltas but found that the AC
  rule omitted its `BonusSources` attribution.
- Adds the exact Crowd Control fact to the AC rule's source inventory while
  retaining the genuine temporary +1 dodge modifier and owner-scoped rule-event
  implementation.
- Keeps the repaired 6/10/15 tier selectors, distinct allocation icons,
  selected-state presentation, Controlled Rage Trickery permission, and native
  spellcasting prohibition unchanged.
- Runtime result: focused mechanics and two-launch module-OFF persistence
  passed, but the CotW-normal inventory observer threw `Sequence contains no
  elements` because it still expected the rejected 31-variant root. The exact
  artifact is superseded and is not human accepted or release complete.

## 0.0.84-urban-barbarian-human-review-repair (development candidate)

- Replaces the rejected single 31-entry Controlled Rage allocation surface with three owner-granted tier selectors containing exactly 6, 10, and 15 variants.
- Preserves the 0.0.83 selector identity as hidden save compatibility while adding stable ordinary, Greater, and Mighty selector identities.
- Adds distinct native-donor and repeated-glyph composite allocation icons plus a selected green-border/check presentation.
- Reopens focused Crowd Control, skill-use, persistence, CotW-profile, and 18-state runtime qualification; this candidate is not human accepted.
- Runtime result: **FAILED and superseded**. The exact command-path fixture
  observed +1 attack and +1 target AC at two adjacent hostiles, but the AC
  `BonusSources` inventory lacked Crowd Control and the save-free fixture had no
  campaign combat-log subscriber. It is not human accepted or release complete.

## 0.0.83-urban-barbarian (development candidate)

- Adds Urban Barbarian as an eighth independent, default-enabled native
  Barbarian feature module. Its 70 stable blueprints register in every module
  state; the setting controls only new-selection publication.
- Replaces medium-armor proficiency and Fast Movement with exact consolidated
  urban class skills, Crowd Control, and Controlled Rage.
- Provides one nested allocation selector with all 6, 10, and 15 legal +4,
  +6, and +8 Strength/Dexterity/Constitution allocations. Tier selections are
  independent, default to full Strength, persist until changed, and cannot be
  changed while raging.
- Preserves native Rage resource, per-round spending, fatigue, Tireless Rage,
  spellcasting restriction, Rage condition/descriptor integration, and Rage
  powers while removing ordinary attack, damage, temporary-HP, Will, and AC
  effects for Urban owners only.
- Implements Crowd Control from attack and AC rule events using native
  edge-to-edge five-foot distance, active hostility/life state, and no reach
  expansion.
- Records crowd movement and crowd-influence clauses as intentional no-ops:
  Kingmaker exposes no precise subsystem and broad movement or Persuasion
  substitutes would change the rule.
- Advances feature settings to schema 7 and the generic runtime boundary to
  18 states. Call of the Wild remains optional for Urban Barbarian and for the
  package.

## 0.0.82-brown-fur-human-review-repair

- Records the `0.0.81` Brown-Fur candidate as human-review failed and
  superseded; it was never accepted and does not authorize the final matrix.
- Replaces Powerful Change's legacy instant score actions with six native,
  mutually exclusive one-shot activatable toggles. Native `IsOn` state drives
  the selected overlay, manual deselection, switching, and committed-cast
  cleanup while ineligible and canceled casts preserve the armed score.
- Gives every score and Share Transmutation a distinct native Kingmaker donor
  icon and a native live Arcane Reservoir counter bound with non-spending
  activation semantics. The cast transaction remains the only debit authority.
- Moves Share Transmutation eligibility into the pre-command target-anchor
  path. An armed supported Personal Transmutation reports a unit anchor before
  a target or transaction exists, enters native target selection, and retains
  ordinary Personal self-cast behavior while Share is off.
- Expands guarded evidence for Beast Shape II, Undead Anatomy I, Resinous Skin,
  Bull's Strength, Cat's Grace, cancellation, combined use, typed modifier
  preservation, resource counters, and activatable save/load synchronization.
- Advances assembly/package identity to `0.0.82`. The complete 1,138-test
  domain suite, 40 focused runtime launches with 435 passing assertions,
  persistence, normal/balance/absent/high-risk profiles, and all 16 boundary
  states pass on one immutable installed artifact. Human presentation and play
  review accepted that exact artifact on 2026-08-16. The revised runtime policy
  makes the `2N + 2` boundary authoritative and requires no exhaustive
  game-launch release seal.

## 0.0.81-brown-fur-transmuter (development)

- Adds the independent, default-ON `brown-fur-transmuter` feature-module
  setting while keeping Call of the Wild optional for the overall package.
- Adds fail-closed reflection-only CotW Arcanist, progression, reservoir,
  spellbook, Magical Supremacy, and Shared Spells contract resolution.
- Supports the known CotW normal and balance-fixes exploit schedules through a
  deterministic policy, rejecting unknown or ambiguous schedules.
- Implements descriptor-preserving Powerful Change, owner-scoped Share
  Transmutation, free non-stacking Transmutation Supremacy, immutable per-cast
  accounting, stable persistence, and transactional CotW Arcanist publication.
- Qualifies CotW normal and balance progressions, CotW absence, module-OFF
  existing owners, the highest-risk combined profile, and all 16 seven-module
  boundary states on one immutable pre-human candidate. That candidate was
  superseded by the accepted `0.0.82` human-review repair.

## Unreleased - Eastern Weapons

- Repairs the first human-playtest candidate with seven diagonal inventory
  icons, narrower curved single-edged family meshes, normalized all-30 family
  prefab resolution, forward/two-handed presentation donors, and exact Call of
  the Wild Focused Weapon integration for all four KMG custom categories.

- Adds the independent, default-enabled **Eastern Weapons** module with stable
  Wakizashi, Katana, and Nodachi categories; 12 generic items; and 18 named
  magical weapons forming three complete Act I-to-late-game progressions.
- Publishes **Weapon Proficiency (Katana)** and **Weapon Proficiency
  (Wakizashi)** through the native merged Exotic Weapon Proficiency catalog.
  Katana uses native grip state: exact exotic proficiency works in either grip,
  while broad martial proficiency works only when it is wielded two-handed.
- Makes Nodachi martial and integrates it with Heavy Blades and Polearms
  training without reach, Brace, or duplicate training bonuses. Wakizashi uses
  native light/finesse behavior and singular Dexterity-to-damage routes.
- Adds Paper Lantern through Night Without Moon, Wayfarer's Oath through
  Heaven's Measure, and Border Sentinel through World-Tree Severer. Native
  enchantments are exact installed blueprint references; Heaven's Measure uses
  the positively qualified native Brilliant Energy property.
- Adds exact live-rule implementations for Falling Petal, Wayfarer's Oath,
  Moonlit Crossing, Mountain-Sunder, and Unfixed Form, including critical,
  grip, active Power Attack, once-per-round, polymorph, size, equipment-set,
  and weapon-switch boundaries.
- Adds verified Act I, capital, regional, Pitax, and fixed-loot progression.
  All 12 generics appear exactly once in each of four installed Beneath the
  Stolen Lands weapon tables (48 additive rows); named weapons are excluded
  from ordinary BTSL stock.
- Adds three project-owned Blender/FBX equipped models, six original 128x128
  icons, a deterministic Unity 2018.4.10f1 bundle, structural runtime checks,
  and native donor fallbacks.
- Advances the release to assembly version `0.0.80` and informational/package
  identity `0.0.80-eastern-weapons`.
- Qualifies 1,048 dependency-free tests, all 64 module states, standalone,
  Call of the Wild, Arms and Armor, Toggle Custom Soundpacks, maximum combined
  profiles, three-phase module-disabled persistence, and canonical working-save
  smoke. Subjective visual acceptance remains a human review item.
- Defers tabletop Deadly because the installed engine exposes no reliable
  coup-de-grace Fortitude-DC hook. No approximation or ordinary damage bonus is
  added. Brace remains intentionally out of scope.

## 0.0.79 - Elven Branched Spears

- Names the Exotic Weapon Proficiency child **Weapon Proficiency (Elven
  Branched Spear)**, excludes it from the prioritized top block, and places it
  immediately above Elven Curve Blade through the native merged selector
  ordering path, including Call of the Wild compatibility.
- Advances the release candidate to assembly version `0.0.79` and
  informational/package identity `0.0.79-elven-branched-spear`; package
  selection is explicit and can no longer validate the prior Expanded
  Summoning archive by accident.
- Resolves the stable custom weapon category through Kingmaker's central
  category-name path, so prerequisite and feature surfaces display **Elven
  Branched Spear** instead of decimal `4934983` or hexadecimal `0x004b4d47`.
- Aligns selector presentation with native behavior: Exotic Weapon
  Proficiency shares its native icon, Finesse Training is named **Finesse
  Training (Elven Branched Spear)** and uses spear art, and parameterized
  weapon feats use the native decorative `EB` category glyph.
- Adds all six generic spear tiers to the two standalone and two campaign
  Beneath the Stolen Lands weapon-vendor tables, additively and idempotently.
- Adds an independent, default-enabled `elven-branched-spears` module and one
  stable exotic, two-handed, finesse-compatible reach-weapon category with
  native Elven Weapon Familiarity and Spears fighter-group integration.
- Adds mundane, masterwork, cold iron, masterwork cold iron, +1, and +1 cold
  iron progression plus Boughkeeper, Thornstep, Moonlit Fork, Viper's Reach,
  Briar-Crowned Spear, and Spear of the First Branch.
- Publishes the category idempotently through Exotic Weapon Proficiency, Rogue
  Finesse Training, Weapon Focus, Greater Weapon Focus, Improved Critical,
  Weapon Specialization, Greater Weapon Specialization, Sword Saint Chosen
  Weapon, and Weapon Mastery while preserving all native prerequisites.
- Adds the exact +2 movement-provoked attack-of-opportunity modifier at the
  native disengagement command boundary. Ordinary, charge, generated,
  spellcasting, ranged-attack, and other nonmovement attacks do not receive it.
- Preserves native Weapon Finesse, Finesse Training, Agile, Call of the Wild
  Fighter's Finesse, and Trained Grace semantics without double Dexterity;
  one-handed, Grace, named-weapon, and Dervish restrictions remain unchanged.
- Adds verified Act I-through-final-act vendor and fixed-loot progression with
  append-only, idempotent, module-gated publication.
- Adds an original project-owned Blender/FBX/icon asset, dedicated Unity
  2018.4.10f1 bundle, transactional validation, and native Longspear fallback.
- Qualifies at least 1,033 dependency-free tests, all 32 module combinations, isolated
  Call of the Wild combat, module-OFF item/feature save persistence and cleanup,
  strict package validation, and the canonical working-save smoke. Brace and
  pseudo-Brace behavior are intentionally not implemented or advertised.

## 0.0.78 - Expanded Summoning

- Replaces every player-visible Expanded Summoning child icon with one of 77
  project-owned original creature icons. The 128x128 RGBA set is manifest-
  validated, packaged deterministically, cached once per creature, and never
  falls back to an Owlcat spell, item, portrait, paw, or generic summon icon.
- Rebuilds nine preserved Nature's Ally choices as creature-named KMG wrappers,
  removing the duplicated generic Tier-I entries and white-square child icons.
  All 322 visible SNA and 371 visible SM choices now resolve to exact creature
  icons, including lower-tier quantity choices.
- Renames the player-facing `Dire Tiger / Smilodon` concept to `Smilodon` while
  retaining the frozen `dire-tiger` key and all existing blueprint GUIDs.
- Reduces Eagle's view-only multiplier to `0.30`; live renderer measurement is
  1.360 units tall versus 1.926 for the Medium humanoid control. Mechanical
  Small size, selection footprint, animations, and shared SM/SNA unit remain
  unchanged.
- Splits the remaining Owlcat hybrid summon umbrellas into 17 distinct,
  directly executable choices while retaining every original blueprint
  identity; hides the five obsolete umbrella children and all 14 Dire Bat
  placements whose Roc proxy failed visual acceptance.
- Adds deterministic original creature icons and measured view-only scaling
  for Eagle, Poisonous Frog, Dire Boar, Pteranodon, Dire Bear, Elephant,
  Mastodon, and Roc. Dire creatures now read larger than their ordinary
  analogues without changing mechanical size or navigation footprint.
- Requalifies all 667 visible generated SM/SNA roots plus 26 native/preservation
  choices through real spellbook parents, representative combat and special
  actions, 67 live views, enabled/disabled persistence, all 16 module states,
  and all five supported compatibility profiles.
- Repairs the first-playtest no-op for templated natural/proxy summons by
  replacing unsupported nested player-facing variants with direct executable
  roots and caster-selected post-spawn templates.
- Reconciles 48 exact-GUID native semantic duplicates, orders singles before
  `1d3` before `1d4+1`, adds category/donor icon selection, moves Invisible
  Stalker to the Medium Air Elemental view, and rebuilds Erinyes on a safe
  outsider chassis.
- Adds an actual native-parent spellbook acceptance scenario covering all 681
  logical roots, exact one-slot success semantics, zero-slot pre-cast
  cancellation, post-creation live-world state, and exact cleanup.

- Adds an independent, default-enabled, restart-bound Expanded Summoning
  module for Summon Monster I-IX and Summon Nature's Ally I-IX.
- Adds the approved 66-entry Summon Monster roster and 57-entry Nature's Ally
  roster, sharing 67 summon-safe creature identities and generating all 681
  legal one/1d3/1d4+1 same-kind placements.
- Preserves every vanilla and third-party summon option by reference and order
  through additive, idempotent, transactional final-live reconciliation.
- Adds caster-aligned celestial/fiendish Summon Monster choices, Nature's Ally
  alignment handling, native summon lifecycle/feat integration, and bounded
  Lantern Archon, Salamander, Invisible Stalker, Shadow Demon, Succubus,
  Bebelith, and Pixie adaptations.
- Keeps all 1,184 Expanded Summoning identities registered when the module is
  disabled so existing saves and active summons remain load-safe.
- Corrects Shield Other so close range limits initial targeting only; an
  established link now ends on duration/removal, dead or missing endpoints, or
  area separation, not ordinary post-cast distance.
- Adds guarded 153-cast quantity coverage, 67-unit visual contracts,
  persistence/cleanup qualification, all 16 module states, and standalone,
  Call of the Wild, Arms and Armor, Toggle Custom Soundpacks, and combined
  compatibility profiles.

## 0.0.77 - Shield Other

- Fixes spontaneous-caster availability and action-bar corruption by preserving
  Kingmaker's required non-null empty material-component contract.
- Adds a distinct project-owned Shield Other spell/buff icon and removes the
  obsolete tabletop-item note from the player-facing description.
- Adds the independent, default-enabled Shield Other module with stable ability
  and target-buff identities and schema-1-to-2 settings migration.
- Publishes Shield Other at level 2 to Cleric, Paladin, Inquisitor, Community,
  and Protection; unambiguous final-live Call of the Wild Oracle, Warpriest,
  and Psychic lists are reconciled without a compile-time dependency.
- Adds +1 deflection AC, +1 resistance to all saves, caster-linked close-range
  lifecycle enforcement, and save/load-persistent caster/target/CL context.
- Splits only finalized HP damage before HP loss and downstream damage
  consumers; odd damage favors the caster, defenses apply once, and transferred
  damage cannot recurse or regenerate source riders.
- Adds guarded duplicate scans, eight-module matrix coverage, transactional list
  publication, expanded runtime mechanics, compatibility repetitions, and a
  two-fresh-launch working-save persistence/cleanup qualification.

## 0.0.76 - Acadamae mode, persistent fatigue, and Cord icon repair

- Adds the default-off, per-character `Use Acadamae Graduate` native toggle;
  mode-off casts retain native timing and no risk, while mode-on commands
  snapshot acceleration and their Fortitude-save obligation.
- Applies failed-save fatigue through Kingmaker's independent caster-context
  overload, producing indefinite canonical Fatigued that survives summoning
  context disposal and follows native remove-on-rest behavior.
- Replaces the Cord's donor icon with original project-owned transparent art.
- Preserves the accepted UMM modules, prerequisites, Cord mechanics, merchant
  placement, Paper Cartridge lifecycle behavior, and every existing GUID.

## 0.0.75 - Feature modules, Acadamae Graduate, and Cord of Stubborn Resolve

- Adds independent, persistent, default-enabled Gunslinger and Acadamae Graduate feature modules to the composed UMM panel. Changes apply after a complete restart; disabled content is hidden from new selection/acquisition while existing save identities and mechanics remain available.
- Adds Acadamae Graduate with exact specialist-Wizard and Conjuration-opposition prerequisites, prepared arcane Conjuration (Summoning) Full-Round-to-Standard casting, and one native Fortitude save at DC 15 + spell level with Fatigued on failure.
- Adds the belt-slot Cord of Stubborn Resolve: +2 enhancement Constitution; fatigue/exhaustion substitution; one capped 1d6 nonlethal-equivalent result; and one fixed 15,000-gp capital-blacksmith copy.
- Adds settings-aware, idempotent catalog reconciliation that retains native and exact local Call of the Wild classes/feats while publishing or withholding only project entries.
- Adds guarded settings, publication, Acadamae, Cord, integration, and vendor scenarios plus four-combination and exact optional-profile qualification.

## 0.0.74 Paper mode view-lifecycle repair

- Initialized the persistent Use Paper Cartridges marker's no-FX resource links
  so existing marker facts reconstruct safely during save load and area changes.
- Preserved every Paper Cartridge blueprint identity and all ammunition behavior.
- Added guarded attached-view lifecycle coverage, including Call of the Wild's
  composed `Buff.SpawnParticleEffect` path.

## 0.0.74 - Paper Cartridges, auto-reload, and rare firearms

- Adds stackable 12-gp Paper Cartridges for every canonical early Pistol,
  Musket, and Blunderbuss family, including +1 and named magic firearms.
- Adds a per-unit Use Paper Cartridges mode, persistent loaded-ammunition state,
  one-step faster reloads, and the exact +1 misfire modifier before Reliable.
- Uses one atomic reload plan for manual reload, native right-click auto-use,
  free-action full attacks, and once-per-round Lightning Reload fallback.
- Adds shared-entitlement crafting (20 for 120 gp), zero resale, and normalized
  stock of 200 at the capital blacksmith and installed BTSL vendors.
- Includes the merged rare-firearm release: eight project-owned magic early
  firearms, Reliable and Seeking, capital/BTSL stock, and five named campaign
  loot publications remain intact.

## 0.0.72 - Optional-mod compatibility framework

- Establishes the manifest-driven, exact-local-reference compatibility mission,
  read-only inventory schema/catalog, and bounded fixture-tested inventory tool.
- Preserves standalone operation and packages no third-party mod payload.

## 0.0.71 - Native firearm weapon-rig candidates

- Adds deterministic identity-grip firearm rigs with muzzle transforms and
  native `EquipmentOffsets.IkTargetLeftHand` support targets for long guns.
- Enables all five equipped models as structurally qualified autonomous
  candidates pending human visual acceptance; holstered presentation is hidden.
- Preserves inherited native attach slots so active Musket and Blunderbuss
  models remain player-visible; empty forced attach slots are explicitly
  rejected after human A/B testing.
- Adds a session-only calibration lab and guarded structural runtime evidence.

## Unreleased - Native Wwise firearm reports

- Replaces the ineffective Unity firearm-audio fallback with a native
  Kingmaker-compatible Wwise 2016.2.6.6153 SoundBank.
- Routes ordinary attacks and all custom firearm deeds at their exact committed
  discharge boundaries while preserving zero normal reports for misfires and
  rollback/rejection paths.
- Packages exactly one embedded-media `KMG_Firearms.bnk`; never packages or
  replaces `Init.bnk`.

## 0.0.70 - Focused Aim repair

- Adds an original Focused Aim toolbar/status icon.
- Replaces null duration, saving-throw, status, and terse activation presentation.
- Fixes firearm detection during weapon-stat calculation so Focused Aim adds its Charisma bonus to damage rolls as specified; it does not add to attack rolls.

## 0.0.69 - Mysterious Stranger

- Adds Mysterious Stranger as a selectable Gunslinger archetype.
- Replaces Wisdom-based Grit, Quick Clear, Nimble, Gun Training 1, and Bleeding Wound with Charisma-based Grit, Focused Aim, Lucky, Stranger's Fortune, and Clipping Shot respectively.
- Preserves later Gun Training choices and every unrelated Gunslinger deed and class feature.

## 0.0.68 - Distinct Gunslinger supply item icons

- Assigns five explicit, distinct project sprites to the authoritative Lead
  Ball, Black Powder Charge, Firearm Repair Kit, Gunsmith's Kit, and Firearm
  Overhaul Kit blueprints before vendor publication.
- Adds original project artwork for the two previously unmapped gunsmithing
  kits and fail-closed registry/vendor/crafting identity validation.
- Preserves the accepted Gunslinger's Dodge R3 implementation unchanged while
  displaying an unambiguous `0.0.68 SUPPLY-ITEM-ICONS` build label in UMM.

## 0.0.66 - Sixth-playtest repair and BTSL testing support

- Makes Gunslinger's Dodge spend immediately and grant +2 dodge AC for one round.
- Makes Deadeye spend immediately and expose a one-round Deadeye Armed buff.
- Adds once-per-rest 20/20 basic ammunition crafting with a non-consumable
  Gunsmith's Kit and atomic 22 gp completion cost.
- Publishes all five firearms and maintenance supplies idempotently to the exact
  installed standalone and campaign Beneath the Stolen Lands vendor tables.
- Adds explicit firearm presentation profiles, corrected short-firearm wrapper
  pivots, hidden-holster fallback, and a clone-derived renderer-free projectile.
- Preserves native Weapon Focus integration, Targeting Arms delivery, firearm
  item/state identities, and all qualified maintenance behavior.

## 0.0.65 - Fifth-playtest visual and native-feat repair

- Appends all five firearm parameters through the native level-up UI's actual
  `ExtractSelectionItems` path while retaining the original native feat identity.
- Assigns approved firearm models to both weapon-type and equipped item visual
  parameters so the doll no longer instantiates the inherited crossbow model.
- Plays mapped firearm discharge clips from a persistent, non-spatial unit
  emitter and keeps inherited crossbow combat sounds suppressed.
- Strengthens runtime presentation observation to reject inherited item-level
  crossbow visuals.

## 0.0.64 - Fourth-playtest runtime and UX repair

- Restored five firearm parameters inside native Weapon Focus and dependent
  feat menus while preserving native icons, gates, options, and kind isolation.
- Made Grit deeds share one finite native resource and action-bar counter, with
  atomic cost, zero-resource rejection, recovery, persistence, and True Grit.
- Added native Reload Firearm auto-use continuation with fail-closed switching,
  interruption, action-economy, Wrecked, and ammunition behavior.
- Added timed out-of-combat Overhaul, explicit condition/Quick Clear UX, useful
  firearm qualities, and production descriptions without placeholder wording.
- Replaced duplicate progression presentation with a semantic icon family and
  a native-style Rapid Reload icon.
- Added deterministic five-model/five-audio Unity 2018.4.10f1 assets, native
  weapon-model mapping, crossbow renderer/audio suppression, and corrected
  Winchester CC-BY-4.0 provenance for Advanced Rifle only.

## 0.0.63 - Third-playtest feat, reload, grit, dodge, and asset repair

- Restores the native weapon-feat family presentation and prerequisites while
  publishing exact firearm choices through the native parameter menus; obsolete
  wrappers remain hidden compatibility identities.
- Preserves the single Reload Firearm action, native right-click autocast,
  pre-command empty-firearm rejection, and the standing one-round +2 AC
  Gunslinger's Dodge adaptation.
- Exposes the one shared grit resource through native action-bar counters on all
  paid deed abilities and adds a native-palette Rapid Reload icon.
- Ships a deterministic Unity 2018.4.10f1 Windows AssetBundle containing approved
  Pistol, Musket, Blunderbuss, and Revolver prefabs plus five processed CC0 shot
  sounds. The unverified advanced-rifle binary remains quarantined.
- Passes 878 deterministic tests and exact-assembly runtime checks for UMM load,
  prefab/material resolution, shared grit UI binding, and exactly-once discharge
  audio.

## 0.0.62 - Second-playtest functional and UX repair

- Inserts Gunslinger alphabetically without globally sorting native classes and
  presents coherent Deeds, Nimble, Bonus Feat, and Gun Training progression tracks.
- Adds distinct native-style semantic icons and integrates firearm choices into
  native-style Weapon Focus, Greater Weapon Focus, Weapon Specialization, Greater
  Weapon Specialization, and Improved Critical selections while retaining hidden
  legacy Weapon Focus compatibility.
- Keeps Rapid Reload optional and firearm-type-specific, exposes one Reload Firearm
  parent command, and hides all static action-cost implementation variants.
- Adds native right-click auto-reload scheduling and rejects empty-firearm attacks
  in `UnitAttack.CreateAttackCommand`, before a UnitAttack or attack rule exists.
- Expands the deterministic suite to 868 tests. External model/audio candidates are
  not packaged unless their exact local binary provenance is established.

## 0.0.60 - Complete Base Gunslinger qualification

- Completes and runtime-qualifies every meaningful base Gunslinger feature and
  the supporting item-owned firearm system from character creation through
  level 20.
- Passes 854 deterministic tests, strict Release/package validation, final mod
  load, and two independent 32-slice comprehensive runtime acceptances.
- Records accepted Kingmaker adaptations and unsupported tabletop interactions
  in the fidelity matrix and the complete qualification report.

## 0.0.60 - Sprint 60 Player-Facing Presentation

- Adds approved fallback icons to project-owned visible Gunslinger progression
  features and granted abilities while preserving feature-specific icons.
- Groups existing visible level-entry features for native progression display
  without changing class mechanics or hidden implementation facts.

## 0.0.59 - Sprint 59 True Grit

- Establishes the centralized True Grit cost/gate policy and stable eligible
  deed catalog, including computed-cost, zero-cost, no-spend, Cheat Death, and
  Slinger's Luck exclusion boundaries.
- Adds eight focused policy cases; the complete suite is 827 tests.

## 0.0.58 - Sprint 58 Stunning Shot

- Adds the level-19 pre-shot Stunning Shot deed with exact native critical-hit
  immunity, Fortitude save, and one-round Stunned mechanics.
- Adds six focused policy cases; the complete suite is 819 tests.

## 0.0.57 - Sprint 57 Death's Shot contract observer

- Adds a guarded save-free observer for the installed native Death descriptor,
  Fortitude-saving-throw, and kill-action graph required by Death's Shot.

## 0.0.56 - Sprint 56 Cheat Death

- Adds the level-19 Cheat Death deed using the completed native damage event.
- Spends every remaining grit point (minimum one) and leaves the owner at exactly 1 HP.
- Adds six focused policy cases; the complete suite is 813 tests.

## 0.0.55 - Sprint 55 Slinger's Luck

- Adds separate level-fifteen saving-throw and skill-check reroll arming actions.
- Uses the exact native d20 source and completed-rule replacement contract,
  always retaining the second roll for fixed, non-reducible costs of 2 or 1 grit.
- Adds six focused policy cases; the complete suite is 807 tests.

## 0.0.54 - Sprint 54 Menacing Shot

- Adds the level-fifteen Menacing Shot deed as a self-centered 30-foot burst
  affecting living creatures, including the Gunslinger and allies.
- Atomically spends one grit and one loaded firearm chamber, then applies an
  exact native-Fear-derived Will-save effect at the Gunslinger deed DC.
- Adds six focused policy cases; the complete suite is 801 tests.

## 0.0.53 - Sprint 53 Evasive

- Adds the level-fifteen Evasive feature, conditionally granting project-owned
  clones of Kingmaker's exact Evasion, Uncanny Dodge, and Improved Uncanny
  Dodge mechanics while the Gunslinger has positive grit.
- Refreshes the grants on exact grit Spend/Restore transitions without
  disturbing native facts from other classes.
- Adds five focused policy cases; the complete suite is 795 tests.

## 0.0.52 — Sprint 52 Lightning Reload

- Adds the level-eleven swift-action Lightning Reload deed for one equipped
  firearm chamber once per round while grit remains, without spending grit.
- Uses the existing atomic inventory-backed reload transaction, preserves
  Broken condition, and rolls back its unit-local round marker on failure.
- Adds six focused policy cases; the complete suite is 790 tests.

## 0.0.51 — Sprint 51 Expert Loading

- Adds the level-eleven free-action pre-shot Expert Loading adaptation.
- An armed Broken early-firearm misfire spends exactly 1 grit, remains Broken,
  and suppresses the otherwise native Broken-to-Wrecked burst.
- Adds four focused policy cases; the complete suite is 784 tests.

## 0.0.50 — Sprint 50 Bleeding Wound

- Adds the level-eleven four-choice Bleeding Wound deed with free-action
  pre-shot selection, exact post-hit grit costs, ordinary firearm damage, and
  persistent native-descriptor HP or ability-score bleed.
- Adds four focused policy cases; the complete suite is 780 tests.

## 0.0.47 — Sprint 47 Targeting Legs

- Adds the level-seven full-round Targeting — Legs deed with normal firearm
  damage and a native automatic-strength Trip rider that preserves native
  sneak/trip immunity.
- Adds three focused rider-policy cases; the complete suite is 776 tests.
- Runtime-qualified native damage, successful Trip/prone aftermath, and native
  maneuver-immunity suppression in two independent guarded fresh launches.

## 0.0.46 — Sprint 46 Targeting Torso

- Adds the level-seven full-round Targeting — Torso deed with a reference-scoped
  19–20 threat range, native confirmation and multiplier, and sneak-immunity
  suppression.
- Adds three focused threat-policy cases; the complete suite is 773 tests.

## 0.0.45 — Sprint 45 Targeting Head

- Adds the level-seven full-round Targeting — Head ability.
- Spends one grit and makes one ordinary native firearm attack.
- A qualifying hit applies one round of mind-affecting native Confusion while
  preserving native sneak-attack and mind-affecting immunity handling.
- Adds five focused policy/rider cases; the complete suite is 770 tests.

## 0.0.44 — Sprint 44 Startling Shot (in progress)

- Adds the level-seven standard-action Startling Shot deed using native weapon
  targeting, one item-owned chamber, positive-but-unspent grit, no attack or
  damage event, and a one-round native flat-footed condition.
- Adds atomic firearm/buff rollback, focused policy tests, stable production
  blueprints, and a guarded save-free runtime scenario.

## 0.0.43 — Sprint 43 Dead Shot

- Added and runtime-qualified the full-round BAB-iterative Dead Shot deed with
  one discharge, base-dice-only hit aggregation, adjusted native critical
  confirmation, and all-roll aggregate misfire.

## 0.0.42 — Sprint 42 Gun Training (in progress)

- Adds cumulative firearm-kind selections at levels 5, 9, 13, and 17.
- Adds exact selected-kind Dexterity-to-damage and trained Broken-state misfire
  handling without using borrowed weapon categories as firearm identity.

## 0.0.41 — Sprint 41 Gunslinger bonus feats (in progress)

- Began exact level 4/8/12/16/20 bonus-feat integration by reusing
  Kingmaker's native prerequisite-respecting Fighter combat-feat selection.

## 0.0.40 — Sprint 40 Utility Shot (in progress)

- Classified Blast Lock and Scoot Unattended Object as having no meaningful
  supported Kingmaker interaction, and began the Stop Bleeding vertical slice
  with exact grit, range, bleed-descriptor, and one-chamber contracts.

## 0.0.39 — Sprint 39 Pistol-Whip (in progress)

- Began the level-three Pistol-Whip vertical slice with explicit handedness,
  grit, condition, native melee-attack, enhancement, and Trip contracts.

## 0.0.38 — Sprint 38 Gunslinger Initiative

- Added the level-three grit-gated +2 native initiative-check slice through
  Kingmaker's exact post-roll `IUnitInitiativeHandler` boundary.
- Added rule-object duplicate protection and a guarded detached runtime
  scenario; the conditional Quick Draw clause remains under exact contract
  review rather than guessing inventory or hand state.
- Advanced build, package, runtime-request, and repository validation guards to
  version 0.0.38 while preserving inherited Sprint 37 evidence.

## 0.0.37 — Sprint 37 class integration (in progress)

- Began the next progression slice with exact cumulative Nimble ranks at levels
  2, 6, 10, 14, and 18, using native Dodge AC semantics in light or no armor.
- Advanced build, package, runtime-request, and repository validation guards to
  version 0.0.37 while preserving inherited Sprint 36 evidence.

## 0.0.36 — Sprint 36 core deed bundle

- Began the coherent level-one Deadeye, Gunslinger's Dodge, and Quick Clear
  checkpoint on the runtime-qualified Sprint 35 grit foundation.
- Advanced build, package, runtime-request, and repository validation guards to
  version 0.0.36 while preserving all inherited Sprint 35 evidence.
- Runtime-qualified Deadeye, the Gunslinger's Dodge drop-prone branch, and both
  Quick Clear action-economy variants on exact source commits.

## 0.0.35 — Sprint 35 grit resource (in progress)

- Added the dependency-free bounded grit pool model and deterministic daily
  reset, maximum reconciliation, spend, restore, and operation-deduplication
  plumbing.
- Added 12 focused cases, bringing the complete suite to 703 tests.
- Advanced active build, package, runtime-request, and repository validation
  guards to version 0.0.35 while preserving inherited Sprint 34 checks.
- Added stable native grit resource/feature blueprints, level-one progression
  ownership, an exact Wisdom-floor maximum subscriber, initial restoration,
  and fail-closed non-refill on ordinary level-up.
- Added a guarded save-free detached-unit scenario for native grant, spend,
  level-up retention, capped restore, and cleanup qualification.

## 0.0.31 — Sprint 31 early firearm catalog (in progress)

- Began canonical production definition data with the tabletop early pistol.
- Added explicit catalog acceptance criteria for pistol, musket, and
  blunderbuss without silently inventing the blunderbuss's `special` range.
- Preserved the runtime-qualified Sprint 30 generic action and item-owned state
  baseline.

## 0.0.30 — Sprint 30 generic definition-driven firearm actions

- Accepted Sprint 29 from the combined live contract evidence and exact 0.0.29 passing maintenance matrix.
- Added one marker-first exact-equipped-firearm context shared by Reload, Overhaul, and Repair.
- Added common action decisions and dependency-free eligibility policy.
- Added definition-owned ammunition identity and definition-driven capacity/ammunition Reload behavior.
- Preserved stable Test Musket blueprints and accepted delivery-time transaction/rollback services as adapters.
- Added 12 focused tests; the 611-test portable suite passes with zero failures.
- Kept the early firearm catalog and capacity greater than one deferred.

## 0.0.29 — Sprint 29 complete maintenance loop and qualification automation

- Accepted the supplied 0.0.28 player-facing Overhaul evidence, including availability gating, interruption safety, exact one-kit consumption, same-item Wrecked-to-Broken recovery, repeat-use rejection, Reload availability, and save/load persistence.
- Added the separate full-round personal extraordinary Repair Test Musket ability.
- Firearm Proficiency now grants Reload, Overhaul, and Repair together with missing-fact restoration enabled.
- Completed the staged same-item maintenance loop: Wrecked to Broken by Overhaul, Broken to Normal by Repair, then empty Normal to loaded Normal by Reload.
- Repair accepts exactly one equipped empty/Broken Test Musket and consumes exactly one Firearm Repair Kit only when delivery completes.
- Repair rejects Normal, Wrecked, loaded Broken, missing-kit, missing-inventory, and ambiguous-target cases before mutation.
- Added exact-item identity and one-revision verification plus independent state/inventory rollback for mutation-time failures.
- Added a deterministic two-item maintenance fixture, process-local baseline, and concise PASS/FAIL matrix for FixtureReady, OverhaulPassed, RepairPassed, and MaintenanceLoopPassed.
- Added a one-command immediate transaction regression runner while retaining focused manual action-bar interruption tests.
- Added 30 dependency-free tests, bringing the suite to 599.
- Retained the item-owned inert BlueprintWeaponEnchantment state carrier and did not revive the rejected ItemEntityWeapon.UniqueId vault.

## 0.0.28 — Sprint 28 player-facing same-item overhaul

- Added a stackable Firearm Repair Kit blueprint.
- Added the full-round personal extraordinary Overhaul Test Musket ability.
- Firearm Proficiency now grants Reload and Overhaul.
- Completed Overhaul consumes exactly one repair kit and changes the exact empty/Wrecked item to empty/Broken.
- Added atomic cross-resource rollback, exact-item identity/revision verification, readiness diagnostics, and repair-kit controls.
- Retained separate Broken-to-Normal repair, native Heavy Crossbow isolation, and the item-owned token carrier.
- Added 26 dependency-free tests, bringing the suite to 569.
- Added an accelerated Sprint 29–38 roadmap and feature-package cadence.

## 0.0.27-s27-item-lifecycle-recovery-contract

- Accepted the supplied 0.0.26 native-burst evidence and the user's explicit item-isolation confirmation for Sprint 27 entry.
- Preserved the Sprint 26 screenshots and recorded that the later disappearance of two Test Muskets was consistent with the destructive cleanup diagnostic rather than the explosion path.
- Inspected exact Kingmaker 2.1.7b item lifecycle IL: `ItemsCollection.Remove` safely detaches collection/equipment ownership; `ItemEntity.Dispose` only disposes enchantments; blueprint add and `ItemSwitch` replacement create new runtime items.
- Confirmed no installed item-condition `Repair`, `Mending`, `MakeWhole`, or `Make Whole` contract; `ItemRestoreValue` restores blueprint counts by adding items and is not same-item repair.
- Decided to retain exploded nonmagical firearms as exact empty/Wrecked items rather than automatically remove or replace them.
- Added the pure development-contract transition `OverhaulWrecked`, which accepts only Wrecked and returns empty/Broken.
- Added an exact equipped-item overhaul control that verifies unchanged repository identity and runtime reference, exactly one revision increment, empty final load, and Broken final condition.
- Kept ordinary Broken-to-Normal repair separate and deferred all player-facing cost, skill, timing, and action delivery.
- Replaced one-click removal of all unequipped Test Muskets with an arm/confirm/cancel safety flow.
- Added three dependency-free state-transition cases, raising the suite from 540 to 543 tests.
- Retained item-owned inert `BlueprintWeaponEnchantment` tokens and did not revive the rejected `ItemEntityWeapon.UniqueId` vault.
- Deferred automatic destruction, player-facing repair, Gunsmithing, Quick Clear, make whole, additional firearm types, scatter triple damage, and class progression.

## 0.0.26-s26-misfire-burst

- Accepted the supplied 0.0.25 runtime evidence: first misfire no explosion, condition-preserving Broken reload, second Broken-to-Wrecked misfire, exact-wielder Reflex DC 12 save, native half-damage, one applied event, empty/Wrecked final state, and zero relevant faults or duplicates.
- Added a validated `MisfireBurstRadiusFeet` field to immutable firearm definitions and their blueprint component round trip. The Test Musket declares a 5-foot burst.
- Inspected the exact Kingmaker 2.1.7b spatial contracts and bound the burst to `GameHelper.GetTargetsAround(Vector3, Feet, checkLOS: true, includeDead: false)`.
- Added deterministic reference-identity target planning: native-qualified nearby units are deduplicated and sorted by mechanics distance, stable unit identity, and display name; the exact wielder is inserted once and resolved last.
- Expanded the second-misfire consequence to create a fresh native Reflex DC 12 save and fresh native base weapon-damage bundle for every unique qualified unit.
- Added attack-level and per-unit duplicate gates, exact-item/repository/state checks, per-target evidence, query counters, target counters, and explicit partial-failure diagnostics.
- Added dependency-free validation for burst-radius invariants, target records, deterministic plans, reference deduplication, and per-target native-result evidence.
- Retained item-owned inert `BlueprintWeaponEnchantment` tokens and did not revive the rejected `ItemEntityWeapon.UniqueId` vault.
- Deferred scatter triple damage, firearm destruction, repair gameplay, additional firearm types, and class progression.

## 0.0.25-s25-second-misfire-explosion

- Accepted the supplied 0.0.24.1 Kingmaker evidence: Normal → Broken, condition-preserving Broken reload, Broken → Wrecked, Wrecked reload rejection, Wrecked attack rejection, and zero relevant runtime faults all passed.
- Recorded the Pathfinder early-firearm second-misfire consequence and the exact Kingmaker 2.1.7b save/damage contracts before implementation.
- Added a pure bounded explosion policy: only a detected Broken → Wrecked second misfire schedules damage; ordinary rolls and first misfires do not.
- After the exact firearm is committed empty/Wrecked, validate the correlated `RuleAttackRoll`, source `RuleAttackWithWeapon`, exact runtime item, exact current wielder, and repository identity.
- Resolve one native Reflex DC 12 save and one native non-critical, non-precision base weapon-damage event against only the exact current wielder. A passed save uses Kingmaker's native half-damage flag.
- Build one native base weapon-damage entry from the exact runtime firearm's current damage dice and blueprint damage type, avoiding target-specific data from the original attack while still using Kingmaker's native damage pipeline.
- Preserve at-most-once behavior per attack-roll object and add explicit scheduled, attempts, applied, not-required, rejected, duplicate, fault, save, damage, HP, and final-state diagnostics.
- Preserve the exact empty/Wrecked state even if native damage delivery faults; no broad retry or fallback is attempted.
- Keep native Heavy Crossbows, ordinary firearm attacks, first misfires, empty firearms, Wrecked firearms, and second blueprint-identical Test Muskets outside the consequence.
- Defer nearby-creature burst targeting, item destruction, repair gameplay, Quick Clear, automatic iterative reloads, Rapid Reload, additional firearm blueprints, and Gunslinger class progression.

## 0.0.24.1-s24-broken-reload-repair

- Evaluated the supplied 0.0.24 Kingmaker result and kept Sprint 25 blocked.
- Confirmed that the Normal → Broken misfire transition worked, but the stale Sprint 21 reload restriction made the required Broken → Wrecked test unreachable.
- Permitted an empty Broken exact Test Musket to pass both player-facing reload availability and the atomic reload transaction.
- Required every successful reload to preserve the firearm's existing Normal or Broken condition; reload cannot silently repair a Broken firearm.
- Retained Wrecked reload rejection before mutation.
- Preserved exact one-pair Black Powder Charge plus Lead Ball consumption, exact-item writes, rollback, state-token persistence, and the rejected `ItemEntityWeapon.UniqueId` vault boundary.
- Added regression coverage for empty/Broken success, loaded/Broken already-loaded rejection, and successful-result condition preservation.
- Added explicit numbered Kingmaker test instructions for every repair and carried-forward control.
- Added no repair gameplay, Quick Clear, explosion, splash damage, Rapid Reload, automatic iterative reload, new firearm, or Gunslinger class behavior.

## 0.0.24-s24-misfire-condition-transitions

- Entered Sprint 24 by explicit user-approved carry-forward: forced natural 1 and 2 misfires and ordinary 3/20 behavior were observed, while the remaining Sprint 23 isolation and persistence controls were intentionally folded into the combined 0.0.24 runtime gate.
- Added a pure deterministic condition policy that joins one natural-roll decision to an already-empty post-discharge firearm state.
- A detected misfire now transitions the exact discharged item from Normal to Broken or from Broken to Wrecked.
- Preserved one-round discharge ordering: the firearm is empty before condition damage, remains at `rounds=0`, and consumes no attack-time Black Powder Charge or Lead Ball.
- Committed condition damage through the existing item-owned inert `BlueprintWeaponEnchantment` token repository; the rejected `ItemEntityWeapon.UniqueId` vault remains unused.
- Added exact runtime-item and repository-identity verification before accepting a committed condition transition.
- Added per-attack duplicate-evaluation protection so one `RuleAttackRoll` object cannot apply condition damage more than once.
- Added diagnostics for `normalToBroken` and `brokenToWrecked`, including pre/post condition and complete token-backed state.
- Added twelve dependency-free condition-policy tests, raising the suite from 489 to 501 cases.
- Retained the deterministic natural 1/2/3/20 diagnostic and native Heavy Crossbow, empty-firearm, Wrecked-firearm, and no-natural-roll queue boundaries.
- Added no explosion, area or splash damage, repair gameplay, Quick Clear, iterative automatic reload, Rapid Reload, additional firearm content, or Gunslinger class behavior.

## 0.0.23-s23-natural-roll-misfire

- Accepted the complete 0.0.22.1 Kingmaker runtime gate: loaded/empty/Broken/Wrecked behavior, save/restart durability, token reconciliation, and native Heavy Crossbow isolation all passed without observed faults.
- Added exact final-natural-d20 observation for successfully discharged marked firearms through `RuleAttackRoll.Roll` assignment and `RuleAttackRoll.IsSuccessRoll(int)`.
- Added Test Musket misfire detection for natural 1-2; a misfire can only change Kingmaker's ordinary success result from hit to miss.
- Preserved the already-proven one-round discharge transaction: misfires consume the fired loaded round exactly once and do not consume additional inventory ammunition.
- Added a process-local deterministic force-next-roll diagnostic for natural 1, 2, 3, and 20, plus cancellation.
- Scoped forced-roll consumption to a successfully discharged exact firearm that actually reaches natural-d20 assignment; native Heavy Crossbows, empty firearms, Wrecked firearms, and attacks ending before a natural roll do not consume it.
- Added process-local misfire diagnostics for eligible attacks, observed rolls, ordinary results, misfires, forced rolls, duplicates, no-natural-roll completions, pending force state, and faults.
- Added exact reflection-contract tests for the private `set_Roll(RollEntry)` and public `IsSuccessRoll(int)` hooks, plus pure misfire and forced-queue tests, raising the dependency-free suite from 455 to 489 cases.
- Deliberately left firearm condition unchanged. Automatic Normal → Broken and Broken → Wrecked transitions remain bounded to Sprint 24.
- Added no explosions, area damage, repair gameplay, automatic iterative reloads, Rapid Reload, additional firearm content, or Gunslinger class behavior.

## 0.0.22.1-s22-attack-hook-repair

- Evaluated the supplied 0.0.22 Kingmaker result and kept Sprint 23 blocked.
- Corrected the Harmony target contract from an assumed zero-argument `OnTrigger()` to the exact installed `void OnTrigger(RulebookEventContext)` signature used by `RuleAttackRoll`, `RuleAttackWithWeapon`, and `RuleCalculateAC`.
- Added a fail-closed executable reflection predicate and nine regression cases covering the exact accepted callback shape and rejected alternatives.
- Limited native `ItemEntity.ApplyEnchantments()` firearm-state inspection to `ItemEntityWeapon`, preventing the observed `ItemEntityShield` reflection faults while retaining the exact-token restoration path.
- Corrected the Windows runtime-contract inspection script so future qualification checks enforce the installed one-argument callback contract.
- Preserved the item-owned inert `BlueprintWeaponEnchantment` state carrier, atomic reload transaction, loaded/empty/Broken/Wrecked discharge decisions, exact firearm marker, and native Heavy Crossbow isolation.
- Raised the dependency-free exact .NET Framework 4.7 suite from 446 to 455 cases.
- Added no natural-roll, forced-roll, misfire, condition-transition, explosion, automatic-reload, weapon-content, or Gunslinger-class behavior.

## 0.0.22-s22-loaded-round-enforcement

- Fixed equipped loaded-state loss during quicksave and native item-enchantment refresh.
- Added a parent `MechanicsContext` to new state-token enchantments whenever the exact item has a wielder or owner.
- Added a Harmony guard around `ItemEntity.ApplyEnchantments()` that verifies one known state token and restores the exact token if Kingmaker removes an older null-context token.
- Added loaded-round attack enforcement at the start of `RuleAttackRoll` for exact marked firearms.
- Loaded Normal and Broken firearms consume exactly one round; empty and Wrecked firearms are forced to miss.
- Cleared `AutoHit` when enforcing an empty-fire miss so auto-hit attacks cannot bypass the chamber state.
- Added weak reference-identity duplicate-event protection so one attack-roll object cannot consume more than one round.
- Kept inventory powder and Lead Ball counts unchanged when firing; components are consumed only during reload.
- Added process-local discharge and token-reconciliation diagnostics to the UMM panel.
- Added 27 discharge and reconciliation tests, raising the exact .NET Framework 4.7 suite from 419 to 446 cases.
- Kept misfires, explosions, iterative automatic reloads, Rapid Reload, models, vendors, crafting, and the Gunslinger class out of scope.

## 0.0.21-s21-reload-ability

- Activated the stable `KMG.Test.ReloadAbility` blueprint ID, raising the active custom-blueprint count from ten to eleven.
- Added a personal extraordinary `Reload Test Musket` ability configured as a full-round action.
- Granted and restored the reload ability through Firearm Proficiency, with an explicit development-time repair path for existing disposable saves.
- Added strict availability checks for exactly one equipped empty, undamaged Test Musket and one Black Powder Charge plus one Lead Ball.
- Connected the proven item-token firearm state and Sprint 20 shared-inventory components through a verified cross-resource transaction.
- Added best-effort rollback of both firearm state and ammunition, with independent state-rollback and inventory-rollback diagnostics.
- Added action-delivery diagnostics and an immediate transaction control to distinguish action-bar integration faults from transaction faults.
- Added 21 reload-specific tests, raising the dependency-free exact .NET Framework 4.7 suite from 398 to 419 cases.
- Kept attack-time loaded-round enforcement, firing consumption, iterative reloads, Rapid Reload, and misfires out of scope.

## 0.0.20-s20-basic-ammunition

- Accepted the Sprint 19 item-token carrier for continued development after the A-D state set survived save, full process exit, restart, and reload.
- Activated the reserved Black Powder Charge and Lead Ball blueprint IDs.
- Added isolated, stackable, component-free inventory item clones with custom localization, cost, and weight.
- Added an engine-independent two-component inventory boundary and exact count snapshots.
- Added verified all-or-nothing consumption of one powder charge plus one lead ball, with rollback and rollback-failure diagnostics.
- Added typed shared-inventory controls to add, count, consume, and remove ammunition.
- Added 25 ammunition tests, raising the suite from 373 to 398 cases.
- Kept the reload ability reserved and left Sprint 19's four core item-token carrier files byte-for-byte unchanged.

## 0.0.19-s19-proficiency-token-smoke

- Consumed the first real Kingmaker runtime evidence: blueprint bootstrap passed and the assumed item `UniqueId` contract failed.
- Fixed selected-unit resolution and Firearm Proficiency granting against the exact Kingmaker APIs.
- Moved development-command results to the top of the UMM panel.
- Rejected the identity-keyed UnitPart carrier and activated item-owned enchantment tokens as the persistence candidate.
- Added a token-based A-D save/restart fixture.
- Compiled against the user-exported Kingmaker 2.1.7b reference set and reran 373 tests three times with zero failures.

## 0.0.18-s18-runtime-smoke-candidate

- Compiled the full mod successfully against the user-provided Kingmaker 2.1.7b, Unity Mod Manager 0.32.4, Harmony 1.2.0.1, Unity, and Newtonsoft.Json assemblies.
- Fixed an unassigned blueprint lookup out parameter and two C# local-scope collisions exposed by the exact-reference compiler.
- Replaced the direct UnityEngine.IMGUIModule compile dependency with a fail-closed reflection adapter resolved from the running game.
- Matched Info.json ManagerVersion to the supplied Unity Mod Manager 0.32.4 runtime.
- Re-ran the 373-case .NET Framework 4.7 regression suite three times with zero failures.

## 0.0.17-s17-executed-evidence-handoff — 2026-07-13

### Added

- Exact .NET Framework 4.7 Roslyn compile and three-run executable evidence for the dependency-free harness.
- Two invalid-distance regression cases, bringing the suite to 373 tests.
- `scripts/export-private-build-references.ps1` for a narrow, private Kingmaker/UMM assembly handoff.
- `tools/run_exact_net47_domain_tests.py` and `tools/build_mod_from_private_references.py`.
- Executed test logs, machine-readable hashes, defect-discovery evidence, ADR-0024, and Sprint 18 criteria.

### Fixed

- Recursive `FirearmItemId` equality operators that caused stack overflow.
- Invalid `NaN`, infinite, and negative attack distances incorrectly collapsing to zero-range touch AC.
- Stale missing-state diagnostic wording.
- Stale duplicate-token exception expectation in the test harness.

### Preserved

- All twelve blueprint GUIDs and all eight active blueprint registrations.
- The Sprint 14 identity-vault persistence candidate and Sprint 15/16 evidence model.
- The NoGoIncomplete persistence decision.
- No ammunition, reload, class, vendor, or crafting additions.


## 0.0.16-s16-runtime-qualification — 2026-07-13

### Added

- Pure trusted runtime-preflight model and evaluator for persistence rows I01 and I02.
- Kingmaker runtime probe for exactly one blueprint initialization, eight custom registrations, and the inherited `ItemEntityWeapon.UniqueId` contract.
- Evidence-recorder command that can automatically append only trusted I01/I02 observations.
- Deterministic A-D Test Musket fixture for I03, including strict identity, state, and record verification.
- One-command Windows qualification workflow for source validation, explicit path validation, fingerprinting, runtime-contract inspection, C# test execution, Release compilation, UMM packaging, hashing, and qualification reporting.
- Twenty preflight test declarations, bringing the total to 371.
- Runtime-qualification documentation, ADR-0023, and Sprint 17 branch criteria.

### Changed

- Persistence evidence snapshots now accept only the strict engine-issued `UniqueId` used by the identity-vault carrier; diagnostic fallback identities are rejected.
- Runtime-contract metadata and assembly versions advanced to Sprint 16.
- Sprint 15 report and Sprint 16 entry criteria moved to history.

### Not added

- No ammunition items, reload action, inventory consumption, attack consumption, new save carrier, or new blueprint GUID.
- No compiled DLL or UMM install archive produced in this environment.

### Validation status

- Portable source, syntax, invariant, documentation, packaging, and independent-model validation complete.
- Main Kingmaker compilation and the in-game lifecycle matrix remain unperformed here.
- Persistence gate remains NO-GO / incomplete.

## 0.0.15-s15-persistence-evidence — 2026-07-13

### Added

- Immutable 35-row persistence lifecycle catalog with 30 Critical and 5 High-severity rows.
- Deterministic gate evaluator with PASS, FAIL, BLOCKED, and two-run reproduction requirements.
- Build-fingerprinted external JSON evidence sessions and generated Markdown reports.
- Structured BEFORE/AFTER snapshots for visible firearms, vault records, repository counters, and migrations.
- UMM controls for evidence sessions, matrix navigation, reproduction runs, notes, hashes, captures, outcomes, and export.
- Atomic UTF-8 evidence writes under the installed mod's `evidence/` directory.
- Portable .NET 8 domain-test runner that reuses the classic test project's explicit source list.
- Twenty-four evidence-domain test declarations, bringing the total to 351.
- ADR-0022, recorder documentation, and Sprint 16 branch criteria.

### Changed

- Sprint 14's engine-item-identity UnitPart vault remains the sole new-write persistence candidate.
- Persistence gate evaluation is now mechanically reproducible instead of relying on hand-copied observations.
- Evidence sessions resume only for an exact compiled-build and game fingerprint match.
- Sprint 14 report and Sprint 15 entry criteria moved to history.

### Not added

- No ammunition item blueprints, reload actions, inventory transactions, or attack consumption.
- No new persistence carrier and no new blueprint GUID.
- No compiled DLL or Unity Mod Manager install archive.

### Validation status

- Portable source, catalog, evaluator, packaging, and independent model validation complete.
- C# test execution, Kingmaker compilation, UMM rendering, evidence-file I/O, and lifecycle observations remain unperformed here.
- Persistence gate remains NO-GO / incomplete.

## 0.0.14-s14-item-identity-vault — 2026-07-13

### Added

- Immutable `FirearmItemId` value object using canonical nonempty GUID D-form values.
- Strict `IFirearmItemIdentityProvider` and Kingmaker adapter that read only `ItemEntityWeapon.UniqueId` and accept only `System.Guid` or `System.String`.
- Primitive identity-keyed UnitPart records containing no runtime item reference.
- `IdentityBackedFirearmStateVaultStore` and identity-aware repository reconstruction behavior.
- One-way migration from Sprint 13 direct-reference records, with unresolved evidence preservation, conflict rejection, verification, and rollback.
- Separate diagnostics for Sprint 13 reference migration and Sprint 12 token migration.
- Development-only Sprint 13 direct-reference migration fixture.
- Installed-assembly inspection for the exact inherited `UniqueId` member and supported runtime type.
- Thirty-five dependency-free identity and migration cases, bringing the declared total to 327.
- Engine-item-identity documentation, ADR-0021, revised lifecycle matrix, and Sprint 15 branch criteria.

### Changed

- New firearm-state writes now use an engine-item-identity-keyed UnitPart record.
- Sprint 13 direct item references remain serialized only as one-way migration inputs.
- Sprint 12 state-token enchantments remain registered only as an older migration input.
- Missing, malformed, empty, or unsupported engine identity now blocks persistence access rather than inventing an ID or returning implicit empty state.
- Blueprint manifest remains unchanged at 12 stable IDs and 8 active entries.
- Ammunition and reload work remains blocked pending a compiled lifecycle GO decision.

### Validation status

- Portable source and independent identity/migration-model validation complete.
- No compiled UMM package produced.
- Installed `UniqueId` shape, item-identity lifecycle semantics, custom UnitPart serialization, and legacy migration remain unproven in Kingmaker.
- Architecture gate remains NO-GO.

## 0.0.13-s13-unitpart-vault — 2026-07-13

### Added

- Save-owned `UnitPartFirearmStateVault` attached to the main-character save graph.
- Direct `ItemEntityWeapon` references plus primitive `FirearmStateData` records.
- Expected-current vault replacement with defensive copying and verification.
- `VaultBackedFirearmStateRepository` preserving the existing repository contract.
- `MigratingFirearmStateRepository` for one-way verified migration from all four Sprint 12 tokens.
- Fail-closed equivalent-state cleanup, conflict preservation, invalid-token handling, and rollback-failure diagnostics.
- Development-only legacy-token migration fixture.
- Installed-assembly contract inspection for UnitPart access, `Get<T>()`, `Ensure<T>()`, main-character resolution, and Json.NET attributes.
- Fifty-three dependency-free C# cases, bringing the declared total to 292.
- UnitPart-vault documentation, ADR-0020, revised lifecycle matrix, and Sprint 14 gate.

### Changed

- New firearm-state writes now target only the save-owned vault.
- The four Sprint 12 token blueprints remain registered solely for old-save migration.
- Process-local weak metadata remains diagnostic only.
- Blueprint manifest remains unchanged at 12 stable IDs and 8 active entries.
- Sprint 14 ammunition and reload work is explicitly blocked pending runtime persistence proof.

### Validation status

- Source and independent-model validation complete.
- No compiled UMM package produced.
- UnitPart serialization, direct item-reference restoration, merchants, respec, deletion, and migration remain unproven in Kingmaker.
- Architecture gate remains NO-GO.

## 0.0.12-s12-persistence-spike — 2026-07-13

### Added

- Strict finite firearm-state token definitions and catalog.
- Four component-only no-op `BlueprintWeaponEnchantment` state tokens.
- Token-backed per-item repository whose source of truth is the exact gun's token.
- Reflection-contained Kingmaker item-enchantment read/add/remove adapter.
- Expected-current validation, post-write verification, and best-effort rollback.
- Installed-assembly contract inspection for enchantment collections and methods.
- Full save/load, process restart, inventory, merchant, deletion, migration, and presentation test matrix.
- 52 dependency-free C# cases, bringing the declared total to 239.

### Changed

- `FirearmRuntimeState` now composes the token-backed repository instead of the Sprint 11 weak repository.
- The weak table retains only process-local diagnostics and revisions.
- Blueprint manifest expanded from 9 to 12 stable IDs and from 4 to 8 active entries.
- Sprint 13 feature work is explicitly blocked pending runtime persistence proof.

### Validation status

- Source and independent-model validation complete.
- No compiled UMM package produced.
- Save/load durability unproven.
- Architecture gate remains NO-GO.

## Sprint 11 — 2026-07-13 — `0.0.11-s11-runtime-item-state`

### Added

- `IFirearmStateRepository` process-local state boundary.
- `WeakFirearmStateRepository` keyed by exact runtime object reference through `ConditionalWeakTable`.
- Per-entry immutable state, revision, counters, and process-local diagnostic identity.
- `FirearmItemStateService` and strict `ItemEntityWeapon` resolver that reject native Heavy Crossbows and ambiguous markers before repository creation.
- Immutable item-state diagnostic snapshots that retain no runtime game object.
- UMM development controls for visible-state inspection, two-musket isolation, and debug load/damage/repair/reset transitions.
- Runtime-contract inspection for `ItemEntityWeapon`, item blueprint access, and candidate runtime IDs.
- Thirty-two repository/service tests, bringing the dependency-free harness to 187 declared cases.
- Runtime item-state documentation, ADR-0018, and Sprint 12 persistence-spike criteria.

### Changed

- Version advanced to `0.0.11-s11-runtime-item-state` without changing any blueprint GUID or active blueprint count.
- Removal of Test Muskets now explicitly forgets process-local state for the exact removed item when resolvable.
- Development diagnostics now report repository identity, revision, runtime metadata, immutable state, and repository counters.
- Current architecture documents distinguish process-local association from save persistence.

### Explicitly not included

- Compiled DLL or UMM install ZIP.
- Save serialization, process-restart identity, inventory ammunition, reload actions, empty-fire interception, shot consumption, attack-time misfire interception, or explosions.
- Any claim that equip, transfer, or save/load preserves the same Kingmaker item object until tested in the running game.
- Any claim that the 187 C# tests were compiled or executed in this environment.

## Sprint 10 — 2026-07-13 — `0.0.10-s10-firearm-state`

### Added

- Stable `AmmunitionId` value object with strict serializer-safe syntax and ordinal equality.
- Immutable `FirearmStateRules` for capacity and compatible ammunition inputs.
- Immutable `FirearmState` schema containing loaded rounds, ammunition identity, and Normal/Broken/Wrecked condition.
- Pure load, fire, misfire-damage, repair, and wreck transitions with typed rejection reasons.
- Primitive-only `FirearmStateData` DTO and strict codec without selecting a Kingmaker persistence mechanism.
- Sixty-one state tests, bringing the dependency-free harness to 155 declared cases.
- Firearm-state contract documentation, ADR-0017, and Sprint 11 runtime item-association criteria.

### Changed

- Version advanced to `0.0.10-s10-firearm-state` without changing any blueprint GUID or active blueprint count.
- Project and test compile declarations now include the pure state files.
- Current architecture documents distinguish pure state, runtime item association, and save persistence as separate gates.

### Explicitly not included

- Compiled DLL or UMM install ZIP.
- Association of state with `ItemEntityWeapon` or any character buff.
- Save persistence, inventory ammunition consumption, reload action, empty-fire interception, misfire-roll interception, or explosion damage.
- Any claim that the 155 C# tests were compiled or executed in this environment.


## Sprint 9 — 2026-07-13 — `0.0.9-s09-touch-ac`

### Added

- The first gameplay-changing firearm rule: exact early firearms target touch AC inside their first firearm range increment and ordinary AC beyond it.
- A pure, game-object-free armor-class selector and strict Kingmaker reflection adapter.
- Context-preserving AC selection using `current TargetAC + (touch AC - ordinary AC)`, retaining rule-event changes such as cover and flat-footed adjustments.
- A 0.1-millimeter boundary tolerance to prevent floating-point noise from moving an exact-range shot into the next increment.
- A short-lived marker-scoped `RuleAttackRoll` context for nested `RuleCalculateAC` events.
- Weak per-event duplicate protection, duplicate counters, and optional `ac.touch-selected`, `ac.ordinary-selected`, and `ac.duplicate-skipped` log events.
- Runtime-contract inspection for participants, `DistanceTo`, ordinary/touch AC, and one writable Int32 `TargetAC` member.
- Twenty-one AC selection and strict-access tests, bringing the dependency-free harness to 94 declared tests.
- ADR-0016, the range-limited touch-AC contract, and Sprint 10 state-machine entry criteria.

### Changed

- Version advanced to `0.0.9-s09-touch-ac` without changing any blueprint GUID or active blueprint count.
- The `RuleCalculateAC` postfix now applies the firearm AC delta before the optional after-trace captures the final selected AC.
- Combat tracing remains optional; touch-AC behavior is active independently of the trace toggle.
- The development panel now reports touch, ordinary, duplicate, and fault counters.

### Explicitly not included

- Compiled DLL or UMM install ZIP.
- Ammunition, reload, empty-fire restrictions, misfire, mutable item state, class progression, vendors, or assets.
- Any claim that the reflection contracts or callback nesting have been confirmed in a running Kingmaker installation.

## Sprint 8 — 2026-07-12 — `0.0.8-s08-combat-tracing`

### Added

- Disabled-by-default, read-only firearm combat tracing for `RuleAttackWithWeapon`, `RuleAttackRoll`, and `RuleCalculateAC`.
- Dynamic Harmony 1.2 patch-target resolution with fail-closed `Prepare()`/`TargetMethod()` behavior.
- Exact firearm identification from one `FirearmDefinitionComponent` on the concrete weapon type; native Heavy Crossbows remain excluded.
- Immutable event snapshots and a game-independent correlation engine for nested attack, attack-roll, and AC callbacks.
- Deterministic single-line log records for trace start, observations, completion, duplicate callbacks, range increment, AC candidates, roll candidates, and command shape.
- A non-persistent UMM toggle for verbose trace output plus diagnostic counters.
- Runtime-contract inspection for the three rule-event types, declared `OnTrigger()` methods, candidate data members, and `UnitEntityData.DistanceTo`.
- Twenty-three range/correlation/formatting test cases, bringing the dependency-free harness to 73 declared tests.
- Combat trace schema, ADR-0015, and Sprint 9 touch-AC entry criteria.

### Changed

- Version advanced to `0.0.8-s08-combat-tracing` without changing any blueprint GUID or active blueprint count.
- The development panel now reports trace status, completed traces, and contained trace faults.
- Runtime diagnostics retain only strings, primitive values, and integer event identities; no Kingmaker or Unity object is retained after a callback.

### Explicitly not included

- Compiled DLL or UMM install ZIP.
- Touch-AC mutation, ammunition, reload, misfire, mutable item state, class progression, vendors, or assets.
- Any claim that the candidate event members or callback order have been confirmed in a running Kingmaker installation.

## Sprint 7 — 2026-07-12 — `0.0.7-s07-proficiency-controls`

### Added

- Dedicated hidden Firearm Proficiency `BlueprintFeature`.
- Item-level `FirearmProficiencyRestriction` derived from Kingmaker's `EquipmentRestriction`.
- Strict equip denial for units that do not possess the dedicated proficiency feature.
- One-transaction registration of four active custom blueprints.
- Manual Unity Mod Manager controls to grant proficiency, add/remove Test Muskets, and inspect equipped firearm definitions.
- Guarded reflection adapter for campaign selection, feature grants, shared inventory, and equipment inspection.
- Runtime-contract inspection for the proficiency restriction, `UnitDescriptor.GetFeature`, `Kingmaker.Game`, and UMM `OnGUI`.
- Ten reflection-helper test cases, bringing the dependency-free harness to 50 tests.
- Sprint 7 architecture, test, known-issue, decision, and Sprint 8 planning documents.

### Changed

- Activated the previously reserved Firearm Proficiency GUID without changing its value.
- Added the proficiency restriction only to the custom Test Musket item; native Heavy Crossbow assets remain untouched.
- Expanded initialization from three to four owned registrations and retained reverse rollback.
- Moved completed Sprint 6 and Sprint 7 planning material into documentation history.

### Explicitly not included

- Compiled DLL or UMM install ZIP.
- Touch AC, combat instrumentation, ammunition, reload, misfire, mutable item state, class progression, vendors, or assets.

## Sprint 6 — 2026-07-12 — `0.0.6-s06-test-musket`

### Added

- Native Heavy Crossbow type/item lookup with exact runtime-type validation.
- Clone-only Test Musket weapon type and item registration.
- Canonical `FirearmDefinitions.CreateEarlyMusket()` factory.
- Exactly one named `FirearmDefinitionComponent` on the custom weapon type.
- Reflection-validated `BlueprintItemWeapon` to `BlueprintWeaponType` adapter.
- Transaction-wide reverse rollback for all custom blueprint registrations.
- Source immutability checks for native Heavy Crossbow blueprints.
- Two additional domain tests, for 40 total.
- Sprint 6 runtime-contract inspection, documentation, and Sprint 7 entry criteria.

### Changed

- Activated the reserved Test Musket type and item GUIDs without changing any GUID value.
- Expanded one-time bootstrap completion from one to three custom blueprints.

### Explicitly not included

- Compiled DLL or UMM install ZIP.
- Player acquisition, firearm proficiency, touch AC, ammunition, reload, misfire, or mutable item state.

## Sprint 5 — 2026-07-12 — `0.0.5-s05-firearm-domain`

### Added

- Immutable firearm era, kind, reload profile, and definition domain types.
- Validation for all initial numeric, enum, scatter, kind/era, capacity, and base-reload invariants.
- Value equality, deterministic hash codes, equality operators, and invariant-culture diagnostics.
- Passive serialized `FirearmDefinitionComponent` deriving from Kingmaker's `BlueprintComponent`.
- Dependency-free .NET Framework 4.7 domain test project with 38 named cases.
- `scripts/test-domain.ps1` and automatic domain-test execution before full builds.
- Firearm definition contract, ADR-0012, Sprint 6 criteria, and a detailed Kingmaker smoke-test guide.
- Runtime contract inspection for the `BlueprintComponent` base type.
- One-time in-memory `FirearmDefinitionComponent` construction/read-back probe with a single `firearms/domain.ready` log event.

### Preserved

- All nine blueprint GUIDs and exactly one active hidden diagnostic feature.
- Exactly one Harmony instance creation and one `PatchAll`.
- Collision-safe registration, rollback, and strict manifest validation.
- No copied third-party source or redistributed proprietary binaries.

### Not added

- No firearm blueprint, item, proficiency, acquisition route, combat rule, ammunition, per-item state, class, asset, or UI.
- No compiled DLL or install ZIP in this environment.

## Sprint 4 — 2026-07-12 — `0.0.4-s04-diagnostic-blueprint`

### Added

- Strict runtime loading of the deployed blueprint ID manifest from the installed mod directory.
- Immutable `BlueprintId` validation with no runtime generation API.
- Rejection of unknown JSON members, malformed IDs, duplicate symbols, duplicate GUIDs, inactive registrations, and planned-type mismatches.
- Collision-safe `BlueprintRegistry` using pre-factory checks, dictionary `Add`, verification, and rollback.
- One hidden, one-rank, component-free diagnostic `BlueprintFeature`.
- Registry verification that the exact diagnostic instance was inserted into Kingmaker's live GUID dictionary.
- Expanded runtime reflection report for `m_AssetGuid`, `BlueprintsByAssetId`, `GetAllBlueprints`, `ComponentsArray`, `HideInUI`, and `Ranks`.
- Portable Sprint 4 validator with nine modeled manifest and transaction scenarios.
- Blueprint manifest architecture guide and Sprint 5 entry criteria.

### Changed

- `KMG.Diagnostic.InitializedFeature` moved from `reserved` to `active`; its GUID is unchanged.
- Blueprint lifecycle completion now reports one diagnostic registration rather than zero content.
- Version advanced from `0.0.3` to `0.0.4`.

### Preserved

- Exactly one Harmony instance creation and one assembly-wide `PatchAll`.
- All nine stable GUID values.
- Non-copying external references and proprietary-binary exclusion.
- No runtime dependency on Call of the Wild or Cowboys and Demons.

### Not added

- No firearm, weapon, proficiency, class, feat, ability, combat rule, inventory item, setting, model, animation, or persistent state.
- No compiled DLL or install ZIP in this milestone environment.

## Sprint 3 — 2026-07-12 — `0.0.3-s03-bootstrap`

### Added

- Process-lifetime UMM loader state guard with duplicate and failure handling.
- Structured, non-throwing UMM log adapter carrying mod ID and informational version.
- Published mod context owning the executing assembly and one Harmony12 instance.
- Exactly one `HarmonyInstance.Create` and one `PatchAll` call.
- Zero-argument `LibraryScriptableObject.LoadDictionary` postfix.
- One-time blueprint lifecycle coordinator with pending-observation support.
- Fail-closed behavior for invalid libraries, patch failures, and initialization failures.
- Portable Sprint 3 source validator and six modeled lifecycle scenarios.
- Runtime contract-reflection script and detailed local acceptance log matrix.
- Sprint 4 entry criteria for manifest loading and one diagnostic blueprint.

### Preserved

- All nine stable blueprint GUID reservations.
- Non-copying external-reference and package allowlist policies.
- No runtime dependency on either reference gameplay mod.

### Not added

- No custom blueprint or manifest parsing at runtime; scheduled for Sprint 4.
- No firearm, class, proficiency, rule handler, setting, save state, or asset.
- No unload/live-toggle behavior.
- No compiled DLL or install ZIP in this milestone environment.

## Sprint 2 — 2026-07-12 — `0.0.2-s02-scaffold`

### Added

- Visual Studio solution and classic .NET Framework 4.7 C# project.
- C# 7.3, AnyCPU, `Prefer32Bit=false`, deterministic-build, and warning policy.
- Ignored local `GamePath.props` convention plus a validated creation script.
- Non-copying references to the initial Kingmaker, Unity, UMM, Harmony12, and Newtonsoft.Json assembly set.
- Pre-build errors for absent game paths or required assemblies.
- Post-build errors for accidental external-DLL copying.
- Unity Mod Manager `Info.json` and a harmless loader stub.
- Reproducible build, source-package, install-package, output-validation, package-validation, and environment-fingerprint scripts.
- Blueprint manifest JSON Schema and explicit copied-content deployment decision.
- Portable standard-library Python scaffold validator.
- Sprint 3 entry criteria.
- MIT license for original project source and documentation.

### Preserved

- All nine blueprint GUID reservations from Sprint 1 remain unchanged.
- The firearm marker, real-weapon attack pipeline, and item-owned persistence decisions remain controlling.

### Not added

- No Harmony patches or blueprint lifecycle hook; scheduled for Sprint 3.
- No diagnostic blueprint; scheduled for Sprint 4.
- No firearm, class, save state, custom art, or runtime settings.
- No compiled DLL or install ZIP in this milestone environment.

## Sprint 1 — 2026-07-12 — `0.0.1-s01-architecture`

- Established the target runtime, architecture, stable-ID policy, reference audit, blueprint discovery plan, persistence candidates, and risk gates.
## 0.0.67 - Seventh-playtest player-path repair

- Prevented ordinary Blunderbuss attacks from consuming a chamber; Scatter Shot is now the sole guarded 15-foot cone firing path.
- Moved Dodge and Deadeye spending and buff application onto Kingmaker's native ability resource/action pipeline.
- Made basic ammunition crafting immediate while preserving its atomic cost, output, tool, and once-per-rest rules.
- Resolved Reload Firearm to a fixed native action variant before command construction so matching Rapid Reload choices govern actual action consumption.
- Replaced generic firearm mesh-axis inference with explicit per-model transforms and restored the stable crossbow animation contract without inherited crossbow renderers.
- Added listener and playback diagnostics and a persistent 2D firearm-audio fallback for Kingmaker's active listener configuration.
- Restored clickable Reload Firearm by preserving the granted parent ability through native command construction; action economy now changes only the final command-type argument.
- Added the native True-Grit-aware Dodge resource cost calculator and removed manual component invocation from command-path acceptance.
- Preserved the accepted drawn-Pistol wrapper, added separate Pistol/Musket/Blunderbuss belt/back wrappers, and recalibrated Musket and Blunderbuss equipped transforms.
