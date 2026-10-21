# Sprint 29 adult giant contract

## Current development state (2026-10-21)

The source branch `codex/expanded-summoning-sprint29-cloud-storm-ettin` is a dormant development checkpoint. Three original icons are prepared, the private creature builder and first save-free `profiles` observer compile against the installed Kingmaker 2.1.7b references, and the no-game request preflight passes 700 checks under unelevated Howard. The exact runtime orchestration test passes 218 assertions. The last native input census was research only; no Cloud, Storm, or Ettin product unit has been registered, summoned, or qualified. Public roots and stable v0.0.162 remain unchanged.

Automatic approval review twice rejected the proposed production catalog/bootstrap registration. Its stated concern is exposing three still-unqualified creatures and risking bootstrap/runtime breakage. The attempted patches made no production catalog changes. The worktree's actual Mods, saves, private references, settings, tasks and processes matched its captured intake after the no-game preflight. Hidden-candidate registration, actual combat/magic/body and persistence observations, public publication, merge, stable release, downloaded-asset audit and final records remain blocked at that review boundary; none is claimed PASS. The private Awesome Blow action's obstacle collision rider and native behavior still require proof or repair before release.

Scope: Cloud Giant, Storm Giant, and Ettin as eight untemplated Summon Nature's Ally roots. Registration and publication remain withheld until their source art, equipment, magic, observers, and native behavior are qualified. This document freezes the tabletop target and names the permitted scope decisions; it does not claim implementation or runtime acceptance.

