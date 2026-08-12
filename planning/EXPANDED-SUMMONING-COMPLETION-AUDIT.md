# Expanded Summoning completion audit

Audit date: 2026-08-12

Status: in progress until the final artifact/hash freeze and PR refresh are
complete. This document maps the complete durable mission to current
authoritative evidence. A PASS here means the cited evidence covers the full
requirement; it is not inferred merely from a green build.

## Evidence anchors

- Selected baseline: `origin/master` at
  `2894d9fcce250708e354894ffd8e1be9c7493b9b`, containing required ancestor
  `e4d560f8dd2909518614e3a20e77ba4d70dadeb8`.
- Final native qualification source:
  `5205805eab3fe0115d6888c53bce73c80474d1b7`. Commits after this SHA through
  the current audit checkpoint change only checked-in documentation and the
  development-time roster evidence header; no runtime, test, blueprint,
  manifest, settings, or asset file differs.
- Structural run:
  `20260812T1327062696968Z-bd09acfba08942df8f7c42e5c70252f4`, PASS 31/31.
- Mechanical run:
  `20260812T1330147883834Z-ec8896f1d65b43e0913a6bea7cba4405`, PASS 12/12
  aggregate assertions and 153/153 production `UnitUseAbility` commands.
- Visual run:
  `20260812T1151394827201Z-add45a04f5de44c1a39e3251f7ff0778`, PASS 10/10
  for 67/67 unique unit views.
- Enabled persistence sequence:
  `20260812T1155220523013Z-6d2a18f9b33344d08d3127ffce7e5cb6`,
  `20260812T1158042311459Z-f21e3713df3b4545add5e7ab3436e865`, and
  `20260812T1200508004220Z-02af221881da449f827cf5a048ba6c67`.
- Disabled persistence sequence:
  `20260812T1203346138814Z-0c81aa138d344d8e867d84d0ec316564`,
  `20260812T1206142978812Z-a085c35bc5174b9d9bdd2633826436d3`, and
  `20260812T1208449380302Z-65c9b7056d97483fb48a4a9b76c22ea6`.
- Module matrix: 16/16 fresh launches PASS, first result
  `20260812T1211226889431Z`, last result `20260812T1241396573801Z`.
- Acadamae run:
  `20260812T0759568976282Z-be2dea5c910c4c4780d17ce11de1aee4`, PASS 13/13,
  using the concrete KMG native-preservation child selected by
  `PrepareAcadamaeSpell`.
- Final compatibility transactions: standalone
  `compat-20260812T133252Z-8ab70bfdbf75` and
  `compat-20260812T133451Z-f7096e45017c`; Call of the Wild
  `compat-20260812T133727Z-74e8798f7849` and
  `compat-20260812T134033Z-433e1ea1a746`; highest-risk combined
  `compat-20260812T134340Z-adb4d15f893d` and
  `compat-20260812T134649Z-1b83742188f4`; Arms and Armor
  `compat-20260812T125302Z-b93f3cd123d7`; Toggle Custom Soundpacks
  `compat-20260812T125454Z-1d1dbe9924c8`. All PASS and restored.

Exact machine-local result paths, hashes, and restoration fingerprints are in
`EXPANDED-SUMMONING-STATE.json`; curated interpretation is in
`EXPANDED-SUMMONING-IMPLEMENTATION-REPORT.md` and the fidelity matrix.

## Sections 1-3: baseline, persistence, and objective

| Requirement | Evidence | Audit |
|---|---|---|
| Correct repository, remote, baseline, and required ancestor | Preflight is journaled; Git ancestry proves the selected baseline and required ancestor. Branch is `codex/expanded-summoning`; history has no merge commit and local/origin equality is checked after every push. | PASS |
| Preserve unrelated work and avoid destructive Git operations | Dedicated branch/worktree history; mandated push helper logs ordinary non-force pushes; no reset/clean/rebase/force-push or release-branch merge. | PASS |
| Shield Other established range prerequisite | Separate commits `f0a103c` and `0617402`; `ShieldOtherPolicyTests` prove link validity is distance-independent while casting remains close-range and endpoint/area/lifecycle rules remain. Final 1009-test regression suite PASS. | PASS |
| Durable mission/state/roster/fidelity/journal/report/resume/blockers artifacts | Every named artifact exists. The durable mission now contains all 22 sections rather than a synopsis. `AUTONOMOUS-BLOCKERS.md` retains historical blockers and states no active Expanded Summoning blocker. | PASS |
| Independent, default-on, restart-bound fourth module | Settings schema 3, module catalog, publication plan, UI, migration, active/pending snapshot tests, and 16/16 fresh-process matrix. | PASS |
| Disabled behavior | Matrix and disabled persistence prove zero new KMG parent references, all 1,155 identities registered, active summons load-safe, and other modules independent. | PASS |
| Frozen counts | Generator, domain suite, manifest, structural observer, and state agree: 66 SM, 57 SNA, 67 unique creatures, 361/320/681 placements. | PASS |
| Explicit non-goals/exclusions | Catalog validator and generated roster include only approved entries; sanitizer and package/source review show no companions, vendors, encounters, external assets, dependencies, or unrelated spell-system redesign. Explicit exclusions are documented. | PASS |

