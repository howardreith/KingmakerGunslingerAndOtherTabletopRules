# Sprint 24 Deinonychus and Tyrannosaurus coverage

Status: **IN PROGRESS — SOURCE CONTRACT FROZEN** on October 14, 2026.
Both creature keys remain unpublished until the required mechanics, art,
runtime, persistence, route, and cleanup evidence passes. Sprint 25 was not
started.

## Frozen authority and interpretations

The original expanded-summoning charter pages 21–22 define only two Sprint 24
creatures: Deinonychus at Summon Monster/Nature's Ally IV and Tyrannosaurus at
VII. The adult Pathfinder First Edition monster entries and universal monster
rules provide exact mechanics. No Pathfinder 2e, D&D 3.5, young/advanced,
companion, polymorph, mount, or flavor-prose mechanics are in scope.

Primary sources:

- [Deinonychus](https://www.aonprd.com/MonsterDisplay.aspx?ItemName=Deinonychus)
- [Tyrannosaurus](https://www.aonprd.com/MonsterDisplay.aspx?ItemName=Tyrannosaurus)
- [Bestiary dinosaurs](https://legacy.aonprd.com/bestiary/dinosaur.html)
- [Universal monster rules](https://legacy.aonprd.com/bestiary/universalMonsterRules.html)
- [Summon Monster IV](https://www.aonprd.com/SpellDisplay.aspx?ItemName=Summon%20Monster%204)
- [Summon Nature's Ally VII](https://www.aonprd.com/SpellDisplay.aspx?ItemName=Summon%20Nature%27s%20Ally%207)
- [Bleeding Critical](https://www.aonprd.com/FeatDisplay.aspx?ItemName=Bleeding%20Critical)

Deinonychus derivation is exact: 4d8 averages 18, plus 4 × Constitution +4
gives 34 hp; BAB +3 and Strength +2 give +5 primary attacks and +2 primary
damage. Its one foreclaws attack is secondary, so the attack is +0 after the
-5 secondary penalty and deals half Strength, +1. Four purchased ranks split
2/2 between Perception and Stealth; each ranked class skill gains +3, producing
Perception 2 ranks +3 class +1 Wisdom +8 racial = +14 and Stealth 2 ranks +3
class +2 Dexterity +8 racial = +15. Acrobatics/Mobility is +10 from Dexterity
+2 and racial +8; the separate 60-foot-speed jump adjustment yields printed
+22 only for jumping. CMD is 10 +3 BAB +2 Strength +2 Dexterity = 17, with no
borrowed quadruped trip defense.

Tyrannosaurus derivation is exact: 18d8 averages 81, plus 18 × Constitution +4
gives 153 hp; BAB +13, Strength +11, and Gargantuan attack-size -4 give bite
+20. Powerful Bite requires a signed final Strength contribution of `2m`, so
Strength 32 (`m = +11`) contributes +22. Installed Kingmaker 2.1.7b was already
shown by the Sprint 23 IL record to contribute `floor(1.5m)` for a positive
sole natural weapon and `m` for zero or a penalty. The scoped correction is
therefore `2m - native(m)`: +5, +5, +6, +6 for modifiers +9, +10, +11, +12;
zero stays zero; penalties receive the additional signed penalty needed to
reach exactly twice the modifier. It never doubles dice, enhancement, added
damage, critical riders, or any other damage bundle. Perception is 18 ranks +3
class +2 Wisdom +8 racial +6 Skill Focus = +37.

Grab and Swallow Whole have distinct gates. A Gargantuan Tyrannosaurus may grab
a Gargantuan or smaller target after an exact bite hit. It may swallow only a
Huge or smaller target that was held in its mouth at the start of the later
qualifying turn/round, after a new successful grapple check. The successful
swallow bite remains distinct from periodic 2d8+11 interior damage. There is
no immediate/Fast Swallow path.

## Publication contract

| Family | Creature | Base tier | Roots | Quantities through IX | Candidate state |
| --- | --- | ---: | ---: | --- | --- |
| Monster | Deinonychus | IV | 6 | one; 1d3; then 1d4+1 ×4 | Suppressed |
| Nature's Ally | Deinonychus | IV | 6 | one; 1d3; then 1d4+1 ×4 | Suppressed |
| Monster | Tyrannosaurus | VII | 3 | one; 1d3; 1d4+1 | Suppressed |
| Nature's Ally | Tyrannosaurus | VII | 3 | one; 1d3; 1d4+1 | Suppressed |

The candidate adds 18 logical roots, excluding Monster template execution
children. With no unrelated roster change the public totals derive to 107
project creatures; Monster 95 creatures/543 placements; Nature's Ally 95/550;
1,093 generated placements; 29 retained native wrappers; and 1,122 visible
choices. The hidden candidate keeps the current 1,075 published generated
placements and 1,104 visible choices. Monster routes use only the existing
celestial/fiendish policy; Nature's Ally routes remain untemplated.

## Requirement → carrier → proof map

| Requirement | Intended carrier/seam | Required proof | State |
| --- | --- | --- | --- |
| Stable, save-compatible identities | Append-only identity catalog, blueprint ledger, normal registration | GUID/type/collision tests; module OFF load/refusal; ON attach once | Planned |
| Deinonychus four attacks | Primary talon plus talon and bite as additional primary limbs; one foreclaws secondary limb | Live ordinary full attack has exactly four rolls and +5/+5/+5/+0 with +2/+2/+2/+1 Strength | Contract frozen |
| Pounce only on a legal charge | Exact native Pounce fact; ordinary body routine; no project replay | RTWP/TB legal, illegal, aborted, AoO, ordinary single/full, death/interruption, next-command controls | Reuse audit in progress |
| Deinonychus skills/CMD | Exact one-time 0/2/2 land ranks plus narrow racial modifiers; no four-leg fact | Live ranks, abilities, class bonuses, racial bonuses, ordinary Mobility versus jump-only distinction, CMD 17 | Contract frozen |
| Tyrannosaurus one bite | Owned 4d6 bite weapon with live signed Powerful Bite delta | Live +9/+10/+11/+12, zero/penalty, size, buff/debuff, enhancement, added damage, critical multiplication | Contract frozen |
| Critical suite | Exact native Improved Critical (bite), Critical Focus, and Bleeding Critical carriers | 19–20 threat, ordinary confirmation, confirmed piercing/slashing bleed, heal/end controls, negative attacks | Native identity audit pending |
| Bite-only Grab | Existing summon grapple lifecycle, exact primary-limb identity, +4 maneuver | Hit/miss, same-size legal, oversized illegal, immunity/natural 1/20, duplicate callback, independent owners | Reuse audit in progress |
| Later Swallow Whole | Existing held-since-round-start and maintain seam; separate -1 size delta | Same-size grab cannot swallow; later Huge target check succeeds; immediate/failed/wrong-limb controls | Reuse audit in progress |
| Periodic stomach damage | Tyrannosaurus-owned 2d8+11 payload, not Powerful Bite or donor stomach | One application per intended round in RTWP/TB; attribution, escape, source loss, simultaneous predators/victims | Planned |
| Hold/swallow lifecycle | Existing ownership parts with narrow reload distinction | Mouth holds reset safely; already-swallowed native state is not indiscriminately erased; dismissal/death/expiry/escape cleanup | Shared correction audit pending |
| Original theropod presentation | Existing original-asset/export/attach infrastructure after one bounded donor census | Offline clay/silhouette/textured/unlit/camera/extreme poses; live contacts, speed, reach, navigation, crowding, fade/fallback/module lifecycle | Planned |
| All 18 roots | Existing catalog generator and real parent spell paths | Every root, quantity, slot, duration, Monster templates, Nature's Ally untemplated behavior | Planned |
| Affected old consumers | Existing pouncer, Dire Crocodile/Purple Worm, visual owner, Triceratops if shared Strength seam changes | Narrow fresh native regressions, with historical broad evidence reused only after dependency audit | Planned |

## Explicit exclusions and limits

Deinonychus has no Rake, Rend, Grab, Swallow Whole, poison, fear, or Powerful
Charge. Tyrannosaurus has no claws, tail attack, Pounce, trample, roar/fear,
Power Attack, or automatic critical confirmation. Animation cannot generate
gameplay events or move logical position/collision.

The existing accepted limitations are bounded, not expanded:

- `ACTIVE_SUMMON_GRAPPLES_RESET_SAFELY_ON_RELOAD` applies to active mouth holds.
- `SWALLOW_WHOLE_INTERIOR_AC_HP_UNMODELED` leaves printed interior AC 17/hp 15
  in the fidelity record without claiming an attackable stomach implementation.
- `PASSIVE_CREATURE_SENSES_UNMODELED` and `RUN_NATIVE_SUMMON_AI_DISPOSITION`
  may be reused only after their exact existing boundaries are reconfirmed.

`HumanReview` remains `NOT_PERFORMED_NONBLOCKING` unless the owner actually
reviews the finished art.

## Validation order

Focused development uses a named nonempty Sprint 24 filter and production
compilation. Before deployment: source/profile/identity tests, offline asset
review, JSON-shape checks, internal coverage review, and a coherent pushed
candidate. The frozen candidate then receives exactly one fresh
`Invoke-KmgGate.ps1 -Level Sprint` run. Guarded native work uses that exact ZIP
across fresh transactions. Suppression removal creates the public artifact and
requires only affected native/publication reruns plus justified unchanged
evidence. A documentation-only final-records head receives text/JSON/link/
consistency checks only, never another product build.
