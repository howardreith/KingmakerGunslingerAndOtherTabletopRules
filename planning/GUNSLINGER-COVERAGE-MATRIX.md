# Gunslinger mandatory coverage matrix

Status values follow `AUTONOMOUS-GUNSLINGER-MISSION.md`. This initial audit is
conservative: a lower-level subsystem or old sprint label is not completion.

| Mandatory area | Authority | Status | Implementation | Deterministic tests | Runtime scenario / latest evidence | Remaining work | Final disposition |
|---|---|---|---|---|---|---|---|
| Class chassis, levels 1-20, BAB, saves, hit die, skills | private `GUNSLINGER_PFSRD.md`; mission 4.1 | NOT-STARTED | none | none | none | Implement and qualify full progression | Pending |
| Character creation, level-up, multiclass, respec | mission 4.1 | NOT-STARTED | none | none | none | Implement stable class/progression blueprints and live flows | Pending |
| Presentation, localization, progression UI | mission 4.1 | NOT-STARTED | firearm-only localization exists | none | none | Class/features/icons/tooltips | Pending |
| Firearm proficiency | rules; ADR-0014 | RUNTIME-QUALIFIED | `FirearmProficiencyBlueprints.cs`, restriction/runtime | domain suite | accepted through Sprint 29; 0.0.30 regression pending | Preserve and integrate into class | Pending final integration |
| Gunsmith / starting equipment | rules | NOT-STARTED | diagnostic Test Musket only | none | none | Faithful Kingmaker equivalent and production starting kit | Pending |
| Grit pool, bounds, rest behavior | rules | NOT-STARTED | none | none | none | Implement persistent per-unit resource | Pending |
| Grit critical/killing-blow recovery and duplicate protection | rules | NOT-STARTED | none | none | none | Implement exact firearm event recovery | Pending |
| Deed progression and all deed tiers | rules | NOT-STARTED | none | none | none | Implement every fidelity-matrix row | Pending |
| Bonus feats | rules | NOT-STARTED | none | none | none | Implement selections at levels 4/8/12/16/20 | Pending |
| Nimble | rules | NOT-STARTED | none | none | none | Implement +1 through +5 dodge progression | Pending |
| Gun Training | rules | NOT-STARTED | none | none | none | Implement weapon selection and Dex-to-damage behavior | Pending |
| True Grit / level-20 behavior | rules | NOT-STARTED | none | none | none | Implement deed cost floor and selected deeds | Pending |
| Early/advanced firearm definitions and handedness | firearm rules; mission 4.4 | SOURCE-IMPLEMENTED | immutable definition vocabulary; only early musket factory/content | domain definition tests | Test Musket runtime only | Production early and advanced catalog | Pending |
| Range increments, maximum range, touch AC in penetration range, ordinary AC outside | firearm rules; ADR-0016 | RUNTIME-QUALIFIED | `Rules/`, `Diagnostics/FirearmRangeMath.cs` | domain combat tests | accepted pre-Sprint 30 | Extend/verify for production early/advanced definitions | Pending final integration |
| Native concealment, mirror image, cover, LOS, penalties, criticals, damage | ADR-0005; mission 4.4 | SOURCE-QUALIFIED | ordinary native weapon pipeline | combat/patch contract tests | prior focused runtime evidence | Comprehensive final runtime regression | Pending |
| Loaded state, compatible ammunition, atomic inventory reload | ADR-0026/27 | RUNTIME-QUALIFIED | `Ammunition/`, `Reloading/`, item tokens | domain transaction tests | Sprint 29 acceptance; 0.0.30 regression pending | Production definitions and partial reload | Pending |
| Capacity >1 and partial loading | firearm rules | NOT-STARTED | definition model rejects generic runtime beyond capacity one | validation tests only | none | Implement Sprint 33 behavior | Pending |
| Exactly one chamber consumed; empty firearm rejection | ADR-0027/28 | RUNTIME-QUALIFIED | `Firing/` | discharge tests | prior accepted runtime evidence | Multi-capacity regression | Pending |
| Natural-roll misfire and duplicate event protection | ADR-0029 | RUNTIME-QUALIFIED | `Misfires/` | misfire/event-gate tests | prior accepted runtime evidence | Production catalog regression | Pending |
| Normal -> Broken -> Wrecked, wielder and nearby burst | ADR-0030/32/33 | RUNTIME-QUALIFIED | `Misfires/`, `Explosions/` | condition/explosion tests | Sprints 24-26 runtime acceptance | Production catalog regression | Pending |
| Repair and overhaul with atomic kit consumption | ADR-0034/35/36 | RUNTIME-QUALIFIED | `Recovery/`, generic exact-firearm policy | transaction and Sprint 30 tests | generic actions retained through Sprint 31 catalog runtime on `1539ae9` | Production acquisition/final regression | Pending final integration |
| Scatter weapons | firearm rules | SOURCE-IMPLEMENTED | special-range definition plus exact-reference target planner | 10 focused planner cases within 634-test suite | none; Blunderbuss remains unavailable | Establish cone geometry/range, per-target attacks, one discharge, all-roll misfire, triple explosion, and runtime qualification | Pending |
| Critical ranges/multipliers and special ammunition | firearm rules; mission 4.4 | NOT-STARTED | native Heavy Crossbow fixture values only | none specific | none | Production content and special ammunition scope audit | Pending |
| Equipment switching and independent identical firearms | ADR-0006/19 | RUNTIME-QUALIFIED | item-owned token repository, exact equipped resolver | repository/action tests | Sprint 29 two-item runtime acceptance | Production catalog regression | Pending |
| Inventory/stash/transfer/loot/sale/copy/reconstruction lifecycle | ADR-0034; mission 4.4 | SOURCE-QUALIFIED | token persistence and lifecycle diagnostics | persistence/lifecycle tests | partial prior runtime evidence | Explicit final path matrix and qualification | Pending |
| Save/load persistence, schema migration, diagnostics, conservative recovery | ADR-0019; mission 4.4 | RUNTIME-QUALIFIED | token codec/reconciliation and runtime diagnostics | persistence/reconciliation suite | accepted save/restart evidence through Sprint 29 | Final schema/removal/corruption qualification | Pending |
| Generic definition-driven Reload/Repair/Overhaul | Sprint 30 contract | RUNTIME-QUALIFIED | `Actions/` plus compatibility adapters | 624-test exact suite plus focused guarded-scenario tests | two Sprint 30 PASS runs plus production catalog regression on `1539ae9` | Preserve through acquisition/final integration | Pending final integration |
| Production firearm/ammunition/kit blueprints and starting access | mission 4.5 | SOURCE-QUALIFIED | stable Pistol, Musket, and fail-closed Blunderbuss pairs; existing ammunition/kits | catalog/domain/runtime scenario tests | two fresh-process `production-firearm-catalog` PASS runs on `1539ae9`; latest `20260801T1336546357107Z-1986affde5794aad8eb0710a31932eb0` | Implement scatter, starting grant, and later acquisition | Pending |
| Later equipment acquisition and economy | mission 4.5 | NOT-STARTED | none | none | none | Vendor/loot/crafting path | Pending |
| Player actions and diagnostics | mission 4.5 | SOURCE-QUALIFIED | Test Musket reload/repair/overhaul and diagnostics | domain/runtime harness tests | Sprint 29 accepted; generic 0.0.30 pending | Generalize names/content and final qualify | Pending |
| Visuals, animation, sound, projectiles | mission 4.5; ADR-0007 | INVESTIGATING | Heavy Crossbow fallbacks | none | prior runtime observations only | Approve production fallbacks and document limits | Pending |
| Install/update/removal/compatibility documentation | mission definition of done | NOT-STARTED | historical package docs only | validators | historical packages | Final release docs and warnings | Pending |
| Final comprehensive autonomous acceptance | mission definition of done | NOT-STARTED | working-save harness foundation | harness tests | working-save smoke qualified at `4f28dcf` | Build scenario; two fresh-process PASS runs | Pending |