## Sections 4-6: rules and approved rosters

| Requirement | Evidence | Audit |
|---|---|---|
| Native summon spell contract | Structural assertion covers all 681 roots and 1,045 generated nodes: Conjuration, Summoning descriptor, native full-round casting, close range, point targeting, material object, metamagic, action-bar, and exact parent mapping. | PASS |
| Duration, faction, control, and cleanup | All 153 casts received the exact native `SummonedUnitBuff` with caster context and CL20 bounded duration. Persistence proves fresh-process context, remaining duration, shared area state, commands/view/faction, exact native-buff removal, destruction/detachment, and zero remaining live references. | PASS |
| Area transition, rest, caster death, and RTwP/turn-based safety | KMG adds no lifecycle controller, mode branch, global summon patch, event subscription, or persistent unit handler. Every child reuses the exact Owlcat spawn/pool/duration graph and every unit uses the native summon faction/buff; therefore these boundaries remain the native summon lifecycle rather than a KMG approximation. Save/restart and explicit expiration are exercised directly. | PASS (native-equivalence boundary) |
| Prohibited abilities and side effects | Sanitizer policy/domain tests plus structural assertions prove zero prohibited references, inherited class spells, starting inventory, donor component aliases, or non-KMG action contamination. Exact per-cast area/global/party snapshots reject leaked units, empty loot entities, or inventory effects. | PASS |
| Quantity formula and same-kind packs | Pure matrix tests cover every tier and dice boundary; runtime covers 16/16 family-tier 1d3 and 14/14 family-tier 1d4+1 cases, legal count bounds, and exact unit-reference same-kind identity. | PASS |
| SM templates and descriptors | 182 logical choices, 182 celestial executions, 182 fiendish executions, six HD-banded buffs, two smite markers; structural and good/neutral/evil runtime assertions prove alignment, masks, descriptors, replacement of native default template, and one bounded smite. | PASS |
| SNA caster alignment and no templates | Catalog and identity tests prohibit celestial/fiendish SNA execution IDs; structural/runtime assertions cover all 320 SNA nodes and exact caster alignment. | PASS |
| Complete approved rosters and native equivalence handling | Generated roster and catalog tests enumerate every approved tier row and reject duplicate family/tier/key entries. Publisher starts from final-live arrays and GUID-deduplicates; native tier-I graphs are preserved as first castable variants. | PASS |

## Sections 7-10: inventory, architecture, identities, sanitization

| Requirement | Evidence | Audit |
|---|---|---|
| Exact SM/SNA parent inventory | Checked-in inventory records all 18 canonical parents and GUIDs, direct/variant structure, native child action graphs, spell structure, pools, duration, descriptors, and optional-parent hazards. Final structural run validates all required parents. | PASS |
| Donor inventory and selection | Donor catalog plus generated roster records every creature's exact selected GUID/name/view policy. The 55-GUID final-live donor observer records unit fields and bounded component/body/view graphs; inventory/fidelity rows record scale/size/reach/movement/attacks/abilities and sanitization decisions. Visual run validates instantiated results. | PASS |
| Native templates, quantities, feats, cleanup, and Acadamae inventory | Inventory observer records exact native action/template graphs, native pool and lifecycle, Augment, CotW Superior, and Acadamae classification. Its normalized `Sacred Summons`/`SacredSummons` search returned zero candidates in the installed CotW/high-risk graph, so Sacred Summons was not structurally available and was correctly skipped rather than guessed. | PASS |
| Data-driven architecture | Immutable catalogs/specifications, pure policies, builders, publication transaction, sanitizer, observer, and optional reconciler are separate; no one-off runtime ID generation. Runtime and domain project files explicitly enumerate sources. | PASS |
| Development-time manifest generation | `tools/expanded_summoning_manifest.py` deterministically validates/emits the roster and 1,155 planned identities, rejects collisions/stale output, and allocates only under an explicit development flag. Runtime consumes frozen manifest IDs. | PASS |
| Identity/registration contract | 1,155 feature identities, 1,409 active repository entries, one reserved, 1,410 ledger total, exact planned types/lowercase GUIDs, and constant 1,409 runtime registration in all 16 states. Loaded-library/feature collision assertions PASS. | PASS |
| Safe clone isolation and sanitizer | Deep-cloned mutable component/array graphs; donor reference comparisons report zero aliases/mutation. Runtime reports zero prohibited references, class spells, inventory, and contaminated native actions. All 67 units carry one extraplanar marker and summon faction. | PASS |
| Tabletop fidelity profiles | The fidelity matrix and immutable natural/special profiles cover every requested statistical category and explicit deviation. Domain and structural tests validate tier groups and special profiles. | PASS |

