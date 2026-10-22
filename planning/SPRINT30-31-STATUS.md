# Sprints 30-31 - active Outsider/Celestial pilot

Product PR [#66](https://github.com/howardreith/KingmakerGunslingerAndOtherTabletopRules/pull/66), draft. Product/build/promotion root `C:/Dev/KingmakerGunslingerLab/wt/s30-31`; branch `codex/expanded-summoning-sprints30-31-outsider-celestial-pilot`. Started at released master `a35a3277a9c5e2c111475523b868c2cf08422e05`. Stable v0.0.163 remains public. v0.0.164 was free at intake, not reserved; recheck before promotion. No merge/release/records PR. One unfinished batch; STOP before Sprint 32.

## Scope and coverage map

| Charter | Creature | Routes | Current proof | Required next work |
|---|---|---|---|---|
| S30 | Lemure, new | SM II one; III 1d3; IV-IX 1d4+1: 8 new | Hidden adult/claws/defenses/single/full/AI; original body/core38/38 on4b122ab0 plus native party-camera/private cleanup/SR0 body-pair proof on40b1ab11 | Full animation/attack/expiry bodies; turn-based and item lifecycle; all routes, expiry/persistence/final artifact |
| S30 | [Dretch](https://www.aonprd.com/MonsterDisplay.aspx?ItemName=Dretch), new | SM III one; IV 1d3; V-IX 1d4+1: 7 new | Hidden adult/cloud10/10 on73e1c6f8; c5b430eb native safe cloud5/5 and owned player-ally Fear5/5; original128px icon; original native body/camera/private cleanup/resources/SR0 pair proof40b1ab11 | Signed/resolved melee and defense consumers, held/action Escape Artist+5, lifecycle; final-artifact AI/finite-resource controls |
| S30 | Hound Archon, new | SM IV one; V 1d3; VI-IX 1d4+1: 6 new | Hidden native adult, signed armed/natural plans, canonical CL6 circle, mixed menace/source hand-off and actual CL6 Aid AI; refreshed native original body/SR15/circle/menace/plans/support18/18 one093336c | Full defenses/resolved attacks/Power Attack, hit-break/24h, disarm/fallback, combat body/lifecycle and final artifact |
| S31 | Lantern Archon, existing | Preserve 7 SM III-IX identities/order | Released identities/rays retained; candidate menace/Aid with Hound freshly observed | Owned luminous orb, standard/full ray distinction/range/touch/DR; shared archon isolation/support; valid existing icon |
| S31 | Astral Deva, new | SM IX one: 1 new | Original painting; native deva audited; inert | Distinct angel aura; enhanced disrupting hammer/slam/two-hit stun; CL13 SLAs incl limited Heal/Blade Barrier; owned equipment |
| S31 | Trumpet Archon, new | SM IX one: 1 new | Hidden genuine CL14 native book/Heal AI and separate turn-controller proof; Purge12/12 plus turn4/4 one packagee98af7e7; repeated Virtue/action/slot isolation on aeb25601/37a600d2 | Full legal preparation and cold slot hydration; remaining feats/skill consumers/Aid; trumpet/sword, archon-only blast exemption and safe AI |

Five additions plus one upgrade: 23 new roots and 7 changed/preserved Lantern roots, 30 affected. No SNA, templates, alignment-feat roster/variants, mounts, change shape, Lantern gestalt, new flight/navigation/illumination/social/skill framework or Sprint 32.

Baseline: 117 creatures; Monster 96/545; Nature 104/582; 1127 roots + 29 wrappers = 1156 choices. Current:121; Monster100/567; Nature unchanged; 1149 registered roots,22 hidden; public 1127 + 29 = 1156. Final cross-check: 122; Monster 101/568; Nature 104/582; 1150 + 29 = 1179. Actual catalogs/visibility/retained wrappers are authority. Lemure, Dretch, Hound and the early Trumpet prepared-book slice are registered hidden candidates; Lantern remains available; Astral remains inert.

## Current correction: armed attack observed, aggregate FAIL retained

Source2995a15e8ea24a93260dc8913ba9ff6471aa335c/package40, purpose`sprints30-31-hound`:
ZIP`5fd660cd65513486925027ecf6d4c061106f9ab7f927fd9b56cc22e3b0ad7855`,
DLL`5cc11a0f7e0bba2e1b991420cd3993d12bfd07b4f3abc8eda6b36c474bfc3084`,
MVID`67d0afa4-9924-45d7-a10f-f9170b36bc10`, source-state`d25c8c63614de163d857aa4880871b6b4e3301cd65afe0863dc10cf65ba30827`.
Build13:07:59.5723231?13:12:32.1973390Z passed803 actual limited preflight,
14 actual outer dispatch/refusals,27 dispatch/15 selected/exact/strict408 package.
Game44 transaction`compat-20261022T131317Z-3f0baf24f706`, request
`20261022T1315258748949Z-disposable-expanded-summoning-sprints30-31-native-review`,
RunId`20261022T1315259165903Z-a9696193eb20424993166ba011b42d80`.
Six PASS/one FAIL: actual native Power Attack source buff5898bcf7 supplied both
-2 attack modifiers, sword9?7/damage3?9 and secondary bite3?1/damage1?3. One
native full command resolved sword7/2 and bite1, all three random hits/damage,
one full-round action charge. Those are observed mechanics, not aggregate PASS.
Observer then restored the prior turn-based setting before its final cleanup
wait, correctly hitting the unchanged uncovered-actor guard; cleanup row UNRUN.
Premature restoration moved after the last assertion in an outer finally.

Actual combat PNG and renderer list show no sword model: product presentation
omission, not aesthetics. Exact native hand-equipment Active requires a Character
avatar, absent on this donor. Small exact-Hound-only native sword model instance
now follows measured R_Palm/actual equipped nonlootable item, hides in natural/
disarmed mode and joins body rollback/teardown/resource evidence. No native
prefab/material mutation, no new asset redistribution/item/attack/loot. Native
proof required. Natural case on2995 was NOT launched after infrastructure failure.
Transaction13:13:16.7128150?13:17:29.4161704Z, restored13:17:29.1662993Z;
independent full Compare PASS, human intake/Working untouched. Totals44 actual
game launches/transactions,40 completed development packages. Current focused
correction compilation19.2s/14 tests1.4s PASS; final default-blueprint model lookup
also supports a future cold natural/disarmed load; final focused compile17.7s and14/14 selected tests1.1s PASS.

## Latest exact development artifact: Hound original body

Tested source `e093336c42e8a80f753aa3cbf913f539b270c4ea`, completed development
package39, purpose`sprints30-31-hound-body`; archon-hound18/18 PASS in one actual
game/process/transaction (game43). Native original canine-headed/floppy-eared
humanoid body is finite, visible/selectable and usable at actual party camera
scale. Both actual PNGs were inspected. Idle images do not establish a drawn
sword; item ownership/nonlootability passed, resolved commands and combat images
remain required. Coarse anatomy/cloth/seams/textures are backlog; no owner visual
approval. Four captured private view resources released; shared canonical CL6
Magic Circle, SR15, signed armed/natural plans, mixed source-specific menace,
reset and useful Aid/no-repeat all passed on this artifact.

- ZIP SHA256 `1920c7d0a6c8ba6a76a975b6f9c7a801bfa5656653f07ba96eb8d65ebd731660`
- DLL SHA256 `bc251db9e98036b4bda0deaacbea1b422c65d983ca2a869278209ed48bdd93e1`
- MVID `18231bd0-6ca7-4998-9497-3f04673543a0`
- Source-state SHA256 `8a700d6852b6104d6d60474885939250d304d277c2ad7d6b35569db4df3684c1`
- Build12:49:06.6601452?12:53:57.5235265Z PASS: fresh actual limited admission,
  actual outer dispatch/refusals,27 production dispatch,15 selected/exact/strict
  408-member package. Synthetic count408 acceptance/406 mismatch/409 unsupported
  checks passed. Initial admission12:47:25.7216652?12:48:28.2916294Z refused
  scratch-file deltas in its no-artifact-change test: infrastructure interference
  from preparing runtime scripts, no compile/package/deployment/game. Original
  failure retained; fresh retry held all files frozen and passed.
- Transaction`compat-20261022T125434Z-5fb4f47f52ae`; request
  `20261022T1257026548158Z-disposable-expanded-summoning-sprints30-31-native-review`;
  RunId`20261022T1257027441818Z-5455f4c8dc1b4d5ba814c5c32b5ff7b8`.
  Supervised12:54:33.3384294?12:59:18.4480926Z; native12:57:17.8078787?
  12:59:10.0120515Z,112200ms. Restored12:59:18.2518011Z; profile/settings
  restoration and independent full Compare PASS (Mods967,saves118,refs18,
  settings83,tasks0,processes1 Steam, bank/ref/settings bytes/metadata exact).
  No saves touched or owned live game/task/lease/permanent development install.
- Local build`artifacts/s30-31-hound-build-e093336c-r2`; native
  `artifacts/s30-31-hound-e093336c`, including exact image hashes/inspection.

Prior fixed Lemure/Dretch original-body pair12/12 in one process/package38 on
40b1ab11 remains at its [immutable map](https://github.com/howardreith/KingmakerGunslingerAndOtherTabletopRules/blob/90c600f47d6c0bd6f0b12c2533c564ed73cbe120/planning/SPRINT30-31-STATUS.md).
Its order/cleanup/residue evidence is not relabeled as this Hound run. Both
native SR0/canonical extraplanar identity and distinct original bodies passed;
Dretch independent resources unchanged. Earlier actual Lemure38/38 and
Hound/Lantern15/15 on4b122ab0 remain linked there. Changed final dependencies
still require fresh combined-artifact qualification.

At that body checkpoint:43 actual launches/transactions,39 completed development packages;
failed admission/request/build attempts separate. Current source adds exactly
`hound-armed` and`hound-natural` fresh-process full-command observers: native
Power Attack toggle/source modifiers, real random rolls/damage, exact routine,
action charge, combat image and owned cleanup. Focused compile18.1s and14/14
selected tests PASS (4.5s wrapper); these new command rows are not native proof
yet. One actual command per fresh process retains the observed combat-state
boundary; no forced sharing or protected-party reset.

Next: qualify those two commands using one changed package, then continue the
remaining six-creature mechanics. Lantern one-standard/two-full rays/orb,
Astral full mechanics and Trumpet signatures/loadout remain pending. No public
candidate/final qualification/merge/release/records PR has occurred.

## Current dependencies and decisions

The scoped `OutsiderEscapeArtist` production adapter is unchanged after game36. Existing private Dretch/Trumpet traits own ranks2/class and ranks14/nonclass. Five exact native value reads and one native skill construction preserve native algorithm/CMB choice, live Dex and engine armor/condition/buff stream. Ordinary skills/stats remain unchanged. Native rule probes and actual item/scene/resource reset now passed onf2df6def; source-specific live-item negative control also passed. Actual held-state/action/TB qualification remains. Shared setup was demonstrated through reset into later real caster AI, with the aggregate observer failure preserved. No new request phase/permission. No unchanged package rebuild is needed for this record-only milestone beyond future changed production/observer inputs. Continue independent scoped implementation and remaining native requirements.

Ledger7313 entries/7310 active/3 historical,173 appended after exact baseline7140; foundation2388/adapters3925. RegistrySHA `366bebfdd633a32c84d37e04aa2590527d599acf81399aec5504d16c6380dae1`. Each append/activation had a backup/hash and explicit new-symbol list; no released identity or protected pixels were replaced. Exactly two authorized push allowlist additions were backed up/hashed/refusal-tested; sanctioned wrapper only.

Owner choices: Hound general PF1 live signed attack-role Strength, with printed discrepancy disclosed; Trumpet general supernatural blastDC20/live Charisma, printed19 disclosed. Offensive dismissal/banishment/Dispel Evil/Holy Word riders remain legal; caster travel is excluded. Hound/Trumpet use the actual shared published Magic Circle so future changes carry through. Its accepted limits omit summoned-creature contact/SR barrier and suppression of existing control; configured AC/save/new-control immunity remains. Astral's additional20ft/+4/lesser-globe mechanics remain mandatory.

Canonical native extraplanar additionally bound only to Lemure/Dretch/Hound/Lantern/Trumpet: native dismissal/banishment ignores the project's inert subtype marker. Existing markers/identities and other roster units are preserved. Astral must bind this fact. Fresh final affected-consumer checks remain mandatory.

Trumpet preparation is creation-only native bookCL14, zero cleric class levels/domains, real slots/capacities6/5/5/5/4/3/2. Current23 preparations across levels1-7. L1 drops one printed Cure Light copy to fit six; L2 excludes expensive Consecrate and drops one Lesser Restoration copy to fit five. Other copies stay. Travel/expensive-material preparations have no combat replacements. Sanctuary, Spell Immunity, Dispel Evil and remaining named mappings are incomplete. Native Stabilize has empty actions and Daylight has party/global actions; no chosen adaptation or silent omission. Virtue stays a real native Spell/Cantrip fact, no fictional L0 Memorize/SLA relabel.

## Per-ability disposition

| Ability | Mapping/access/priority | CL/DC/uses | Ownership/lifetime | State |
|---|---|---|---|---|
| Lemure claws | Private Medium 1d4/five-foot item/type via existing weapon clone; NaturalBody primary + additional; native DumbBrain | Racial HD2; ordinary native attacks | Exact owner/item Strength correction; unchanged qualified equipment sweep marks both instances nonlootable | Native signed calculations and private-item single/full/AI passed |
| Lemure defenses | Own DR5 good OR silver; native fire immunity; acid/cold10; native buff + ability descriptor-immunity components | No casting resource/SR | Private trait carrier, no donor mutation | Physical OR/fire/resistance/unrelated energy/buff and ability controls passed |
| Five configured native extraplanar identities | Canonical native fact retained for actual Dismissal/Banishment; exact owning-blueprint/source component filter skips its unselected SR5+CR | Passive; printed adult SR remains separate | Native source-ID cleanup; other native actors and other source grants untouched | Actual Lemure0/native control5/foreign buff17/cleanup0 and Hound own15 PASS4b122ab0; Dretch/Lantern/Trumpet/cold/final affected proof remains; Astral inert |
| Lemure mindlessness | Int0 absent-score surrogate, no ranks/selections/feats or generic undead traits | No intellect granted | Creation-only racial HP/save bases | Actual conscious/CanAct/single/full/native AI proved at Int0 |
| Lemure senses | Printed darkvision60/see-in-darkness references | Passive | Established ordinary-map/passive-sense convention | No new senses/illumination framework |
| Dretch natural routine/defenses | Own Small claws/bite1d4, three primaries; DR5 good OR cold iron, electricity/poison immune, acid/cold/fire10; Toughness | Racial HD2, ordinary attack; native live attributes/feats | Exact owner/item correction and equipment guard; no donor mutation | Compile; native profile/cloud first, full signed attacks/defense consumers pending |
| Dretch Cause Fear | Exact general native clone; native enemy AI eligibility (<=5HD, immunity, available use), priority3 | SLA CL2/DC11 liveCharisma;1 use | Independent resource, no nested summon | Actual CL2/DC11 AI cast, action, real save, Frightened and independent use passed |
| Dretch Stinking Cloud | Exact general native all-creature area; existing20ft ally/owner-area target safety plus immunity/resource eligibility, priority4 | SLA CL2/DC13 liveCharisma;1 use | Native victim/area lifetime measured, no blanket source-loss deletion; sibling independent | Native cast/action/saves/eligible ally/poison-immune/lifetime10/10; safe cloud and player-ally refusal/autonomous Fear5/5 each passed |
| Dretch skills/senses | Perception2ranks and Stealth2ranks use native outsider/size consumers; Escape Artist reference+5 retained | Printed darkvision/limited Abyssal telepathy reference under existing passive/social convention | No rank reallocation or general Mobility compensation | Native profile checks Perception+5/Stealth+9; native break-free rank/class/live-Dex/real roll/routing/ordinary-skill/resource probes passed17fbb4d3; actual live-item negative/reset/scene/resource proof passedf2df6def; held-state/action/TB proof remains |
| Hound armed/natural attacks | Own mwk greatsword+secondary bite, or primary slam+bite; native item/rule/plan consumers | HD6; general live signed Strength, explicit Power Attack off for comparisons | Exact private items/ownership, no duplicate empty-hand slam | Native plans/signed rules PASS; resolved hits/Power Attack/disarm/fallback still mandatory |
| Archon menace (Hound/Lantern, Trumpet candidate) | Private20ft moving exposure, native per-source saves and24h penalty/immunity; one named-penalty stat carrier with source hand-off | Live10+HD/2+Cha+2 racial; Hound16/Lantern13/Trumpet22 | Separate native buff contexts/parameter objects; emitter removes only own grants | Mixed Hound/Lantern saves/overlap/re-entry/live-stat cleanup/reset PASS; actual hit-break/success/24h/TB pending; Trumpet unqualified |
| Hound/Trumpet constant Magic Circle | Exact existing canonical spell/carrier/area/recipient; no independently forked rules | Constant SLA CL6/14 via native owned AbilityExecutionContext | Serialized exact applied fact; shared native area/recipient cleanup | Hound actualCL6/source PASS; defenses/control/overlap/dispel/load still mandatory; accepted published limits retained |
| Lantern/Hound Aid | Exact native spell-like effect with existing useful-target AI consideration | At will CL3/6, native standard action; no duplicate on already aided target | Own ability parameters, native finite buff lifetime | Actual native Lantern and Hound AI/action/CL/no-repeat PASS on original tuples |
| Trumpet prepared Heal (early slice) | Native Heal selected from exact native cleric book's level6 list; own prepared book/private list; bounded healing AI score6 | Genuine book CL14/Wisdom DC21, two prepared level6 copies, native slots/actions | Native book/slot serialization; creation-only preparation; no class stats/domains; no Rest on load/activation | Actual native slot/action/ownership6/6 PASS on36c0ee23; full legal prepared loadout and cold hydration remain mandatory |
| Trumpet other native preparations | Exact native cleric spell/level identities; genuine prepared slots; no SLA/class substitution | CL14/Wisdom;22 native copies across1-7 including the two Heal, plus one private Purge; unfilled named/filtered slots explicit above | Creation-only native preparation/Rest; spent state serializes in the owner book | Native prepared identities/slots/parameters passed on e98af7e7; other spells effect/AI/cold checks pending |
| Trumpet Invisibility Purge | Private genuine level3 prepared spell, manual native cast; automatic all-creature moving area | CL14:70ft/14min, one own slot; no save/SR | Native AddAreaEffect and per-area recipient; native counted invisibility immunity preserves foreign buffs/deadlines | Native commands/slots/radius/ally-enemy/mixed-source/exit/cleanup passed e98af7e7; AI/duration/expiry/persistence remain |
| Trumpet Virtue | Native Spell/Cantrip fact, direct access; no L0 Memorize or SLA relabel; manual | Real native book CL14/live Wisdom, level0/DC15 at adult profile; at will | Native one-temp-HP buff/lifetime; leveled slots independent | Two native commands/actions, actualCL14/nonstacking1 temporaryHP and unchanged leveled slots PASS on aeb25601/37a600d2 |
| Trumpet Magic Vestment | Exact captured native clericL3 parent and native armor/shield variants; prepared/manual | CL14, one own L3 preparation | Native enhancement/buff/item consumers and parent-slot debit | Native self-cast/standard/root-slot debit/+3 actual ArmorEnhancement/source/CL14/50400seconds/other-owner preparation PASS ona7157a52; external SR25 resistance observed88311b5d unchanged |
| Trumpet Aid | Same canonical native spell mapping/helper as Hound/Lantern; autonomous priority4 | At-will SLA CL14/live Charisma, ordinary standard action; native1d8+CL temporaryHP caps at+10 | Own ability; native buff; no prepared slot debit | Actual native Aid AI/standard action/CL14/15 temporaryHP/independent prepared slots PASS on88311b5d |
| Trumpet Banishment | Exact native clericL6 preparation and native all-creature burst; eligible enemy/own slot/HD-budget/safe-point AI priority5 | Genuine CL14/DC21/live Wisdom, one L6 preparation | Native victim effect/lifetime; own book slot, no caster travel | Native ally refusal/useful Aid alternative, clear Banishment AI/real DC21 failed save/removal/one-own-slot debit PASS on88311b5d |
| Trumpet feats/named skills | Private Blind-Fight melee gates/native flat-footed component, no gaze immunity; Persuasive only named Diplomacy/Intimidate; named rank/class mapping | Diplomacy24/Intimidate5/Bluff3 at Cha17, live changes; Perception22 unaffected | Exact owner feature/rule consumers; no donor or global shared-stat mutation | Actual named/live Charisma totals and Perception exclusion PASS on d885b058/aeb25601/37a600d2; concealment/AC/gaze native controls still required |


The fixed `lemure-core` phase now runs rules, a verified rules-to-single reset and one issued single attack. `lemure-full` and `lemure-ai` each require a fresh guarded process/transaction after the observed combat-state residue. No general batch engine, weaker reset or protected-party tick/reset. Request admission remains exact/case-sensitive, one parameter, empty native world, no save and automatic exit. Focused production compile18.2s/tests4.1s: 8 PASS; updated admission/composition subset: 8 PASS; final added native alignment/book/ability/damage-alignment bindings compile PASS. The same verified14d8c574 package served all three separately restored transactions; no source/DLL changes occurred between them. Dretch source compilation19s and12 selected tests4.3s passed; affected icon pin/current-manifest tests3/3 passed. Dretch native observations are recorded above.

Native API inspection showed BuffDescriptorImmunity only handles RuleApplyBuff. Ability-level immunity uses UnitPartSpellResistance and SpellImmunityToSpellDescriptor. The candidate adds only MindAffecting/Poison descriptor eligibility, with actual native Cause Fear/cloud rule checks and susceptible controls; these rule probes are not finite spell casts. The spellbook/HD contracts remain separate.

Owner decision: Hound uses general PF1 attack-role rules with live Strength: armed secondary bite half Strength; unarmed bite and slam are primary and receive full Strength. Disclose printed +2 armed bite and +3/+1 unarmed damage discrepancies; no species-only multipliers. Trumpet owner decision: general PF1 supernatural scaling, DC20 at14HD/Cha17 with live Charisma; disclose printedDC19 discrepancy. No blast implementation yet. Prepared native book APIs are present; printed loadout counts still need an explicitly legal, filtered preparation before implementation. Do not convert this caster into arbitrary SLAs. Owner decision: offensive dismissal/banishment, including Dispel Evil/Holy Word riders, remain legal; the summoned caster?s own teleportation/planar travel is excluded. Owner explicitly retains the published Magic Circle adaptation and its stated contact/SR barrier/existing-control suppression omissions. Use the shared built carrier/area rather than an independently drifting copy, so future common improvements carry to Hound and other aura users. Astral?s additional20ft protective bonuses and lesser globe remain required.
| Lantern light rays/body | Preserve released unit/ray/seven route identities; two ranged-touch light rays, not native wisp electricity | HD2, two rays per full routine; standard must not add extra rays | Native/private attack and DR consumers; own luminous orb sufficient | Standard/full distinction, range/touch/DR and original orb still required |
| Astral adult/hammer/slam | Inert; original painting/native deva donor audited; +2 disrupting hammer versus slam, same-opponent/two-hit stun | HD15; live Strength stunDC25; ordinary Cleave | Exact owned nonloot weapon, native death immunity/attack consumers | Implementation and native qualification pending; no change shape |
| Astral protective aura/SLAs | Distinct20ft +4 protections/lesser globe, shared canonical circle semantics; useful CL13 offense/healing/buffs/removals | Protective auraCL15 distinct from SLA13; limited Heal/Blade Barrier and7/day spells | Per-effect native/owned lifetime and independent resources | Implementation/native checks pending; no archon menace or cleric progression |
| Trumpet blast/weapon | Native14HD cleric plus trumpet/+4 sword; all creatures except archons, safe AI target selection | Live generalSuDC20 atadultCha17; paralysis1d4 rounds | Owned nonloot expensive equipment; future actions close at expiry | Blast/sword mode and actual immunity/AI/native qualification pending |


## Retained failure history and evidence reuse

| Scope/source | Classification and present boundary |
|---|---|
| f2df6def game37 | Escape ownership/live-item negative/rules/reset10 PASS; native Aid14HP triggered overstrict minimum15. Correct cap11-18 reader-only effect PASS retains original aggregate FAIL and later unrun rows |
| 17fbb4d3 game36 | Native escape probes7/7 PASS; null cached-item-reference reset assumption failed; actual live-root/nonloot/scene observer and real live-item negative control passed freshly onf2df6def |
| a7157a52 game35 | Full fixed caster23/23 PASS including actual vestment self-cast/action/root-slot/+3/source/14-hour duration; exact restoration |
| 88311b5d game34 | Aid/Banishment AI/actions/effect/slot proof PASS; vestment positive outcome unmet from observed native SR; positive self-cast proved freshly ona7157a52 |
| d64de36f game33 | Composite enemy/ally relation failed despite score0/slots1; native hostile-summoner ownership/explicit relations passed freshly on88311b5d |
| 37a600d2 game32 | Fixture:15m exceeds native sight; combat/10.5m native lane passed freshly on d64de36f |
| aeb25601 game31 | Native vestment command/action/slot passed, buff/AC absent; effect cause unclassified, bounded SR/application/lifetime capture next |
| d885b058 game30 | Observer fixture omitted combat before action costs; real combat/actions passed freshly on aeb25601, original FAIL retained |
| cafd7160/fdaba297 games29/28 | Observer compared event addition with total skill, and requested base type for exact feature; fresh corrected captures passed later |
| 38b93f5b/11e11b5f | Product L0 Memorize stopped book creation; observer used RTWP bool for TB costs. Correct leveled book/Purge12 and separate controller4 passed freshly on e98af7e7 |
| b80b1250/3544d2ef/42e82b9b | Product canonical circleCL1/parameter alias and duplicate natural slam; actualCL6/private parameters/armed-natural plans/mixed source15 passed freshly on aefe7f57 |
| 0990235c/22791eee | Product menace stacked penalties; one-2 carrier/source hand-off and live-stat survival freshly proved on aefe7f57 |
| Early Lemure | Disabled primary hand, native energy/DR template boundary, raw weapon reach and dirty full-attack controller state corrected. Core31/full5/AI5 passed on14d8c574; full/AI retain separate processes |
| Early Dretch | Native IsAlly omitted player faction in cloud selection; nonenemy selector fixed and safe cloud/Fear5+5 passed c5b430eb. Shared old Cloud Giant control requires fresh final proof |
| Tooling/intake | Earlier stale icon pins/authoring hashes, wrong Heal name, admin preflight refusal and disallowed suffix-wide activation preserved; exact explicit activation/limited tasks/current dependency guards retained |

Admission792 is original fdaba297 limited preflight, logSHA `2e22337eff6961cbd2333da783a64daa75a1bc904dea0255bb7871648292a15c`. Later observer/record-only candidates explicitly reuse it under exact changed-file maps. Current strict package/dispatch remain fresh. Native development rows retain original package/source tuples; no final-artifact relabeling. Later common aura/item/caster changes invalidate affected consumers. No unrelated historical roster replay is selected.

## Transaction, art and final plan

Thirty-seven actual launches/transactions,33 completed development packages, all intake comparisons exact; no save loaded/written. Actual human modv0.0.117, current protected baseline/Working/other saves immutable. Counts are observations, never future restoration authority. No owned game/task/lease/staging/permanent development deployment at this checkpoint. One source writer/runtime owner, limited Howard/Steam640820 guarded execution; source/DLL frozen per transaction. Tooling refusal/failed build attempts remain; total compilation attempts are unmeasured here.

Rules-to-single and injury-residue detection demonstrated limited reset sharing. Single-to-full sharing failed; full/AI stayed isolated. Early book/Purge shared one save-free process and per-case assertions; separate turn-controller Heal used the same immutable artifact in a fresh process. Current caster sequence has owned inputs/queues/resources and explicit ally-blocker transition; safe independent reset remains observed, never assumed from cleanup return. Different profiles and save semantics keep separate guarded transactions. Preserve prepare/cold ON/OFF/cleanup/fresh-absence processes; combine six units in one envelope only if capacity/isolation permit.

Five original paintings/provenance in [generation record](../assets-source/original-icons/expanded-summoning/sprint30-31-generation.json); Lemure/Dretch/Hound/Trumpet exported with real icons, Astral painting inert.133 distinct exports; protected prior pixels unchanged. Borrowed/native bodies are unqualified. Use proven original mesh attachment/owned luminous orb where needed; no neck/articulation research campaign or redistributed game assets/private captures. Native private spell/feat icons retained. No owner aesthetic approval claimed.

Pilot time breakdown uses actual logs only. Backup listing previously took at least173.3s before copying; changing that listing to Verbose retained guards, later backup-stamp/deployment intervals10-12s inclusive measure a different interval. Native result times include fixture setup; useful observation/restoration subdivisions remain unmeasured where not established. No clean speedup or unlike-creature comparison claimed. Final qualification/publication times and comparable S28/S29 assessment remain.

Required next: finish current caster vertical slice, remaining named spell/combat skill/feat consumers, Astral and all six attack/AI/defense/aura/resource/TB/lifecycle checks, usable bodies/icons, each23 new+7 Lantern route, targeted six-unit aura/effect/item/book persistence, standalone/highest-risk combined and relevant old controls. One coherent read-only internal review precedes final freeze. Small combined publisher selector/map must preserve identity/coverage/reuse/artifact/restoration refusals. One COMPLETE combined source/build/package gate; remove only five suppressions after hidden qualification, qualify combined public artifact, merge product and publish next free stable patch with exact wt/s30-31 bytes, download/verify all public assets/tag/source lineage, audit exact restoration, merge separate RELEASED/STOP records without another DLL. STOP before S32.
