# Changelog

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