## Sections 11-14: adaptations, publication, modules, and feats

| Requirement | Evidence | Audit |
|---|---|---|
| Lantern Archon | Dual two-projectile ranged-touch light ray, archon defenses, optional exact Aura carrier, no teleport; Wisp visual only. Structural, mechanical, projectile/visual tests PASS. | PASS with documented aura/teleport deviations |
| Elementals and mephits | All 24 elemental tiers and four mephits use exact dedicated donors with rebuilt/validated tier statistics and traits. Four breath types and bounded mechanics are structurally covered; Fire Mephit breath damaged a runtime target. | PASS with documented unconditional Fast Healing 2 adaptation |
| Salamander, Invisible Stalker, Shadow Demon, Succubus | Exact special builders and profiles; runtime attacks and key bounded effects pass. No campaign inventory, teleport, summon, permanent drain/gift, or unsafe possession. | PASS with documented conservative omissions |
| Bebelith and Pixie | Custom units, items/resources/brains/abilities and bounded state buffs; runtime armor remains intact while one-round dismantle applies, dance/sleep consume bounded resources, visual/nav/projectile/death cleanup passes. | PASS with documented conservative adaptations |
| Natural proxies | 26 rebuilt natural profiles use donors only for view/rig, a bounded natural-attack brain, project-owned weapon profiles, intended stats/facts, and no donor campaign behavior. Structural tier assertions, attacks, and 67-unit visual matrix PASS. | PASS with row-specific deviations |
| Ability preservation | All nodes match exact native parent/child contract, including non-null material data, parent spell context, metamagic, UI, duration, and pools. Tier-I native graphs remain first and castable. | PASS |
| Additive transactional merge | Domain tests prove reference/order preservation, deterministic append order, GUID/reference dedupe, idempotence, malformed target rejection, exact rollback, and refusal after unrelated mutation. Final-live assertion proves no preexisting variant changed or disappeared. | PASS |
| Optional-parent reconciliation | Exact structural signatures, one-match rule, additive preservation, idempotence, and ambiguity fail-closed tests; no optional compile references. CotW and combined profiles PASS. | PASS |
| Module isolation and migration | Schema 0/1/2 migration, malformed quarantine, atomic write, equality/hash/string/state/UI, active/pending restart semantics, publication plans, and all 16 states are tested. Settings restored to original SHA-256. | PASS |
| Acadamae Graduate | All 1,045 generated nodes structurally classify as native summons. Dedicated 13/13 runtime uses a concrete `KMG_Summoning_Native_*` child and proves mode off, eligible success/failure exactly once, cancellation, command snapshot, ineligible behavior, fatigue/rest, Cord, and cleanup. | PASS |
| Augment, Superior, Sacred Summons | Mechanical runtime proves Augment applies once and Superior changes quantity only, when installed. Structural fact search confirms no Sacred Summons surface existed; the feature made no guessed mutation. | PASS |
| Prepared/spontaneous and action-bar/metamagic | Parent lists remain unchanged except additive variants; all generated nodes have exact native spell/metamagic/material/action-bar contract. Prepared invocation is exercised through Acadamae; identical parent ownership preserves spontaneous discovery and slot level. Fresh launch logs have no summon/action-bar exception storm. | PASS |
| Existing feature regressions | Complete 1009-test suite includes Shield Other, Acadamae, Gunslinger, firearms, settings, Cord, vendor, loot, and persistence coverage. Module matrix proves no cross-surface leakage. | PASS |

