# Sprint 29 adult giant contract

## Current development state (2026-10-21)

The branch `codex/expanded-summoning-sprint29-cloud-storm-ettin` is BOOTSTRAP_QUALIFIED on exact development source `3418074f`: all three hidden units instantiated, the twelve native profile checks passed, and a separate six-check native modes run observed Ettin's four-flail and two-javelin full-attack plans, distinct item instances and return to the same flails. Both fresh transactions restored the actual laptop intake exactly (Mods 967, saves 118, references 18, settings 83; no owned task or lease). All eight new roots remain suppressed, so the existing player-visible surface and stable v0.0.162 remain unchanged. This qualifies initial chassis, bodies, actions/resources and plans only. Resolved attacks, signature magic, AI, Awesome Blow collision, ownership/disarm, persistence, public routes and release remain pending.

Current hidden qualification includes exact `431b6053` modes (18/18 native queries and plans, including 15-foot Cloud/Storm command reach), exact `40838b10` issued RTWP commands (25/25) and exact `01c09822` turn-based commands (16/16). All eight new roots remain hidden. Autonomous AI, signature magic, Awesome Blow collision, item/disarm safety, persistence, public routes and release remain pending.

Two separate save-free Cloud and Storm direct-magic phases passed focused production compilation plus 709 unelevated no-game request checks. Exact `b8966336` Cloud native execution passed 10/10: distinct standard-action mist and fog created native areas of 960 and 9600 seconds at CL16, and only its Fog Cloud resource was spent. Storm's first execution failed before casting: its direct command had CL19/DC21/spell level9. Installed IL shows `AbilityData.CalculateParams` raises `RuleCalculateAbilityParams`; the cloned spell's `ContextCalculateAbilityParams` alone did not set direct command values. A private exact-owner/ability rule component now sets the printed CL and spell levels. Exact `32f8be8b` Storm native execution passed 13/13: constant defenses on both siblings, CL15/DC15 activation and bolt, CL15/DC18 Chain Lightning, actual native saves and damage, finite bolt state and independent uses. Each fresh transaction restored actual laptop intake exactly. Autonomous AI, repeated bolt cadence, effect expiration, ownership/disarm, persistence and public routes remain. The separate phases prevent Cloud's real all-creature fog from obscuring Storm's test hostile.

The RTWP `commands` phase was connected across the C# request catalog, executable observer, PowerShell guard and documentation. Its preparation passed focused production compilation, 704 unelevated no-game preflight checks and 218 orchestration assertions; these source checks alone did not qualify its nine native attack cases. The separate native result follows below.

The later exact `40838b10` development ZIP passed 25/25 fresh native RTWP assertions (`20261021T0516414148005Z-cb124a7f147b482a9a040fbf2d05588a`). Nine issued attack commands covered Ettin standard/full flails and paired javelins, Cloud morningstar/slams/rock, and Storm greatsword/slams/bow. Each had a visible legal hostile, expected exact weapon events and one native action cost. Natural misses remained misses, and damage packets matched natural hits. The child and task exited zero, the task was removed, and actual intake matched exactly across Mods, saves, references, settings and processes. This is resolved RTWP command proof only; turn-based, AI, magic, Awesome Blow collision, item/disarm, persistence and public routes remain pending.

A bounded turn-based `turns` phase shares the issued-command observer after exact owner turn admission. Its preparation passed focused production compilation and 706 unelevated no-game preflight checks; that source checkpoint carried no turn-based gameplay verdict. The later exact `01c09822` development ZIP passed all 16 native assertions (`20261021T0529230954465Z-62b0798f776d4c47b9c30a2e45b35522`): four fresh owners received active turns and issued the paired Ettin flail/javelin, Cloud morningstar and Storm bow commands with exact weapon events, natural hit/damage packets and one native turn-based action cost each. The child and task exited zero and the laptop restored exactly.

The expanded modes development run on `b20cd991` failed 8 of 18 checks (`20261021T0416186111206Z-199df8fb432842eeb4cbc20ce5a70e13`). Native paired-weapon penalties left Ettin below its printed attacks, a redundant Cloud oversized bonus put its morningstar above the printed attack, and the observer compared the body reach stat instead of equipped melee range. The laptop restored to its fresh intake exactly. The scoped accuracy and observer repairs are under development and have no native PASS yet; the earlier bootstrap verdict remains limited to the observations it actually made.