Primary PF1 adult entries: [Cloud Giant](https://www.aonprd.com/MonsterDisplay.aspx?ItemName=Cloud%20Giant), [Storm Giant](https://www.aonprd.com/MonsterDisplay.aspx?ItemName=Storm%20Giant), [Ettin](https://www.aonprd.com/MonsterDisplay.aspx?ItemName=Ettin), and the [legacy Bestiary giant](https://legacy.aonprd.com/bestiary/giant.html) and [ettin](https://legacy.aonprd.com/bestiary/ettin.html) records. These rules establish the target, while installed Kingmaker 2.1.7b references and live observations must establish implementation.

| Adult | Cloud | Storm | Ettin |
|---|---:|---:|---:|
| Type; alignment | Huge humanoid (giant); one NG or NE identity | Huge humanoid (giant); CG | Large humanoid (giant); CE |
| HD; BAB; hp | 16d8; +12; 168 | 19d8; +14; 199 | 10d8; +7; 65 |
| Str/Dex/Con/Int/Wis/Cha | 35/13/23/12/16/12 | 39/14/23/16/20/15 | 23/8/15/6/10/11 |
| Fort/Ref/Will | +16/+6/+10 | +17/+8/+13 | +9/+2/+5 |
| AC/touch/flat | 25/9/24 | 28/10/26 | 18/8/18 |
| Armor/natural/size | +4 chain shirt/+12/-2 | +6 masterwork breastplate/+12/-2 | +2 leather/+8/-1 |
| Land speed; reach | 50 ft.; 15 ft. | 50 ft. (35 in armor); 15 ft. | 40 ft.; 10 ft. |
| Equipped melee | Gargantuan morningstar +22/+17/+12, 4d6+18 | masterwork greatsword +27/+22/+17, 4d6+21, 17–20 | two flails, each +12/+7, 2d6+6 |
| Alternative melee | two slams +22, 2d6+12 | two slams +26, 2d6+14 | none |
| Ranged | rock +12, 2d6+18, 140-ft. increment | masterwork composite longbow +15/+10/+5, 3d6+14, ×3, rating +14 | two javelins +5, 1d8+6 each |
| Perception | +17 | +27 | +12 including racial +4 once |

Cloud's morningstar uses its **Oversized Weapon** permission for that Gargantuan weapon only. It has rock throwing and catching, Great Cleave, Awesome Blow, Improved Bull Rush, Improved Overrun, Intimidating Prowess, Iron Will, Power Attack, scent, and low-light vision. Its listed other skills are Climb +19, Craft (one) +10, Diplomacy +9, Intimidate +26, Perform (string instruments) +8. The one Cloud identity chooses neutral good; no alignment variants are added.

Storm has rock **catching without throwing**, electricity immunity, constant Freedom of Movement, and the printed native armament. Its feats are Awesome Blow, Cleave, Combat Reflexes, Improved Bull Rush, Improved Critical (greatsword), Improved Sunder, Improved Vital Strike, Iron Will, Power Attack, Vital Strike. Its listed other skills are Acrobatics +18, Climb +17, Craft (one) +13, Intimidate +20, Perform (sing) +12, Sense Motive +24, Swim +22. Water breathing, swim speed, and levitation require faithful existing native consumers; neither is simulated by land movement or flight.

Ettin has Cleave, Improved Initiative, Improved Overrun, Iron Will, Power Attack and low-light vision. Superior Two-Weapon Fighting applies to its paired flails and javelins with no ordinary two-weapon attack or damage penalty. Its ordinary full flail routine has four attacks, two at +12/+7 per arm; a standard attack or opportunity does not duplicate. The +4 racial Perception contribution is included once. Handle Animal +8 is printed reference data.

## Public routes

| Creature | Starting SNA tier | Public quantities | Roots |
|---|---:|---|---:|
| Ettin | V | V one; VI 1d3; VII–IX 1d4+1 | 5 |
| Cloud Giant | VIII | VIII one; IX 1d3 | 2 |
| Storm Giant | IX | IX one | 1 |

No Summon Monster placement, alignment variant, template child, list-expansion feat, player racial choice or equipment drop is added. Expected source-derived change, absent unrelated concurrent work: creatures 114→117; Monster 96/545 unchanged; Nature 101/574→104/582; roots 1,119→1,127; wrappers 29 unchanged; visible choices 1,148→1,156. These are cross-checks, not new global-count assertions.

## Spell-like disposition and owner boundaries

| Ability | Printed resource | Required disposition | Native proof needed |
|---|---|---|---|
| Cloud obscuring mist | at will, CL16 | useful distinct direct/AI action | area effect, duration, line of effect, action, no daily decrement |
| Cloud fog cloud | 1/day, CL16 | own limited direct/AI action | independent resource, persistence and duration |
| Cloud levitate | at will, CL16 | use a faithful existing vertical consumer if present; otherwise disclose omission | inspect installed representation once |
| Storm Freedom of Movement | constant, CL15 | active continuous native protection | effects, dispel and summon lifetime |
| Storm call lightning | 1/day, CL15, baseline DC15 | independent limited activation with bounded repeated bolts | activation, bolt cadence, saves, resistance and lifecycle |
| Storm chain lightning | 1/day, CL15, baseline DC18 | independent limited direct/AI action | targets, saves, resistance and lifecycle |
| Storm levitate | 2/day, CL15 | use faithful existing vertical consumer if present; otherwise disclose omission | inspect installed representation once |
| Storm control weather | 2/day, CL15 | filtered as downtime/world-scale utility | no replacement combat grant |

The mission selects no general skill system: named Climb, Craft, Perform, Storm's water-only Swim mapping and Ettin's Handle Animal mapping may be omitted only after checking native consumers, with each actual omission disclosed. Supported Perception, Intimidate, combat skills, saves and attacks remain required. No vertical-flight, aquatic-navigation, weather or global feat project is authorized. Summoning's nested-summon, teleport/planar-travel and prohibited expensive-material effect screens remain. Storm lightning, Cloud fog, constant Freedom of Movement and creature-specific attacks are required combat behavior; an unavailable faithful implementation is an unresolved design boundary.

Summon-safe arrows and javelins follow bounded native ammunition-free availability without collectible supply, transferable equipment or extra actions. Every unit owns its magic uses and equipment across siblings, dismissal, death, disarm, expiry and cold load. No child action outlives its owner deadline; cleanup preserves unrelated casters' and victims' effects.

## Qualification map

| Requirement | Reused check or small new observation |
|---|---|
| Adult chassis, stats, defenses, reach, equipment profiles | Sprint 28 profile/mode native reader extended to the three exact owners |
| Cloud morningstar, slams, rock/catch, Great Cleave | Sprint 28 mode/rock/catch/Cleave reader with Cloud-specific assertions and Hill/Fire controls |
| Storm sword, bow, armor, immunity, criticals, Vital Strike | distinct native action and damage observations; Fire control |
| Ettin single/full/ranged cadence, disarm fallback | exact-owner turn/action/item observations in RTWP and turn based |
| Magic commands, autonomous selection, uses and effect lifetime | new per-ability native observer in both modes; cold-load resource check |
| Art and body usability | source/export hash and UI capture plus body visibility/action observation |
| Item ownership and no loot | Sprint 28 quantity/disarm/death/expiry/cold-load observers extended to new item paths |
| Public spellbook and persistence | eight actual SNA routes with slot, quantity, alignment, lifetime and removal; ON/OFF fresh cold load |
| Compatibility | changed consumers standalone and highest-risk combined; selected released giant controls |

Pixels establish presentation only. Native structured observations establish mechanics. The release candidate must pass the selected full Sprint gate once and reuse that exact ZIP for fresh per-transaction native evidence and publication.