## Sections 15-17: domain, runtime, build, and package

| Requirement | Evidence | Audit |
|---|---|---|
| Required domain/static cases | All named catalog, identity, quantity, template, merge, transaction, disabled publication, 16-state settings, migration, sanitizer, donor, adaptation, count, and existing-feature cases are registered in the dependency-free executable. Current result: 1009/1009 PASS. | PASS |
| Structural runtime | Final run PASS 31/31, covering 67 units, 1,050 ability identities, 681 published roots, 1,045 generated nodes, 55 donors, sanitizer/source isolation, exact special structures, parents, and registration. | PASS |
| Actual cast matrix | Final mechanical run executed all 123 one-creature options, 16 d3 and 14 d4+1 tier/family representatives through native `AbilityData`, `UnitUseAbility`, and `RuleCastSpell`; count, kind, duration/range, combat, feats, templates, and cleanup PASS. | PASS |
| Movement/commands/AI/combat | Every spawned unit had native summon faction/control and command surfaces. Natural units use the bounded native attack brain; three specials have frozen custom AI identities. Visual run exercises locomotion agents and attacks; mechanical run exercises representative hit/damage/special effects. No KMG mode-specific path exists. | PASS |
| Visual contracts | 67/67 attached/renderable views, bounded geometry/selection/footprint, navigation/locomotion, attack, projectile fallback, hit/death, and exact cleanup. No screenshot/OCR is used for mechanics. | PASS |
| Persistence and disabled load | Both three-stage fresh-process sequences PASS through Steam App ID 640820 using only `KMG_AUTOMATION_WORKING`; identity/context/duration/control/no duplication, expiration, save/relaunch absence, and disabled publication are proven. | PASS |
| 16-state matrix | 16/16 fresh launches, constant registry, exact module surfaces, no duplicate/leakage, settings hash restored. | PASS |
| Compatibility profiles | Eight required transactions PASS at current installed profile versions; standalone/CotW/combined repeated twice after native source freeze; every transaction restored Mods/settings. | PASS |
| Static/build/package gates | `git diff --check`, deterministic roster validator, repository validation, 1009 tests, exact-reference warnings-as-errors Release, repeat deterministic build, strict canonical package, and exact hashes are recorded. Final audit will refresh hashes from the final artifact source. | PENDING final hash freeze only |
| Publication exclusions | Git tree/package validators reject proprietary assemblies, installed-mod DLLs, saves, credentials, raw runtime logs/screenshots, machine config, and generated packages. | PASS |

## Sections 18-22: release documentation, publication, and closeout

| Requirement | Evidence | Audit |
|---|---|---|
| Version and release docs | `Info.json`, `Directory.Build.props`, changelog, README, installation/compatibility, known issues, build info, testing, architecture, manifest, module and runtime docs identify 0.0.78 and current counts. Historical sprint text is explicitly subordinate to the 0.0.78 sections. | PASS |
| Complete roster/fidelity record | Generated roster includes every family/tier, policy, donor GUID/name/view, unit ID, placement/execution IDs, adaptation/sanitization and qualification. Fidelity matrix gives full profiles/deviations and evidence. | PASS |
| Reviewable commits and push policy | 105 coherent, non-merge commits currently follow the selected baseline. Every coherent checkpoint was pushed with the mandated helper. | PASS |
| Draft PR | Draft PR #2 targets `master`, remains open/unmerged, and contains baseline, counts, tests, runtime/profile results, hashes, deviations, and visual checklist. Final audit will refresh its head SHA/hash block. | PENDING final refresh only |
| Definition of done | All behavior and native evidence conditions are proven above; only self-referential final artifact/hash documentation and PR refresh remain. | PENDING |
| Catastrophic conditions | None present. Repository/game/assets/save path/Steam/GitHub are usable; no engine hard limit or unsafe conflict was encountered. | PASS |
| Final response contract | State/report contain all required values. Issue only after final artifact freeze, push, PR verification, clean/local-origin equality, and goal completion update. | PENDING |

## Residual human-only aesthetic checklist

Mechanical and visual-contract qualification is complete. Reviewers may still
inspect only subjective presentation: proxy scale beside ordinary party units,
camera comfort for Roc/Bebelith/Mastodon, selection-circle aesthetics, sleep-arrow
and light-ray visual polish, and icon/list readability in crowded spellbooks.
These checks may not overturn or substitute for the structured mechanical
results unless they reveal a reproducible defect.
