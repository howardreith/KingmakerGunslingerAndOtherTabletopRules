# Sprints 30-31 - active Outsider/Celestial pilot

Product PR [#66](https://github.com/howardreith/KingmakerGunslingerAndOtherTabletopRules/pull/66), draft. Product/build/promotion root `C:/Dev/KingmakerGunslingerLab/wt/s30-31`; branch `codex/expanded-summoning-sprints30-31-outsider-celestial-pilot`. Started at released master `a35a3277a9c5e2c111475523b868c2cf08422e05`. Stable v0.0.163 remains public. v0.0.164 was free at intake, not reserved; recheck before promotion. No merge/release/records PR. One unfinished batch; STOP before Sprint 32.

## Scope and coverage map

| Charter | Creature | Routes | Current proof | Required next work |
|---|---|---|---|---|
| S30 | Lemure, new | SM II one; III 1d3; IV-IX 1d4+1: 8 new | Hidden bootstrap; adult profile, signed claws, physical/energy and descriptor-buff consumers; actual single attack on 38a94864 | Private five-foot claw and ability-level immunity candidate; isolated full/AI; original usable body; all routes, expiry/persistence/final artifact |
| S30 | Dretch, new | SM III one; IV 1d3; V-IX 1d4+1: 7 new | Original painting inspected; inert | Early CL2 finite Cause Fear/Stinking Cloud, actual poison/nausea/lifetime and safe AI target choice; claw/bite; no nested summon |
| S30 | Hound Archon, new | SM IV one; V 1d3; VI-IX 1d4+1: 6 new | Original painting; inert | Shared archon source isolation/support; owned sword/bite vs natural routine; canine/humanoid body; general PF1 attack roles per owner |
| S31 | Lantern Archon, existing | Preserve 7 SM III-IX identities/order | Released implementation unchanged | Owned luminous orb, standard/full ray distinction/range/touch/DR; shared archon isolation/support; valid existing icon |
| S31 | Astral Deva, new | SM IX one: 1 new | Original painting; native deva audited; inert | Distinct angel aura; enhanced disrupting hammer/slam/two-hit stun; CL13 SLAs incl limited Heal/Blade Barrier; owned equipment |
| S31 | Trumpet Archon, new | SM IX one: 1 new | Original painting; native book API audited; inert | Early genuine CL14 prepared-slot/action/ownership; legal filtered loadout; archon auras; trumpet/sword, archon-only blast exemption and safe AI |

Five additions plus one upgrade: 23 new roots and 7 changed/preserved Lantern roots, 30 affected. No SNA, templates, alignment-feat roster/variants, mounts, change shape, Lantern gestalt, new flight/navigation/illumination/social/skill framework or Sprint 32.

Baseline: 117 creatures; Monster 96/545; Nature 104/582; 1127 roots + 29 wrappers = 1156 choices. Current: 118; Monster 97/553; Nature unchanged; 1135 registered roots, 8 hidden; public 1127 + 29 = 1156. Final cross-check: 122; Monster 101/568; Nature 104/582; 1150 + 29 = 1179. Actual catalogs/visibility/retained wrappers are authority. Only Lemure is registered; Lantern remains available and other additions inert.

Append-only identities: baseline 7140 retained exactly. Current 7185 entries / 7182 active / 3 historical reserved: 45 appended (unit + 8 roots + 2 carriers + 2 justified private weapon identities + 32 caller adapters). New claw type `dfcd9e2b2dd14e1c81b052aaf21fac96`; item `780c5819371c4b5b88d72d12fc43a9f2`. Allocation used the existing tool, activated only those two new rows and compared every preceding row exactly. Foundation 2316; adapters 3869.

## Latest native artifact

Development source `38a948644eb1a77d79f0fa1263d6c85515aa9aa1`, purpose `sprints30-31-lemure-core`, remains immutable in this root.

| Identity | Value |
|---|---|
| ZIP SHA256 | `aaf458050c1063a73e81058b8ff06c90603e9502ad5121ff48e66c753c18dad7` |
| DLL SHA256 | `9875777cb2a8d8acb86f45320afd919f3b63945d535d32abf3a1574f5e5e79a9` |
| MVID | `4254999d-ae7d-4e7c-aa08-df57dd4e8d72` |
| Source-state SHA256 | `d5bd7ce84640ffcaa477c0a8011084da7b46d8a254b887c61ba75a3591d42505` |

Build 22:58:28.5486318Z-23:01:52.8065994Z: 15 selected tests, exact 2.1.7b/Harmony12/UMM compile and strict 399-file package PASS. Eighth game/transaction 23:02:44.8311828Z-23:06:39.3532555Z: FAIL, 28 PASS/3 FAIL. Request `20261021T2304479604491Z-disposable-expanded-summoning-sprints30-31-native-review`; RunId `20261021T2304479917675Z-0f890d574e594af9bb03b359fde01f7b`; native-result interval 23:04:55.4792468Z-23:06:31.4880007Z (96004ms) includes fixture setup.

Observed: all existing adult/signed-claw/DR/energy/descriptor-buff/no-SR checks passed; native template absent and summon marker retained. The borrowed claw actually has two-foot reach: an independent printed-five-foot check failed. The original 2.2m contact was outside native ApproachRadius 2.0096m; measured 1.9596m contact admitted the issued single attack. Native roll 17, attack +2, one 1d4 claw packet dealt 4; command Success and one ordinary standard-action cost, fixture attack commands=1, forced rolls=0. CanAct/conscious/enemy/view predicates were true. Then reset-before-full detected targetOutOfCombat=false (all other recorded reset predicates true); it stopped and left full/AI unrun. No forced protected-party cleanup.

Last successful bootstrap: 08cb4271, 6/6, actual conscious Int0/HP13/BAB2/two-primary-claw plan and exact 1156 public choices with 8 hidden routes. Every artifact/receipt here is development evidence, never release qualification.

## Current coherent correction and disposition

| Ability | Mapping/access/priority | CL/DC/uses | Ownership/lifetime | State |
|---|---|---|---|---|
| Lemure claws | Private Medium 1d4/five-foot item/type via existing weapon clone; NaturalBody primary + additional; native DumbBrain | Racial HD2; ordinary native attacks | Exact owner/item Strength correction; unchanged qualified equipment sweep marks both instances nonlootable | Signed native calculations and old-weapon single proved; changed private item needs native core/full/AI |
| Lemure defenses | Own DR5 good OR silver; native fire immunity; acid/cold10; native buff + ability descriptor-immunity components | No casting resource/SR | Private trait carrier, no donor mutation | Physical OR/fire/resistance/unrelated energy/buff controls passed; separate ability-level immunity candidate needs native proof |
| Lemure mindlessness | Int0 absent-score surrogate, no ranks/selections/feats or generic undead traits | No intellect granted | Creation-only racial HP/save bases | Actual conscious/CanAct/single attack proved at Int0; AI remains |
| Lemure senses | Printed darkvision60/see-in-darkness references | Passive | Established ordinary-map/passive-sense convention | No new senses/illumination framework |

The fixed `lemure-core` phase now runs rules, a verified rules-to-single reset and one issued single attack. `lemure-full` and `lemure-ai` each require a fresh guarded process/transaction after the observed combat-state residue. No general batch engine, weaker reset or protected-party tick/reset. Request admission remains exact/case-sensitive, one parameter, empty native world, no save and automatic exit. Focused production compile18.2s/tests4.1s: 8 PASS; updated admission/composition subset: 8 PASS; final added native alignment/book/ability/damage-alignment bindings compile PASS. One new clean package will serve the three separately restored transactions; no source/DLL changes between them.

Native API inspection showed BuffDescriptorImmunity only handles RuleApplyBuff. Ability-level immunity uses UnitPartSpellResistance and SpellImmunityToSpellDescriptor. The candidate adds only MindAffecting/Poison descriptor eligibility, with actual native Cause Fear/cloud rule checks and susceptible controls; these rule probes are not finite spell casts. The spellbook/HD contracts remain separate.

Owner decision: Hound uses general PF1 attack-role rules with live Strength; disclose printed damage discrepancies, no species-only multipliers. Trumpet blast prints DC19 but general 14HD/Cha17 scaling gives20: async owner choice pending; no blast implementation selected. Prepared native book APIs are present; printed loadout counts still need an explicitly legal, filtered preparation before implementation. Do not convert this caster into arbitrary SLAs.

## Transactions and final dependency plan

All eight game transactions restored actual protected intake exactly, including independent comparison: current human mod v0.0.117, Mods967 entries, 118 saves/metadata/original Working, 18 references, sound bank, 83 settings, tasks0/processes1(Steam), no owned game/lease/lock. Counts are observations, never future restoration authority. No save has been loaded/written. Current laptop is restored.

One writer/runtime owner. Current guarded unelevated Steam App640820 and fresh intake each transaction; immutable baseline/other saves. Complete the corrected small Lemure slice, then early Dretch cloud; prove Hound/Lantern shared archon source isolation and one support decision, then early Trumpet prepared-slot/action/ownership before bulk caster setup/final art. Pause dependent expansion behind shared failures. For Dretch use the general native cloud: the existing mephit helper intentionally excludes allies under a different charter and is not its mapping.

Rules-to-single reuse and deliberate injury-residue detection have actual successful observations. Single-to-full sharing failed; full/AI stay isolated. No native startup amortization or complete sharing success claimed. Different profiles remain separate. Persistence preserves prepare/cold ON/OFF/cleanup/fresh-absence boundaries; mix six units in one envelope only if supported and safe. No protected party deletion/ticking to manufacture isolation.

Remaining mandatory coverage: all six actual profiles and ordinary/special/AI consumers; source-specific/mixed auras and independent resources; RTWP/TB action/cancellation/expiry; usable bodies/icons, donor/private/cache cleanup; equipment/disarm/death/expiry/no loot; each 23 new + 7 Lantern route's identity/slot/quantity/duration/eligibility/cleanup; affected persistence owners; standalone/high-risk combined and relevant old controls. Reuse only under actual unchanged dependencies with original tuples/reasons. No development proof becomes final-package proof.

Before freeze: one coherent read-only internal review, small combined publisher selector/map with acceptance/refusal guards, inspect exact head/reporting lineage. Then one combined complete source/build/package gate and exact public-artifact qualification. Remove only five suppressions after hidden qualification. One product merge/stable release/public downloads verification; exact restoration; separate records PR/merge; STOP before S32. No record-only rebuild or alternate-root bytes.

## Art, bounded research and retained failures

Five original paintings and exact built-in imagegen prompts/hashes are in [generation provenance](../assets-source/original-icons/expanded-summoning/sprint30-31-generation.json). Only Lemure is exported/registered; 130 distinct sources/exports validated. Dretch source inspected and appropriate for export. Prior pixels unchanged; canonical exporter completed only 11 already-released S29 consumer associations previously absent from metadata. HumanReview=NOT_PERFORMED_NONBLOCKING; no owner aesthetic approval. Recognizable usable bodies remain mandatory; rough anatomy/wings/seams/harmless clipping/texture polish are backlog.

Bounded capture resolved four exact donors/canonical consumers; six species-name counts0 are research, not donor suitability. Retained native deva `afe56099ff6046b40a359b7562c0424e` / prefab `69aa28050e471024baa6f2150bcc0751`; general cloud `68a9e6d7256f1354289a39003a46d826`. Private captures/metadata stay local. One named-skill metadata check found CheckDiplomacy/CheckIntimidate and ordinary Perception/Stealth/UMD, no separate Climb/Craft/Perform/Handle Animal/Escape Artist/Swim/Sense Motive stats. Retain specific reference values, no rank reallocation/broad compensation; actual combat mappings still required. Known final biped rig/census and existing attachment pipeline are available; no neck campaign.

| Record | Outcome/correction |
|---|---|
| Push policy | Exactly two authorized branch names; encoding/newlines/case-sensitive match preserved; 9 refusal probes PASS. Backup SHA b9692c31f149823ae77bda241b6246b52dfb4eb46474210e50793fd9e7582c3b; updated 883c443095893f4e3f040e36f279b51de903978751ac6b997e89b155b6d270e8. |
| Initial setup/builds | Elevation refused; local false-zero wrapper NONQUALIFYING, fail-closed streams/exit/receipt repaired. Fresh Newtonsoft path child-env only. Stale icon pin/count/import fixtures repaired without weakening refusals; raw failed builds retained. |
| 73cd070c | First game, input-only capture PASS; query gaps repaired and freshly observed. |
| 9c052643 | Second game startup failed historical129-icon guard; exact key/pixel/uniqueness validation retained. |
| bb44da2d | Third game conscious Int0/HP13 but missing primary claw: disabled-hand product defect. NaturalBody fix proved in fourth-game 08cb4271 bootstrap6/6. |
| 077e33ca | Fifth game primary Str15 damage+3 vs+2. Existing exact-owner multiple-primary correction reused; both claws then passed signed calculations. |
| ae141430 | Sixth game electricity12 became7. Exact native DR excludes energy; scoped native-template boundary qualified on subsequent artifact, other energy and descriptor-buff controls passed. |
| e908e25b | Seventh game readiness stopped before attack; missing predicate attribution repaired. Eighth game measured the borrowed contact out of native range and succeeded at legal contact. |
| 38a94864 | Eighth game: two-foot reach product defect, actual single attack PASS, retained target combat state detected before full. Separate fixed fresh-process phases selected; full/AI unrun. |

Eight completed development packages; failed attempts/refusals remain immutable and compiler/build-attempt totals will use actual logs. Backup per-file Host listing alone previously took at least173.3s before copying. Moving only that listing to Verbose preserved all guards/copy/restoration; later backup-stamp-to-deployment intervals were about10-12s inclusive, a different interval. Native-result durations include fixture setup; useful-observation/restoration subdivisions remain unmeasured where timestamps do not establish them. No clean throughput ratio or overall speedup inferred; final combined qualification/publication and comparable S28/S29 timing assessment remain.
