# Sprint 37 rules and engine contract — Vrock

This contract keeps four things separate: printed Pathfinder First Edition
requirements, the installed Kingmaker 2.1.7b surface, already accepted project
adaptations, and Sprint 37 decisions proposed for native proof. Source entries:
[Vrock](https://www.aonprd.com/MonsterDisplay.aspx?ItemName=Vrock),
[Demon](https://legacy.aonprd.com/bestiary/demon.html),
[Summon Monster](https://legacy.aonprd.com/coreRulebook/spells/summonMonster.html).
These sources do not prove installed behavior. Charter authority is the
Sprint 37 row of the Expanded Summoning Implementation Charter v1.0
(pages 26–27), including its final acceptance sentence.

## Charter boundary actually quoted

- Objective: the guide-highlighted area-stun demon as a dedicated control-bomb
  summon. Creature scope: Vrock alone. Out of scope: no broad demon dance or
  ritual system.
- Acceptance: the signature area stun has correct targeting, save/DC, use
  limits and ally-safety policy; after it the Vrock remains useful through
  melee **or** approved telekinesis behavior; any dance/spore ability is
  included only if it can be represented faithfully and safely, otherwise it is
  explicitly documented as deferred.

## Printed profile transcribed from the linked entry

CE Large outsider (chaotic, demon, evil, extraplanar), CR 9, 9d10+63 = 112 hp.
Init +6; Perception +23; darkvision 60 ft. AC 22, touch 11, flat-footed 20
(+2 Dex, +11 natural, −1 size). Fort +13, Ref +10, Will +6. DR 10/good;
immune electricity, poison; resist acid 10, cold 10, fire 10; SR 20.
Speed 30 ft., fly 50 ft. (average). Melee 2 claws +13 (2d6+5), bite +13
(1d8+5), 2 talons +13 (1d6+5). Space 10 ft., reach 10 ft.
Str 21, Dex 15, Con 25, Int 14, Wis 16, Cha 16. Base Atk +9; CMB +15; CMD 27.
Feats Cleave, Combat Reflexes, Improved Initiative, Lightning Reflexes,
Power Attack. Skills Fly +12, Intimidate +15, Knowledge (planes) +14,
Perception +23, Sense Motive +15, Spellcraft +14, Stealth +10, Survival +15
(racial +8 Perception). SLAs CL 12: at will greater teleport (self + 50 lbs.),
telekinesis (DC 18); 1/day heroism, mirror image, summon (level 3, 1 vrock 35%).
Special attacks dance of ruin, spores, stunning screech.

Derived live values, not hard-coded print: Fortitude DC 21 is
`10 + 9/2 + Con bonus` and is recomputed from the live Constitution by the
existing `InfernalRules36Policy.ConstitutionDc` contract.

## Creature rules freeze

| Rule surface | Print | Installed engine | Existing accepted adaptation | Sprint 37 decision |
|---|---|---|---|---|
| Chassis | CE Large outsider (chaotic, demon, evil, extraplanar), 9 HD, 112 hp, AC 22 incl. +11 natural, Fort +13/Ref +10/Will +6, DR 10/good, immune electricity and poison, resist acid/cold/fire 10, SR 20, space/reach 10 ft. | `InfernalProfile36`, `OutsiderLevels`, `SummonGiantRacialProfile`, `InfernalDefenses36` (`AddDamageResistancePhysical`, `AddStatBonus` natural armour, `AddSpellResistance`, energy resistance) and the native outsider/evil/chaotic/extraplanar and electricity/poison immunity facts all exist and are released | Generated summon profiles own their stats; genuine native difficulty modifiers stay separate from a printed-profile comparison | Reuse the released Sprint 36 demon chassis helpers unchanged with Vrock numbers and `coldIronOrGood: false` (DR 10/good only). Do not compensate gameplay values to satisfy an observer |
| Movement | Speed 30 ft., fly 50 ft. (average) | Kingmaker exposes exactly one movement speed and the established `Airborne` fact (`70cffb448c132fa409e49156d013b175`); there is no selectable altitude, vertical navigation or wall traversal | Released flyers use one effective speed with `Airborne` and disclose the omitted dual-mode navigation | Effective fly 50 with `Airborne`; the 30-foot land speed is recorded and omitted, never summed or redistributed. No new flight system; shape and animation change no footprint or reach |
| Melee routine | Five primary natural attacks at +13: two claws 2d6+5, bite 1d8+5, two talons 1d6+5 | `Sprint29Weapon` immutable natural-weapon clones, `NaturalBody` primary plus additional limbs, `DeinonychusPrimaryTalonStrength` for full-Strength primaries, `SummonGiantEquipmentOwnership` | Project profiles own attack roles rather than copying a donor; no imported secondary penalties | Three exact weapons (claw 2d6, bite 1d8, talon 1d6) in one body of bite primary plus claw, claw, talon, talon; every limb a full-Strength primary. Native ordinary/single/full/charge/AoO commands with their real action costs. No Pounce, Grab, Rend, Great Cleave or donor spellcasting; no attack-scheduling framework |
| Feats | Cleave, Combat Reflexes, Improved Initiative, Lightning Reflexes, Power Attack | The exact native feats exist in the project's `FactGuids`/special-builder GUID map; the corrected private ordinary Cleave delivery (`SummonGiantCleaveDelivery`) is released | Power Attack contributions are distinguished per limb; private Cleave is ordinary, not Great Cleave | Add the five exact native feats and reuse the released private ordinary Cleave path. No Great Cleave |
| **Stunning Screech (Su)** | Once per hour: all creatures except demons in a 30-ft-radius spread, Fortitude DC 21 (Con-based) or stunned 1 round | `AbilityTargetsAround` radius targeting, `RuleSavingThrow`, `AddCondition(UnitCondition.Stunned)`, `SpellDescriptor.Stun`, native condition immunity, `IAbilityOnCastLogic` (fires once per cast in `AbilityExecutionProcess.PrepareCast`), `AbilityCasterHasNoFacts`, buff durations persisted in saves | The Trumpet Archon blast is the released all-creature area save-or-lose with an exact-identity subtype exemption and an AI ally-safety consideration; the Gorgon breath is the released `IAbilityOnCastLogic` + cooldown-buff + `AbilityCasterHasNoFacts` recharge | Reuse both patterns exactly. Radius 30 ft, live Con-based Fortitude DC, 1-round native stun, standard action, SR does not apply, all creatures eligible for the real effect. Recharge is a hidden 1-hour project buff on that one Vrock: applied once per cast at `PrepareCast`, gating availability through the native `AbilityCasterHasNoFacts`. A cancelled command never reaches `PrepareCast`, so it neither spends nor duplicates; the gate blocks the second activation. Independent summons hold independent buffs and a save/reload keeps the remaining hour |
| Screech exemption | Demons are exempt; non-demons are not | Kingmaker 2.1.7b has no demon subtype fact that the project has ever resolved; "outsider", "evil" and "chaotic" are all broader | The released Trumpet Archon exemption is "exact known archon identities, independent of faction", and the released goblinoid dander exemption is likewise an enumerated native unit set because no subtype fact exists | Exempt the exact enumerated project demon identities (Dretch, Babau, Hezrou, Vrock) by blueprint identity, independent of faction, alignment or wings. The first hidden run also scans the live library for any demon subtype fact and records the result, so the limitation is evidence-based rather than assumed. Non-demon allies are not exempt; AI ally-safety is separate from eligibility |
| Screech AI | — | Native brain, `Consideration`, memory/LOS, `UnitPartSpellResistance` | `TrumpetBlastEligibility` is the released safe-placement consideration: score 0 while any non-exempt non-enemy is inside the radius, and require at least one visible, in-radius, non-immune enemy | Reuse that consideration shape with the Vrock's demon exemption and stun immunity. The player's own targeting is unchanged; only AI selection is conservative. No unrelated party actor is ticked or controlled |
| Heroism (1/day) | CL 12 self heroism | The native Heroism graph exists and `ConfigureOwnedSpell35` plus a one-use `BlueprintAbilityResource` are the released once-per-summoning pattern (Hezrou Blasphemy) | Daily resources are per summoning; AI must not re-buff a satisfied state | One native Heroism at CL 12 on its own single resource, self-targeted, with AI admission refusing while the effect is already active |
| Mirror Image (1/day) | CL 12 self mirror image | The native Mirror Image graph exists | Same as above | One native Mirror Image at CL 12 on its own separate single resource, with the same already-active AI refusal. Resources are independent of Heroism's |
| Greater teleport, summon | At will greater teleport (self + 50 lbs.); 1/day summon 1 vrock 35% | Installed travel and summon graphs exist but are excluded for summons | The released Summon Monster contract removes planar travel, self-teleportation and nested summoning from every summoned creature | Omit both precisely through the existing summon policy. No new blanket spell-like-ability policy |
| **Telekinesis (at will, DC 18)** | Sustained force, combat maneuver at range, or violent thrust | The 103 MB installed `Assembly-CSharp` IL contains **zero** occurrences of "telekines" in any form: no component, rule, command or class. There is no object-physics, object-launching, or ranged-maneuver-delivery consumer, and no retained project or census evidence of a Telekinesis graph | Summons receive no new frameworks built solely to satisfy one entry; an unsupported form must be named, not substituted | **Explicitly omitted, no substitute.** All three printed modes need machinery the engine does not have. The charter accepts "melee **or** approved telekinesis"; the mandatory melee leg is delivered in full, so acceptance does not depend on it and no new gameplay decision is required. Named as an omission in the release notes, not advertised as a partial spell |
| **Spores (Ex)** | Free action once every 3 rounds: adjacent creatures take 1d8, plus 1d4 per round for 10 rounds as vines grow | `UnitCommand.CommandType.Free` exists; `AbilityTargetsAround`; `AddFactContextActions.NewRound` is the native per-round tick the released Sprint 34 Infernal Wound already uses for finite ongoing damage; buff duration supplies the finite 10 rounds; `AbilityCasterHasNoFacts` plus a cooldown buff supply the 3-round cadence | Infernal Wound is the released finite ongoing-damage consumer; the Hezrou stench is an aura and is *not* reused here | **Implemented with proof.** A real free action with a 3-round project cooldown buff, 10-ft radius (the Vrock's own printed reach, which is its adjacency), one immediate 1d8 packet and a distinct 10-round vine buff dealing 1d4 each round through the native new-round action. No save and no spell resistance, matching the printed extraordinary ability. Not poison, not disease, not an aura and not a cloud area: no poison descriptor, so poison immunity does not apply, and the vine buff is extraordinary and not spell-derived, so Dispel Magic does not remove it. Damage uses the native damage rule, so real resistances and immunities apply |
| **Dance of Ruin (Su)** | Dance and chant as a full-round action; at the end of 3 rounds a wave deals 5d6 electricity to everything within 100 ft, DC 17 Reflex half | `Kingmaker.UnitLogic.Commands.Base.UnitCommand.CommandType` has exactly four members — Free, Standard, Swift, Move. There is **no full-round action type at all**, and no multi-round channelled-command or interruption contract anywhere in the installed surface | The charter's own acceptance sentence permits deferral when faithful, safe representation is unavailable | **Explicitly deferred**, with the capability reason above. A buff that silently detonated on its third round would misrepresent both the action commitment and its interruption, and would let the Vrock act freely for three rounds and still detonate — the unsafe representation the charter boundary excludes. No broad demon dance/ritual subsystem, damage substitute or free extra action is added |
| Skills and senses | Fly +12, Intimidate +15, Knowledge (planes) +14, Perception +23, Sense Motive +15, Spellcraft +14, Stealth +10, Survival +15; racial +8 Perception; darkvision 60 ft.; telepathy 100 ft. | Only some of these have supported summon consumers; `SummonGiantRacialProfile` carries Perception/Stealth/Mobility ranks and `AddStatBonus` carries racial bonuses | Released profiles carry only skills with real consumers and record the rest | Carry the racial +8 Perception and the Perception ranks through the released consumers. Skills with no supported summon consumer (Fly, Knowledge (planes), Spellcraft, Sense Motive, Survival, Intimidate) and telepathy are recorded as unrepresented, consistent with every released sprint; no new skill framework |

## Public surface

Exactly one project creature, stable key `vrock`, three untemplated Summon
Monster routes and no Nature's Ally entry, template child or variant:

| Tier | Quantity |
|---|---|
| VII | One |
| VIII | 1d3 |
| IX | 1d4+1 |

Source-derived expectation if no unrelated catalog change occurs:

| Surface | Reviewed baseline | Sprint 37 expected |
|---|---:|---:|
| Unique project creatures | 130 | 131 |
| Monster entries / placements | 108 / 594 | 109 / 597 |
| Nature's Ally entries / placements | 105 / 588 | 105 / 588 |
| Generated placements | 1,182 | 1,185 |
| Retained wrappers | 29 | 29 |
| Visible choices | 1,211 | 1,214 |

## Requirement map

| Requirement | Existing consumer | Actual gap | Smallest change | Selected proof |
|---|---|---|---|---|
| One creature, three Monster-only roots | Append-only catalog, donor catalog, visibility filter, generated summon graphs | No `vrock` identity or dependency exists | Append one catalog row at Monster tier 7, one donor row, one visibility route assertion and the identity specs | Source-derived roster counts plus a final-byte public run of all three parents, children, quantities, costs, CL 20 durations and cleanup |
| Large demon chassis, five primaries, feats, airborne presentation | Sprint 36 demon chassis helpers, `Sprint29Weapon`, `NaturalBody`, private Cleave, `Airborne` | Vrock numbers, a five-limb body and the airborne fact are absent | One new builder partial reusing the released helpers | Hidden profile/body run plus native attack-mode observations |
| Stunning Screech with real targeting, DC, limits and ally safety | Trumpet blast area/exemption/consideration, Gorgon `IAbilityOnCastLogic` cooldown + `AbilityCasterHasNoFacts` | No Vrock screech, demon exemption set, or hour recharge exists | One rules file combining both released patterns | One real cast: action cost, live DC, susceptible and immune controls, radius boundary, natural expiry, refusal and recharge state, two-owner isolation, AI fallback, in RTWP and turn-based |
| Heroism and Mirror Image, each once per summoning | `ConfigureOwnedSpell35`, `BlueprintAbilityResource`, released self-buff AI admission | Both abilities, their separate resources and the already-active refusal are absent | Two owned spells on two independent single resources | Native run: real casts, independent resource debits, no recast while active, melee still chosen |
| Spores every three rounds with finite vine damage | Infernal Wound new-round tick, free command type, cooldown gate | No spores ability, cadence or vine buff exists | One ability plus one 10-round vine buff on the released tick | Native run: free action cost, immediate packet, exactly ten vine ticks, cadence refusal, no poison/dispel interaction |
| Distinct original body and real icon | Blender 4.5 procedural body pipeline and the procedural icon renderer, both project-owned and authorized | No Vrock geometry, albedo or icon exists | One new body generator reusing the measured rig pipeline; one new icon builder in the existing renderer | Hash/provenance records plus the native presentation run |
| ON/OFF persistence | Released three-process Working envelope | New screech cooldown, two resources and vine state need coverage | Extend only the Sprint 37 persistence reader/dispatch | Six fresh final-candidate processes with exact Working and installation restoration |
| One stable exact-byte release | Full Sprint gate, reproducibility, guarded publisher | Sprint 37 binding and records are absent | A named selector and strict preflight | Complete gate, identical reproduction, publisher refusals, preflight, public download and lineage verification |

## Carry-forward task 2B

Confirmed by native inspection and corrected in commit
`Restrict autonomous Babau Dispel to effects that help its enemy`:
`ContextActionDispelMagic.TryDispelBuff` filters only on `IsNotDispelable`,
the from-spell buff type and the maximum spell level, and
`BlueprintAiCastSpell` adds only locator, target-checker and range
restrictions with no `IsAvailable` of its own. Neither distinguishes a
beneficial buff from a harmful one, so the Babau's own selector now does,
from `BlueprintBuff.Harmful` and whether the effect's caster is an enemy of
its owner. Native requalification of the changed selection, its
beneficial-target neighbour and the attack fallback is grouped into this
sprint's Babau follow-up observation.