The `59e4425e` followup resolved the printed attacks and damage for all observed modes, including both Ettin arms, Cloud's Gargantuan morningstar and Storm's greatsword. Four melee checks still failed because the observer compared an internal attack range that includes the Huge owner's collision radius to 15 feet. Its native command radius was 8.3056 m; the observer now checks the command radius against both collision radii plus the printed 15-foot reach. The followup verdict remains FAIL until that changed observer runs; exact intake restoration passed.

The `ec8743db` changed observer showed the deeper private-weapon defect: Cloud and Storm melee command radius remained 8.3056 m, while their measured collision radii plus 15 feet require 6.172 m. The four melee checks failed, and the actual laptop again restored exactly. The next repair adjusts only their private melee type range; it needs a fresh native check before the modes phase can pass.

The `f1233fe2` private type-range attempt failed the same four checks at the opposite side: the native command radius was 4.9528 m (11-foot wielded range). Read-only inspection of installed 2.1.7b IL established why: `BlueprintWeaponType.AttackRange` subtracts four feet for raw values at or below ten, while `UnitDescriptor.GetWeaponRange` adds the owner's `ReachRange`. The raw type value alone cannot produce 15 feet on this Huge chassis. A complete exact-owner, exact-private-item getter correction now retains the 15-foot type input and returns 15 feet only for Cloud and Storm's four melee items; native qualification is pending. Automatic review rejected an earlier partial revert of the type value without this correction; the complete scoped patch was accepted. Historical failures remain FAIL and intake restoration passed.

The exact `431b6053` development ZIP passed all 18 native modes assertions (`20261021T0502429430775Z-581ed55643f046c3b6fbd13964d24598`). Both Ettin flails resolved +12 and both javelins +5 in the native attack-bonus consumer with full printed Strength damage; Cloud and Storm's four private melee modes returned 15-foot wielded range and a 6.172-m native command radius equal to both measured collision radii plus 15 feet. Distinct melee, slam and ranged plans remained separate. The limited task exited zero and the actual laptop compared byte/metadata-equal to its fresh protected intake. This qualifies queried attack and damage rules and native plans, not actual issued combat commands or the remaining gameplay phases.

The four earlier guarded profile attempts retain their original FAIL verdicts: `bb580408` exposed high Will saves, absent Ettin offhand and Storm speed (`20261021T0300317052797Z-666092e652724da4b598a1ac858c7a1e`); `40c38f3f` narrowed the missing hand to native two-hand grip (`20261021T0318229507924Z-55c33328990f4a0384fb7aa213f3aaf0`); `9c005510` showed that creation-time seeding did not retain the item (`20261021T0332196329428Z-c56215462d0a4b65846d650769117d0a`); and `061d3118` had the live paired flails but its observer incorrectly expected an alternate set before switching (`20261021T0343333952137Z-15bcaec64b8e4f428afdf15886d3b84f`). All four restored intake exactly. The current passing profile and modes RunIds and hashes are in `planning/SPRINT29-EVIDENCE.json`.

Automatic approval review twice rejected earlier proposed production catalog/bootstrap registration. Its stated concern was exposing three still-unqualified creatures and risking bootstrap/runtime breakage; those attempted patches made no production catalog changes. On 2026-10-20 the owner explicitly authorized dependency-complete hidden registration with all eight roots suppressed. Combat, magic, persistence, public publication and release remain unqualified. The private Awesome Blow action's obstacle collision rider and native behavior still require proof or repair before release.

Scope: Cloud Giant, Storm Giant, and Ettin as eight untemplated Summon Nature's Ally roots. The stages are: (1) DORMANT source, with no bootstrap connection; (2) HIDDEN_REGISTERED, with dependency-complete builders and all eight routes suppressed; (3) hidden qualification of bootstrap, profiles and affected gameplay; (4) PUBLIC_CANDIDATE, removing only these three suppressions after hidden qualification and testing the actual spellbook routes; (5) stable release after mandatory product, artifact, compatibility and restoration checks. This document freezes the tabletop target and names the permitted scope decisions; it does not claim runtime acceptance.

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

No Summon Monster placement, alignment variant, template child, list-expansion feat, player racial choice or equipment drop is added. At this hidden stage, absent unrelated concurrent work: 117 registered creatures; Monster 96/545 unchanged; Nature 104/582 registered; 1,127 registered roots, eight suppressed, 1,119 published roots plus 29 retained wrappers = 1,148 visible choices. After supported publication, the potential visible total is 1,156 choices. These are cross-checks, not a claim of native or public-route qualification.

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
