# Expanded Summoning fidelity matrix

## Current Sprint 16 disposition, October 5

Sprint 16 remains **NOT QUALIFIED**, not published. Exact laptop candidate
`197840577cb995beea726a34f5dc6ae9dd9674d9` passed all prelaunch gates
(2019 full), then 11/11 smoke, **208/209 main FAIL**, 20/20 crowd,
9/9 prepare, 9/9 cleanup and 5/5 absence.
[Exact review](EXPANDED-SUMMONING-SPRINT16-LAPTOP-NATIVE-UPDATE-BATCH-REVIEW.md).

The complete combat matrix passes 55/55, including all four repaired Dire
tail contacts. Mechanics 37/37, speed 19/19, icons 9/9, all twenty routes,
UI/fallback/resource controls pass. Sole failure: Crocodile active-source-death
observation paused for 343 frames/0.04 native seconds. Fixture-only correction
must obtain a real post-death update; no assertion waived or rule changed.
Actual installation and clean working-save absence verified; no complete gate.

Crocodile identity and fourteen published roots are unchanged. All six Dire
roots remain withheld: 976 registered / 970 published / 29 native wrappers /
999 visible choices. Land skills remain Crocodile +8/+5 and Dire +14/+0 with
zero Mobility ranks. Run is an explicit evidence-backed engine omission, not
a silent substitution; aquatic skills/Hold Breath are outside land-use scope.
The deterministic original models, four packaged assets and qualified rig
binding exist; contact qualification remains incomplete. No visible identity
or icon was reauthored in this laptop checkpoint.

`OwnerAcceptedAdaptation: SWALLOW_ELIGIBLE_TARGET_ELSE_DEATH_ROLL`
remains the deterministic AI/RTWP adaptation, not exact tabletop choice.
`OwnerAcceptedEngineLimitation: SWALLOW_WHOLE_INTERIOR_AC_HP_UNMODELED`
remains activated by the recorded native census; no interior target/AC/HP/
cut-free carrier is claimed. Purple Worm shares that engine-wide limitation
without reopening its qualified mechanics. Preserve
`PASSIVE_CREATURE_SENSES_UNMODELED` and
`ACTIVE_SUMMON_GRAPPLES_RESET_SAFELY_ON_RELOAD`.
`HumanReview: NOT_PERFORMED_NONBLOCKING`.

Historical sections below describe earlier checkpoints, not current counts,
qualification or branch authority.

## Sprint 15 closeout, 2026-10-03: COMPLETE AND PUBLISHED

The Giant Ant (Drone) and the Giant Stag Beetle are published. The visible
surface is **970 published generated placements plus 29 retained native
wrappers, for 999 visible choices**, derived from source. **Nothing is withheld
anywhere** - for the first time since Sprint 9 the registered and published
surfaces are the same number, so no creature in this phase is registered and
hidden.

### What the guarded review proved

Six scenarios, 139 assertions, every run restoring the live tree exactly to
`216A9DC2B8E95CD644BA3CADC69A638463C25E60F40A11F8D4B2065C69D5AAF3` with
restoration verified.

| Scenario | Assertions | Candidate |
| --- | --- | --- |
| mechanical pack | 61/61 | `c8cb25ce` |
| rules pack, with twelve combat-mode cells | 62/62 | `52baae65` |
| visual lifecycle | 8/8 | `52baae65` |
| working-save prepare / verify-cleanup / verify-absent | PASS / PASS / PASS | `52baae65` |

The Drone is the Soldier with the advanced simple template, and the engine
produced it: every ability score measured exactly four higher than a Soldier
spawned in the same run except Intelligence, which the template excludes. Its
poison needed no second graph - the shared Constitution-scaled policy turned
the advanced Constitution of 21 into the printed Fortitude DC 16 unaided, with
1d2 Strength over four exposures cured by one save, delivered only by a sting
that actually wounded, and one attack producing exactly one application
carrying that same difficulty class. Its Perception derived to exactly 7 from a
racial +4 and the advanced Wisdom, with no ranks and no unprinted flat bonus.

The Giant Stag Beetle read its printed CMD 20 and 28 against trip exactly, is
Large with one 2d8 bite and nothing else on its body, carries its own unit type
rather than the Fire Beetle's or the Giant Spider's, and carries none of the
Fire Beetle's luminescence. Its trample ran through the carrier the Sprint 11
ungulates qualified, at its printed DC 17, dealing damage to the hostile and
none to the allied caster.

Both creatures attack through the command a player's click produces, in RTWP
and in turn-based combat: twelve cells, and each two-limbed creature
demonstrated its own bite-and-sting separation rather than borrowing another
creature's evidence. Both were summoned in quantity as well as singly - a 1d3
and a 1d4+1 command for each - with the project visual attached on every body
of every multi-body cast, and the Giant Spider cast as itself came up with no
swap attempted on it at all.

### One measurement corrected, twice

The Drone's combat-manoeuvre defence first measured 15 and 23 against a derived
17 and 25. The engine's own component breakdown settled it: a freshly summoned
creature has not acted, so Kingmaker treats it as flat-footed and denies it its
Dexterity bonus, which is exactly the two points. The creature was right; a
single total could not say so, because the four insects before it all had
Dexterity 10 and read correctly either way.

The second attempt then asserted the engine's split of a size modifier, and a
Large creature's printed +1 arrives in Kingmaker as `size=2` with `misc=-1`.
That is the same number expressed differently and no source text constrains the
split, so the check now asserts only quantities the rules name - base attack
bonus, Strength, the net size modifier, Dexterity when it is not denied, the
printed total, and the eight-point multi-legged difference - and records every
component as evidence without requiring it.

`HumanReview: NOT_PERFORMED_NONBLOCKING`.

## OwnerAcceptedEngineLimitation: PASSIVE_CREATURE_SENSES_UNMODELED

**Accepted by the owner on 2026-10-03.** Kingmaker models none of **Scent**,
**Darkvision** or **Low-light Vision**, and this project omits them wherever a
bounded native audit proves no faithful carrier exists.

### The evidence the acceptance rests on

The audit searched every component on every blueprint the running game had
loaded - the whole library, not a filtered subset - and reported
`senseComponentCensus=Blindsensex74/OverrideVisionRangex23`. Scent has no
carrier of any kind. No enum reachable from `BlueprintUnit`, `UnitEntityData` or
`UnitDescriptor` holds a darkvision value: `visionEnum=<none>`.
`OverrideVisionRange` is a sight radius in metres, carried by 23 blueprints, of
which the sampled six are Vordakai and Horagnamon boss units setting 50 m
against a live unit's 8.5.

### What the label permits

A creature may publish or qualify without these three passive senses. The
omission is recorded here once and referenced from the affected creature rows.
**A creature is never kept hidden for one of these three alone.**

### What it forbids

- No substitution of `AddBlindsight`, `UnitPartBlindsense`, tremorsense or any
  materially different sense. Blindsight is a real and different rule, and this
  project implements it exactly for the Dire Bat; that implementation stays.
- No use of `OverrideVisionRange` as darkvision. It changes general detection
  range in all conditions and would hand a creature an advantage no stat block
  grants it.
- No scent, darkness, stealth-detection or perception subsystem in this phase.
- No claim anywhere that the omitted traits work.
- No use of this label to waive any other sense, combat ability, immunity,
  skill or signature mechanic.
- The limitation is never re-marked BLOCKED.

This is a conservative engine limitation, not a balance adaptation.

### Creatures currently governed by it

Giant Ant (Worker), Giant Ant (Soldier) and Giant Ant (Drone) for Scent and
Darkvision; the Fire Beetle and Giant Stag Beetle for Low-light Vision and
Darkvision respectively; and already-published creatures carrying the same
engine gap, the wolves among them. It applies to later Phase 2 creatures whose
stat blocks list these passive senses.


## Sprint 14 closeout, 2026-10-03: COMPLETE WITH OWNER-ACCEPTED ENGINE LIMITATION

All three Sprint 14 insects are published. The published surface is **952
generated placements plus 29 retained native wrappers, for 981 visible
choices**, derived from source. The Fire Beetle's 18 placements went out on its
own qualification; the Giant Ant Worker's 16 and Soldier's 14 followed once the
owner accepted `PASSIVE_CREATURE_SENSES_UNMODELED`, which was the only thing
holding them - their mechanics had already qualified 174/174 across six guarded
scenarios on candidate `cda8d72c`.

Nothing was implemented to earn that publication. The ruling records an engine
gap rather than closing one: no substitute sense was added, no vision range was
touched, and no record claims the omitted traits work.

What remains withheld is Sprint 15's pair - the Giant Ant Drone's 12 placements
and the Giant Stag Beetle's 6 - registered ahead of their own qualification the
way every sprint before them did. The Drone carries the same unmodelled senses
and is no longer held for them; it waits on its own gates alone.

`HumanReview: NOT_PERFORMED_NONBLOCKING`.

## Sprint 14 publication, 2026-10-03: the Fire Beetle only

The published surface is **922 generated placements plus 29 retained native
wrappers, for 951 visible choices**, derived from source. Sprint 14 registered
48 placements across three insects and publishes 18 of them - the Fire Beetle,
reachable from both parents and splitting nine each way. The 30 placements of
Giant Ant (Worker) and Giant Ant (Soldier) stay withheld.

The ants are not withheld for want of qualification. Their mechanics passed on
the closing candidate: printed Perception +5 exactly with zero class ranks, the
printed trip defence, a sting that is its own weapon type, a grab on the bite
alone carrying exactly +4 to the grapple and nothing to the trip, an injury
poison that a wound delivers and a hit reduced to zero damage does not, vermin
mind-affecting immunity proved by a native mind-affecting buff they refuse and
the caster accepts, and both combat modes driven through the command a player's
click produces. What they cannot have is their printed scent.

Kingmaker has no scent mechanic. The bounded native audit searched the whole
loaded blueprint library, not a filtered subset, and found no component that
could express it; the engine's only sense plumbing is `AddBlindsight` with
`UnitPartBlindsense`, which this project already uses for the Dire Bat and which
is a different rule. Their printed darkvision 60 ft is equally unrepresentable:
no enum reachable from `BlueprintUnit`, `UnitEntityData` or `UnitDescriptor`
carries a darkvision value, and `OverrideVisionRange` is a sight radius in
metres - six Vordakai and Horagnamon boss units set 50 m against a live default
of 8.5 m - so using it for darkvision would hand the ants more than double
their detection range in all conditions, an advantage no stat block grants
them. In an engine with no darkness that is inventing a mechanic rather than
implementing one.

The Fire Beetle's own senses requirement was the opposite case and is met
exactly: its stat block prints **no** darkvision, nothing was inherited from the
Vermin racial class, the project unit type or the Giant Spider donor, and the
live unit reads `darkvision=False` with no carrier. Its printed low-light vision
has no representation either, and no mechanical consequence in an engine with no
light model, so nothing about the creature is softened by its absence.

**This is not recorded as an accepted engine limitation.** No
`OwnerAcceptedEngineLimitation:` label is created here, because only the owner
converts a proven barrier into one. The two castes are held pending a single
ruling, recorded as a blocker with the engine evidence. The same ruling governs
Sprint 15's Giant Ant Drone, which prints scent too; the Giant Stag Beetle does
not and is unaffected.

Current Phase 2 Sprint 11 disposition (2026-09-30): Sprints 9-11 are
internally technically qualified. Aurochs, Bison, Rhinoceros and Woolly
Rhinoceros are published at all 48 authorized placements. The current
published surface after the Sprint 13 publication covers Sprints 9-13 with
904 published generated choices and 933 total choices including the 29
retained native wrappers; nothing registered is withheld. The immutable
v0.0.141 build contained 832 and 861; see
`EXPANDED-SUMMONING-PHASE2-INVENTORY-RECONCILIATION.md`.
The four ungulates have qualified original icons/views, exact natural profiles,
ordinary Trample or Powerful Charge, direct and quantity behavior, lifecycle,
player paths and menu publication. Aurochs and Bison use the faithful bounded
Stampede implementation: three allied Stampede holders must each execute their
own registered Trample in the same round while mutually adjacent. The acting
group then gains same-size eligibility and +2 DC; idle, queued, interrupted,
stale-round or separated units do not count. Trample uses the disclosed
automatic AoO-first Kingmaker adaptation exactly specified by the owner.

Stirge's replacement session-only attachment is owned only by the Stirge;
the prey receives no native grapple part or movement/action condition. The prey
moves, acts and attacks normally while the separately targetable Stirge follows
at a bounded offset. A standard-action Remove Stirge ability with its own icon
chooses the better current CMB or Mobility modifier before rolling against CMD
5. Both methods, failure/success, and the printed +8 maintain bonus passed.
Death, dismissal, expiry, prey death, short translocation, area transition,
module disable and reload clean up without link, ability, condition, collision
or view residue. Four actual Constitution drains detach automatically.

Filth Fever is the disclosed bounded disease adaptation. The primary Paizo
Stirge stat block limits exposure to one check per victim from each particular
Stirge; zero Constitution damage neither rolls nor consumes that check. The
final evidence set is creature review `20260929T1714055122835Z` (12/12),
turn-based `20260929T1824346229028Z`, RTWP `20260929T1827582364082Z`,
inventory `20260929T1850489650251Z` (50/50), and player path
`20260929T1903306672213Z` (834/834 generated roots and 29/29 native wrappers).
The earlier attachment, removal and reload evidence remains detailed in the
Stirge row below. Exact live-install restoration passed after every run.
Final Sprint 11 evidence is rules `20260930T0422334006971Z` (58/58), visual
contracts `20260930T0426570686599Z` (15/15), lifecycle
`20260930T0430006142872Z` (7/7), inventory `20260930T0457205706691Z`
(50/50), player path `20260930T0508037596496Z` (10/10), and the 15/15
working-save trio `20260930T0524086567797Z` /
`20260930T0528280228527Z` / `20260930T0532419916125Z`. Owner visual approval
remains pending and nonblocking.

Status: release-qualified on final native qualification source
`5205805eab3fe0115d6888c53bce73c80474d1b7`. The complete roster passed the
final-live structural observer, all 153 production summon commands, the
67-unit visual-contract matrix, enabled and disabled active-summon persistence,
all 16 module states, and every required compatibility profile. Conservative
adaptations and omissions remain identified in the row that owns them.

Final evidence key: structural run
`20260812T1327062696968Z-bd09acfba08942df8f7c42e5c70252f4`; mechanical run
`20260812T1330147883834Z-ec8896f1d65b43e0913a6bea7cba4405`; visual run
`20260812T1151394827201Z-add45a04f5de44c1a39e3251f7ff0778`; enabled and
disabled persistence runs are recorded in `EXPANDED-SUMMONING-STATE.json`;
the 16-state matrix passed on its recorded qualified source, and all eight
required final compatibility launches passed on the final native source.
“Runtime PASS” in a row means its production summon
path was exercised; it does not imply that an explicitly omitted tabletop
ability was implemented.

Each catalog row will record family/tier, creature and template/alignment policy,
native reuse or donor GUID/name/view, frozen KMG unit and ability identities,
size/reach/speed/movement, ability scores and combat statistics, attacks,
defenses, senses, feats, special abilities, removed donor behavior, deviations,
and structural/runtime/visual/compatibility evidence.

No omitted or conservatively adapted mechanic will be described as implemented.

Primary rules references:

- [Lantern Archon](https://www.aonprd.com/MonsterDisplay.aspx?ItemName=Lantern+Archon)
- [Mephit](https://www.aonprd.com/MonsterDisplay.aspx?ItemName=Mephit)
- [Elemental](https://legacy.aonprd.com/bestiary/elemental.html)
- [Boar](https://www.aonprd.com/MonsterDisplay.aspx?ItemName=Boar)
- [Leopard](https://www.aonprd.com/MonsterDisplay.aspx?ItemName=Leopard)
- [Monitor Lizard](https://www.aonprd.com/MonsterDisplay.aspx?ItemName=Monitor+Lizard)
- [Cheetah](https://www.aonprd.com/MonsterDisplay.aspx?ItemName=Cheetah)
- [Crocodile](https://www.aonprd.com/MonsterDisplay.aspx?ItemName=Crocodile)
- [Dire Bat](https://www.aonprd.com/MonsterDisplay.aspx?ItemName=Dire+Bat)
- [Wolverine](https://www.aonprd.com/MonsterDisplay.aspx?ItemName=Wolverine)
- [Dire Boar](https://www.aonprd.com/MonsterDisplay.aspx?ItemName=Dire+Boar)
- [Dire Wolf](https://www.aonprd.com/MonsterDisplay.aspx?ItemName=Dire+Wolf)
- [Grizzly Bear](https://www.aonprd.com/MonsterDisplay.aspx?ItemName=Grizzly+Bear)
- [Lion](https://www.aonprd.com/MonsterDisplay.aspx?ItemName=Lion)
- [Pteranodon](https://www.aonprd.com/MonsterDisplay.aspx?ItemName=Pteranodon)
- [Dire Lion](https://legacy.aonprd.com/bestiary/lion.html)
- [Ankylosaurus](https://legacy.aonprd.com/bestiary/dinosaur.html)
- [Dire Bear](https://www.aonprd.com/MonsterDisplay.aspx?ItemName=Dire+Bear+%28Cave+Bear%29)
- [Smilodon](https://www.aonprd.com/MonsterDisplay.aspx?ItemName=Dire+Tiger+%28Smilodon%29)
- [Elephant](https://www.aonprd.com/MonsterDisplay.aspx?ItemName=Elephant)
- [Mastodon](https://legacy.aonprd.com/bestiary/elephant.html)
- [Roc](https://legacy.aonprd.com/bestiary/roc.html)

## Native dedicated summon reuse

| Entries | Families/tiers | Implementation | Sanitization | Evidence |
|---|---|---|---|---|
| Air, earth, fire, water elementals; Small through Elder (24 unique units) | SM II/IV/V/VI/VII/VIII and matching SNA tiers | Exact Owlcat dedicated summon donor for every element/size; KMG freezes one shared family-neutral unit identity per creature | XP/loot/inventory/campaign surfaces stripped; class spell arrays empty; no celestial/fiendish template applied | Structural, production-cast, 67-unit visual matrix, persistence, and required-profile PASS |
| Air, earth, fire, water mephits (4 unique units) | SM IV and SNA IV | Exact Owlcat dedicated summon donor preserves Small outsider chassis, two claws, breath weapon, elemental facts, DR 5/magic, and native spell-like abilities | XP removed; no summon/conjure or planar-travel fact appears in selected donor facts; source component arrays independently cloned | Structural, production-cast, 67-unit visual matrix, persistence, and required-profile PASS; Fire Mephit breath dealt actual fire damage |

The native mephit units grant unconditional Fast Healing 2. Tabletop restricts
fast healing to element-specific environments. Kingmaker has no safe local
environment predicate was proven safe. KMG therefore retains Owlcat's native,
unconditional Fast Healing 2 as a documented practical adaptation; the final
mechanical and compatibility runs passed with this behavior.

## Low-tier natural reconstruction

| Creature | Families/tiers | KMG unit | Delivered chassis/offense | Deviation | Qualification |
|---|---|---|---|---|---|
| Dire Rat | SM I; SNA I | `e7e43c80489a48e1b562fe6ac11c33ee` | Small animal 1; 10/17/13/2/13/4; speed 40; bite 1d4; Weapon Finesse, Skill Focus (Perception), four-leg trip defense; exact damaging bite invokes DC 11 Fortitude and native Filth Fever | Climb/swim omitted; compact original rat silhouette still pending | Registered and hidden; focused injury-disease, miss/replay and quantity-source runtime PASS; full Sprint 12 visual/player/persistence matrix pending |
| Dog | SM I; SNA I | `e8c90cb29374455cb6301e4fa7d1f837` | Small animal 1; Str 13/Dex 13/Con 15/Int 2/Wis 12/Cha 6; speed 40; bite 1d4; Perception focus | None structurally identified | Registered and hidden for Sprint 12; inherited structural profile PASS; distinct visual and full live matrix pending |
| Eagle | SM I; SNA I | `7383db28c1d74dce98533ddc257a2e3c`; instance-local original Eagle feathered mesh on the Giant Eagle flying rig | Small animal 1; 80-foot airborne movement; bite and two 1d4 talons; Weapon Finesse; view scale 0.30 | Separate 10-foot ground speed omitted | Phase 2 Sprint 9 visual attachment 2/2, Roc isolation 2/2, 81/81 visual contracts and lifecycle retry live PASS; player-path teardown fault, sprint-specific motion/contact and persistence pending |
| Poisonous Frog | SM I; SNA I | `e1a8e5e206154dd48b6aca1d4262e8e7` | Tiny animal 1; bite 1; native six-tick 1d2 Constitution poison, Fortitude, one save cures | Swim movement omitted | Structural/runtime/visual/persistence/profile PASS; poison graph structurally qualified |
| Giant Centipede | SM II; SNA I | `baf9e8f829e9410db8f3d200bb62a2c6` | Medium vermin 1; speed 40; bite 1d6-1; native six-tick 1d3 Dexterity poison; eight-leg trip defense | Int 1 represents absent Intelligence; climb omitted; native poison lacks the tabletop +2 racial DC bonus | Structural/runtime/visual/persistence/profile PASS; poison graph structurally qualified |
| Giant Spider | SM II; SNA II | `4a3cd49e751448c8b8836485b262fdf1` | Medium vermin 3; bite 1d6; native four-tick 1d2 Strength poison; natural armor +1; native 60-ft blindsight for tremorsense, native web immunity, bounded 50-ft ranged Web (Reflex, native web-grappled state up to ten rounds, two uses per summoning) (Sprint 6) | Int 1 represents absent Intelligence; web, climb, and tremorsense omitted because no bounded contract was proven | Structural/runtime/visual/persistence/profile PASS; poison graph structurally qualified |
| Goblin Dog | SM II; SNA II | `f1584066792a436fa3a5ba0b3731b481` | Medium animal 1; speed 50; bite 1d6+3; Toughness; disease immunity; exact damaging bite invokes DC 12 Fortitude and one nonstacking day at -2 Dexterity/-2 Charisma, removable by positive magical healing or native Remove Disease | Exact native Goblin type is exempt as a disclosed bounded adaptation because no broader Goblinoid subtype contract exists; original silhouette pending | Registered and hidden; focused immunity/allergy/removal/native-Goblin/quantity-source runtime PASS; full Sprint 12 visual/player/persistence matrix pending |
| Hyena | SM II; SNA II | `ec12fab8be5c412d8ee824d15a6621d0` | Medium animal 2; speed 50; bite 1d6+3 with native trip; Perception focus | Original hyena silhouette pending | Registered and hidden for Sprint 12; inherited structural profile PASS; distinct visual and full live matrix pending |

All 67 KMG summon units carry hidden marker
`KMG.Summoning.Subtype.Extraplanar` (`1812739855844dc4adf3c32a70f13512`)
exactly once. This provides deterministic standalone subtype metadata without
requiring CotW's later-loaded feature during bootstrap. No optional-marker
mutation is needed; final-live Call of the Wild reconciliation passed twice.

## Tier III-IV natural and proxy reconstruction

The donor in every row supplies the selected view/rig only. KMG replaces class
levels, stats, size, speed, body weapons, facts, inventory, brain, XP, loot,
tags, and campaign behavior from the immutable profile. All rows share the
frozen quantity abilities generated for their higher-tier placements.

| Creature | Families/tier | KMG unit; visual donor | Delivered chassis/offense | Conservative deviation | Qualification |
|---|---|---|---|---|---|
| Boar | SM III; SNA III | `d98699ea265441a09c5b9b51769ef7ce`; `5f968d63d756f994ebff0d774e88e4ab` | Medium animal 2; 17/10/17/2/13/4; speed 40; NA +4; 1d8 gore; Ferocity, Toughness | None structurally identified | Structural/runtime/visual/persistence/profile PASS |
| Leopard | SM III; SNA III | `f36aad1d463444c69950f219457b894f`; `768275c9885dd954fb3c84ba69ac4281` | Medium animal 3; 16/19/15/2/13/6; speed 30; NA +1; bite 1d6, two claws plus native extra rake limbs; Pounce, Weapon Finesse; bite grab by limb identity on the shared summon grapple lifecycle, a foe of its size or smaller; rake gate: a charge or the foe held since the round began, dropped from other full attacks (Sprint 7; corrected 2026-09-25) | Native four-claw body is Owlcat's rake adaptation; a rake claw never grabs | Structural/runtime/visual/persistence/profile PASS; representative pounce combat PASS |
| Monitor Lizard | SM III; SNA III | `10b148d216364b509cd2c665a47b2950`; `4109b40f6bbb49640840644cc84ada67` | Medium animal 3; 17/15/17/2/12/6; speed 30; NA +3; bite 1d8; exact native Constitution-scaled poison; Great Fortitude; bite grab on the shared summon grapple lifecycle (Sprint 6) | Swim and grab omitted | Structural/runtime/visual/persistence/profile PASS; poison graph structurally qualified |
| Cheetah | SM III; SNA III | `6eb29e76792c41c5ab9e3812fe4b66e0`; Leopard view `768275c9885dd954fb3c84ba69ac4281` | Medium animal 3; 17/19/15/2/12/6; speed 50; NA +1; bite 1d6 with trip, two 1d3 claws; Weapon Finesse, Improved Initiative; procedural spotted coat at a 0.92 view scale; bounded once-per-summoning sprint (+30 ft, one round) (Sprint 8) | Once-per-hour tenfold sprint omitted; no bounded native cooldown contract proven | Structural/runtime/visual/persistence/profile PASS |
| Tiger (Phase 1 Sprint 8) | SNA IV | `KMG.Summoning.Unit.Tiger`; leopard rig `768275c9885dd954fb3c84ba69ac4281` at a 1.25 view scale | Large animal 6; 23/15/17/2/12/6; speed 40; NA +3; native 2d6 bite; four project 1d8 claws (two rake); Pounce; Improved Initiative, Skill Focus (Perception), Weapon Focus (claw); grab with the bite and both foreclaws by limb identity on the shared summon grapple lifecycle; rake gate (a charge, or the foe held since the round began; corrected 2026-09-25); procedural striped coat generated in the rig's texture space | Run and Skill Focus (Stealth) omitted; a stray rake roll is a silent automatic miss and the sequencing seam drops rake slots from other full attacks; no native tiger exists | Phase 1 state file records the guarded evidence |
| Crocodile | SM III; SNA III | `ed3fa562802b418ab062a7da622874da`; Monitor Lizard view `4109b40f6bbb49640840644cc84ada67` | Large animal 3; 19/12/17/1/12/2; speed 20; NA +4; bite 1d8 and secondary KMG 1d12 tail `d7ec01bae32a4d9086214f156ce52ecd` | Swim, grab, death roll, sprint, and hold breath omitted because no summon-safe target-state and movement contracts were proven | Structural/runtime/visual/persistence/profile PASS |
| Dire Bat | SM III; SNA III, 14 placements registered but hidden | `d867cb795b5640219a8362661f447697`; Giant Eagle flying rig `406c1e1af5400ac4881e330502ccbd9e` with instance-local original Bat mesh; sense feature `5dcc039bc9674208a51e4babcd8a30ee` | Large animal 4; 17/15/13/2/14/6; 40-foot airborne movement; NA +3; bite 1d8; Stealthy; native imprecise blindsense component at 40 feet on a Bat-only feature | 20-foot ground mode and Alertness omitted; native Blindsight feature rejected because it grants stronger precision and blindness immunity | Phase 2 Sprint 9 sense 2/2 and original Bat visual attachment 2/2 live PASS; Eagle/Roc donor isolation 4/4 and Pteranodon regression PASS on `20260927T0412311065438Z`; icon, publication, motion/impact and player path pending |
| Wolverine | SM III; SNA III | `f640d3e77d7d4a0d8de3351129cd7148`; Worg view `313a17cbd273d1f40bd1654ee2ae186e` | Medium animal 3; 15/15/15/2/12/10; speed 30; NA +2; two 1d6 claws and secondary 1d4 bite; Toughness | Burrow/climb and after-damage rage omitted because no save/load-safe summon-local rage state was proven | Structural/runtime/visual/persistence/profile PASS |
| Dire Boar | SM IV; SNA IV | `1710475cee544d9d858b05b24fb3ad4c`; `6ec9c63c41a1e754ea4dcd85557625b4` | Large animal 5; 23/10/17/2/13/8; speed 40; NA +6; 2d6 gore; Ferocity, Improved Initiative, Toughness | None structurally identified | Structural/runtime/visual/persistence/profile PASS |
| Dire Wolf | SM IV; SNA IV | `d2e7b46ea8994f7085063abac3775142`; `03dd28e92faf2e44eb9564a6ba01fdd0` | Large animal 5; 19/15/17/2/12/10; speed 50; NA +3; bite 1d8 with trip; Perception and bite focus | Run omitted because no exact final-live feature identity was proven | Structural/runtime/visual/persistence/profile PASS |
| Grizzly Bear | SM IV; SNA IV | `f7370368039e41ba8f88e8218cfb39d0`; `0b214d8e81a563549ba0be37cd1c16d0` | Large animal 5; 21/13/19/2/12/6; speed 40; NA +6; bite and two 1d6 claws; claw grab on the shared summon grapple lifecycle (Sprint 6) | Claw grab, Endurance, Run, and Survival focus omitted; generic grab carries unrelated constrict state | Structural/runtime/visual/persistence/profile PASS |
| Lion | SM IV; SNA IV | `14c7cbb0f32f4e30bbefaeaccba10269`; Leopard view `768275c9885dd954fb3c84ba69ac4281` | Large animal 5; 21/17/15/2/12/6; speed 40; NA +3; bite 1d8, two claws plus native extra rake limbs; Pounce; bite grab by limb identity on the shared summon grapple lifecycle; rake gate (a charge, or the foe held since the round began; corrected 2026-09-25); tawny visual tint on the leopard rig (Sprint 7) | Native four-claw body is Owlcat's rake adaptation; Run omitted | Structural/runtime/visual/persistence/profile PASS; representative pounce combat PASS |
| Pteranodon | SM IV; SNA IV | `c9a94142c9164ab793f7a06ae3fdcf56`; Roc-compatible Giant Eagle view `406c1e1af5400ac4881e330502ccbd9e` | Large animal 5; 16/19/15/2/15/12; 50-foot airborne movement; NA +2; bite 2d6; Dodge, Improved Initiative | Separate 10-foot ground mode omitted | Structural/runtime/visual/persistence/profile PASS; reach/scale/navigation bounded |
| Pony (Phase 1 Sprint 3) | SM I; SNA I | KMG unit per `blueprints/blueprints.json` (`KMG.Summoning.Unit.Pony`); native summoned pony `3f95557fc806db741b500a5735990841` | Medium animal 2; 13/13/14/2/11/4; speed 40; NA +0; two 1d3 hooves, both secondary (Docile: the game's own secondary flag, -5 to hit and half Strength to damage; corrected 2026-09-25) | Endurance and Run omitted | Phase 1 state file records the guarded evidence |
| Horse (Phase 1 Sprint 3) | SM II; SNA II | `KMG.Summoning.Unit.Horse`; native summoned horse `5bb9579fdb2b26b48bb10d61c81cfdfb` | Large animal 2; 16/14/17/2/13/7; speed 50; NA +0; two 1d4 hooves, both secondary (Docile; corrected 2026-09-25); reduced reach | Endurance and Run omitted | Phase 1 state file records the guarded evidence |
| Owlbear (Phase 1 Sprint 3) | SNA IV | `KMG.Summoning.Unit.Owlbear`; body donor `d6e0acbdbdb56114898922063ae2cba0` | Large magical beast 5; 19/12/18/2/12/10; speed 30; NA +5; bite 1d6 and two 1d6 claws; Improved Initiative, Great Fortitude, Skill Focus (Perception); reduced reach | Claw grab deferred to the Sprint 4 shared grapple lifecycle | Phase 1 state file records the guarded evidence |
| Cyclops (Phase 1 Sprint 3) | SNA V | `KMG.Summoning.Unit.Cyclops`; body donor `124f1c45ef24d654e9cd420fe84f7f36` | Large humanoid 10; 21/8/15/10/13/8; speed 30; NA +7 and +4 hide armor as an armor-descriptor fact (AC 19); native greataxe (Large 3d6); Ferocity, Power Attack, Cleave; Flash of Insight bounded to one use per summoning, armed until the next attack roll: that attack's own d20 chosen as a natural 20, ordinary confirmation (corrected 2026-09-25) | Heavy crossbow, Alertness, Great Cleave, Improved Bull Rush omitted; the tabletop choice of any die roll is narrowed to the next attack | Phase 1 state file records the guarded evidence |
| Frost Giant (Phase 1 Sprint 3) | SNA VII, VIII (1d3), IX (1d4+1) | retained native unit `590cd3d5e76fdc649a5f97bc984cd3c4` through creature-named wrappers; no KMG unit | native Frost Giant unchanged | none; the Nature's Ally wrappers spawn the same unit the SM VIII wrapper does | Phase 1 state file records the guarded evidence |
| Shambling Mound (Phase 1 Sprint 4) | SNA VI | `KMG.Summoning.Unit.ShamblingMound`; body donor `b98ae409beb5e8543a75b82ecda082a7` | Large plant 9; 21/10/17/7/10/9; speed 20; NA +10; two 2d6 slams; fire resistance 10, electricity immunity; Power Attack, Iron Will, Lightning Reflexes, Cleave, Weapon Focus (slam); slam grab and constrict 2d6+7 on the shared summon grapple lifecycle | Electric Fortitude Constitution gain, swim and the native poison aura omitted; the holder cannot act while holding (native lockdown) | Phase 1 state file records the guarded evidence |
| Giant Flytrap (Phase 1 Sprint 4) | SNA VII | `KMG.Summoning.Unit.GiantFlytrap`; body donor `fb824352b7968fb4d8103ac439644633` | Huge plant 13; 25/18/25/1/12/6; speed 10; NA +10; four 1d8 bites; acid resistance 20; native 60-ft blindsight for tremorsense; trip immunity; Cleave, Great Fortitude, Improved Initiative, Power Attack, Skill Focus (Stealth), Weapon Focus (bite); one grab link per bite (four at most) on the shared lifecycle, and a mouth that holds or has engulfed a foe attacks no one else; an active hold is session-scoped and releases cleanly on a reload (owner-accepted engine limitation, 2026-09-26); engulf of a Medium or smaller foe held since the round began, 1d8+7 bludgeoning plus 2d6 acid each round (corrected 2026-09-25) | Vital Strike omitted; Intelligence 1 for the absent score | Phase 1 state file records the guarded evidence |
| Purple Worm (Phase 1 Sprint 4) | SNA VIII | `KMG.Summoning.Unit.PurpleWorm`; native summoned worm `bf2216f48b3f4d24c9c502007649340d` | Gargantuan magical beast 16; 35/6/25/1/8/8; speed 20; NA +22; native bite and sting; exact native Constitution-scaled sting poison; trip immunity; Critical Focus, Improved Critical (bite), Power Attack, Weapon Focus (bite); bite grab holds, and a later turn's successful maintain check swallows a foe up to one size smaller whole through the native swallow-whole part (corrected 2026-09-25) | burrow, swim and the native brain omitted; Awesome Blow, Improved Bull Rush, Staggering Critical, Weapon Focus (sting) omitted; OwnerAcceptedEngineLimitation: SWALLOW_WHOLE_INTERIOR_AC_HP_UNMODELED (October 5 live census; no interior target/AC/HP/cut-free path) | Phase 1 guarded mechanics remain qualified; interior omission acknowledged without reopening them |
| Dust Mephit (Phase 1 Sprint 5) | SM IV / SNA IV | `KMG.Summoning.Unit.DustMephit`; native summoned air mephit `50782bc4eb36aac4287023e20ee00808` | Small outsider 3; 13/15/12/6/11/14; speed 40; NA +3; two 1d4 claws; DR 5/magic; fast healing 2; air subtype; 15-ft enemies-only 1d4 slashing breath with a sickening rider; blur once per summoning; project Wind Wall once per summoning (15-ft shelter for 6 rounds for every creature inside that is not the mephit's enemy: arrows and bolts miss, other ranged weapons 30% miss chance; corrected 2026-09-25); warm sand rim glow (the mephit body's visible colour) | spell-like abilities once per summoning rather than per hour or per day; the wall is a cylinder around the mephit rather than a placed wall; allies in the cone take nothing | Phase 1 state file records the guarded evidence |
| Ice Mephit (Phase 1 Sprint 5) | SM IV / SNA IV | `KMG.Summoning.Unit.IceMephit`; native summoned water mephit `4615328295cd7e84bb2ef09d3dba8403` | as the dust mephit; air subtype, cold immunity, fire vulnerability; 15-ft enemies-only 1d4 cold breath with a sickening rider; magic missile once per summoning; project Chill Metal once per summoning (a metal-bearing foe within close range, Will negates, seven-round cold table in full for metal armor and minimal for a metal weapon; corrected 2026-09-25); icy cyan-white rim glow | metal is judged by armor type and weapon category (padded, leather and hide are not metal; wooden weapons are not) | Phase 1 state file records the guarded evidence |
| Magma Mephit (Phase 1 Sprint 5) | SM IV / SNA IV | `KMG.Summoning.Unit.MagmaMephit`; native summoned fire mephit `10a820de0a417f345866f794324205ad` | as the dust mephit; earth and fire subtypes (fire immunity, cold vulnerability); 15-ft enemies-only 1d8 fire breath; project Pyrotechnics once per summoning (enemies within 20 ft blinded 1d4+1 rounds, Will negates) and project Magma Form once per summoning (5 rounds: DR 20/magic, speed 10, no attacks; corrected 2026-09-25); ember-red rim glow (no emission slot on the shader) | pyrotechnics is a 20-ft burst from the mephit's own fire rather than a 120-ft fireworks radius; magma form lasts five rounds | Phase 1 state file records the guarded evidence |
| Ooze Mephit (Phase 1 Sprint 5) | SM IV / SNA IV | `KMG.Summoning.Unit.OozeMephit`; native summoned water mephit `4615328295cd7e84bb2ef09d3dba8403` | as the dust mephit; water subtype; 15-ft enemies-only 1d4 acid breath, Reflex negates damage and sickening together; acid arrow and stinking cloud once per summoning each; slime-green rim glow | none beyond the once-per-summoning uses | Phase 1 state file records the guarded evidence |
| Salt Mephit (Phase 1 Sprint 5) | SM IV / SNA IV | `KMG.Summoning.Unit.SaltMephit`; native summoned air mephit `50782bc4eb36aac4287023e20ee00808` | as the dust mephit; earth subtype; 15-ft enemies-only 1d4 slashing breath with a sickening rider; glitterdust once per summoning; dehydrate as a 20-ft enemies-only burst (2d8, Fortitude half) once per summoning; crystalline white rim glow | dehydrate is a project burst rather than a single-target spell | Phase 1 state file records the guarded evidence |
| Steam Mephit (Phase 1 Sprint 5) | SM IV / SNA IV | `KMG.Summoning.Unit.SteamMephit`; native summoned water mephit `4615328295cd7e84bb2ef09d3dba8403` | as the dust mephit; fire and water subtypes (fire immunity, cold vulnerability); 15-ft enemies-only 1d4 fire breath with a sickening rider; blur once per summoning; boiling rain as a 20-ft enemies-only burst (2d6 fire, Fortitude half) once per summoning; grey-white vapour rim glow (no emission slot on the shader) | boiling rain is a project burst rather than a cloud | Phase 1 state file records the guarded evidence |

The fresh-process assertion compares every row's HD/class, size, six ability
scores, speed, primary/additional/secondary weapon references, natural armor,
feat/special-fact GUIDs, extraplanar marker, and empty inventory against the
checked-in catalog. It also verifies the KMG crocodile tail is exactly 1d12.

## Tier V-VII natural and proxy reconstruction

The final natural group uses the same immutable reconstruction contract: the
donor supplies only the view/rig, while KMG owns all HD, stats, body weapons,
facts, brain, inventory, and alignment state. The three KMG weapon identities
below preserve proven native animation categories while freezing tabletop dice.

| Creature | Families/tier | KMG unit; visual donor | Delivered chassis/offense | Conservative deviation | Qualification |
|---|---|---|---|---|---|
| Dire Lion | SM V; SNA V | `56c64aa6765a4c37a1b30c0c5b31427b`; Smilodon `beae4985629a6f64eb98081e3171e4c1` | Large animal 8; 25/15/17/2/12/10; speed 40; NA +4; bite 1d8, two 1d6 claws plus two secondary rake claws; Pounce, Improved Initiative, Perception/claw focus; bite grab by limb identity on the shared summon grapple lifecycle; rake gate (a charge, or the foe held since the round began; corrected 2026-09-25) | Run omitted; secondary rake follows Owlcat's native pounce adaptation | Structural/runtime/visual/persistence/profile PASS; representative pounce combat PASS |
| Ankylosaurus | SM V; SNA V | `10c80d5cfa594332bd3e5127799e426f`; Hodag `c3524f96954a1d94f8525b86e7626633` | Huge animal 10; 27/10/17/2/13/8; speed 30; NA +14; KMG 3d6 tail `15394605e1664a51bce4b50f38a7603a`; Great Fortitude, Power Attack | Strength-based daze/stun rider omitted because no bounded native Dazed contract was proven; bull-rush/overrun/tail-focus identities unproven | Structural/runtime/visual/persistence/profile PASS; tail animation and bounded scale PASS |
| Dire Bear | SM VI; SNA VI | `11b15a81cea1498babfdb57af1b53c41`; `260da5b557e3fb04bb4960a36a5d1dc4` | Large animal 10; 25/13/21/2/12/10; speed 40; NA +8; bite 1d8, two claws 1d6; Improved Initiative, Iron Will, Perception focus; claw grab on the shared summon grapple lifecycle (Sprint 6) | Grab, Endurance, and Run omitted | Structural/runtime/visual/persistence/profile PASS |
| Smilodon | SM VI; SNA VI | `d15ee151c2274f9f86ab523f111bc3af`; `beae4985629a6f64eb98081e3171e4c1` | Large animal 14; 27/15/17/2/12/10; speed 40; NA +6; 2d6/19-20 bite, two 2d4 claws plus two secondary rake claws; Pounce and exact critical/focus feats; grab with the bite and both foreclaws by limb identity on the shared summon grapple lifecycle; rake gate (a charge, or the foe held since the round began; corrected 2026-09-25) | Run omitted; secondary rake follows Owlcat's native pounce adaptation | Structural/runtime/visual/persistence/profile PASS; representative pounce combat PASS |
| Elephant | SM VI; SNA VI | `9dd8544097234f05bcd35d400d91b510`; Mastodon `028cc6f46e7998f46855a33ffde89567` | Huge animal 11; 30/10/19/2/13/7; speed 40; NA +9; native 2d8 gore and secondary 2d6 slam; Great Fortitude, Iron Will, Power Attack, Perception focus | Trample omitted because no commandable path-safe movement contract was proven; Endurance and Improved Bull Rush identities unproven | Structural/runtime/visual/persistence/profile PASS; bounded scale/navigation PASS |
| Mastodon | SM VII; SNA VII | `e129ffc2768d47c5bee61bb99b0c8703`; dedicated summon `028cc6f46e7998f46855a33ffde89567` | Huge animal 14; 34/12/21/2/13/7; speed 40; NA +12; native 2d8 gore and secondary 2d6 slam; Iron Will, Power Attack, Perception focus | Trample omitted; Endurance, Improved Bull Rush/Will, and gore-focus concrete identities unproven | Structural/runtime/visual/persistence/profile PASS; bounded scale/navigation PASS |
| Roc | SM VII; SNA VII | `439e955cb0fd41daafd0478d3641615a`; Giant Eagle/Roc rig `406c1e1af5400ac4881e330502ccbd9e` | Gargantuan animal 16; 28/15/17/2/12/11; 80-foot airborne movement; NA +14; KMG 2d8 bite `c19d1025fe2b47769c93a3b76d0c052c` and two 2d6 talons `8a3741a7598147baa08de552565635ad`; exact critical/initiative/save/focus feats | Separate ground speed, talon grab, and Flyby Attack omitted | Structural/runtime/visual/persistence/profile PASS; footprint/reach/navigation/camera bounds PASS |

Fresh Steam-backed run `20260812T0045336396930Z` passed all 29 assertions on
exact source `3c2c5fef82a7d9b032f7da906385013a5699cc8c`. It checked every
profile field and attack reference, exact 3d6/2d8/2d6 custom weapon dice, all
681 placements, the constant 1,403 registry, and zero donor component aliases,
forbidden references, inherited spells, inventory, or native-action
contamination. No save was accessed.

## Lantern Archon

| Field | Delivered behavior |
|---|---|
| Families/tiers | Summon Monster III; higher-tier same-kind quantity placements generated by the frozen matrix |
| Unit identity | `02f8e9c6c91549deaded9ef667399449` (`KMG.Summoning.Unit.LanternArchon`) |
| Visual donor | `24719a49b84c5cd43b894268d22d9c89` (`CR6_WillOWispStandart`), prefab `8a8d7c448ff2c8749adc08eeb223333b`; visual only |
| Chassis | 2 outsider HD; Small; lawful good; Str 1, Dex 11, Con 12, Int 6, Wis 11, Cha 10; 60-foot movement using airborne native navigation |
| Offense | KMG ray `d4c2ce6c90094fdfb0fd908312372d72`; two native projectile/ranged-touch attack rolls, each 1d6 direct damage, custom 30-foot range |
| AI | KMG action `3579bfa7c4b040c4812286f4ade47146` and single-action brain `427b496a05db48aa94997415f1a74c39`; no Ghaele spell AI |
| Defenses | Natural armor +4; electricity immunity; DR 10/evil; +4 racial vs poison; +2 resistance saves and +2 deflection AC vs evil through KMG buff `4c55af41c90443c18267a806c740ce16` |
| Traits/aura | Good, lawful, extraplanar, airborne facts; structurally reuses Aura of Menace carrier `1ce4878b5e714f659d0854a12f4b3cf2` when supplied by a compatible optional mod; standalone conservatively omits the aura because Kingmaker 2.1.7b has no native carrier |
| Removed donor mechanics | Wisp HD/type mechanics, touch weapons, invisibility, spell immunity, ambush/tags/brain; all Ghaele weapons, spells, gaze, inventory, and azata facts |
| Conservative deviations | Greater teleport and gestalt omitted by summon safety contract. No distinct native Archon subtype fact was found; outsider HD plus explicit lawful/good/extraplanar traits implement the mechanical type surface. Low-light/darkvision and truespeech are not separately added because no safe bounded unit fact has yet been proven. |
| Qualification | `1009/1009` domain PASS; final structure, production cast, dual-ray combat, view/projectile, cleanup, persistence, and required profiles PASS. Aura is conditionally present only where the compatible optional carrier exists. |

## Invisible Stalker

| Field | Delivered behavior |
|---|---|
| Families/tiers | Summon Monster VI; higher-tier same-kind quantity placements generated by the frozen matrix |
| Unit identity | `3cd7b4f2c65b4d35929dd4d969dfaa41` (`KMG.Summoning.Unit.InvisibleStalker`) |
| Visual donor | `2e24256e459468743b91fbb9aa85e1ab` (`SummonedAirElementalHuge`); prefab only, with the KMG chassis reset to Medium |
| Chassis | 7 outsider HD; Medium neutral; Str 18, Dex 19, Con 22, Int 14, Wis 15, Cha 11; 30-foot airborne movement |
| Offense | Two native `AirElementalSlam_Large` attacks, matching the intended 2d6 slam dice; Combat Reflexes and Weapon Focus (slam) |
| Defenses/traits | Natural armor +6; air, elemental, and extraplanar traits; Improved Initiative and Lightning Reflexes; native `NaturalInvisibilityBuff` whose exact graph preserves invisibility after offensive actions |
| Removed donor mechanics | Huge size/stats/HD, whirlwind, air mastery, elemental brain, enemy scaling, XP/loot, and donor class progression |
| Conservative deviations | Dedicated tracking behavior and scent are omitted because no bounded native tracking fact was proven. |
| Qualification | `1009/1009` domain PASS; final structure, production cast, attack-safe permanent invisibility combat, bounded view/navigation, cleanup, persistence, and required profiles PASS. |

## Bebelith

| Field | Delivered behavior |
|---|---|
| Families/tiers | Summon Monster VII; higher-tier same-kind quantity placements generated by the frozen matrix |
| Unit identity | `9d7d36e71ea141258509b8a32557577e` (`KMG.Summoning.Unit.Bebelith`) |
| Visual donor | `51c66b0783a748c4b9538f0f0678c4d7` (Doomspider); view/rig only, enlarged through the KMG Huge chassis |
| Chassis | 12 outsider HD; Huge chaotic evil; Str 28, Dex 12, Con 24, Int 11, Wis 13, Cha 13; 40-foot ground movement; natural armor +13; DR 10/good |
| Offense | Two KMG 2d4 claws (`85971d6300dd41a0a62b0dd92a570045`) and a native-animation 2d6 Huge bite; +2 attack and damage against exact chaotic-evil outsiders |
| Dismantle | The second same-target claw hit in one round triggers Reflex DC 25; failure applies KMG state `736a349933f24cc2b11bc284b6e559cb` for one round and reduces AC by 2 without changing the equipped item |
| Removed donor mechanics | Doomspider poison, web and web immunity, donor HD/stats/type/brain, enemy scaling, XP/loot/inventory, and campaign behavior |
| Conservative deviations | Permanent armor destruction is replaced with a bounded one-round AC penalty to preserve inventory/save safety. Demon hunting keys from exact chaotic-evil outsider facts and grants +2 attack/damage. Rot and climb are omitted because no safe bounded native implementation has been proven. |
| Qualification | `1009/1009` domain PASS; final structure, production cast, two-claw dismantle/save/effect, demon-hunting combat, bounded scale/navigation/view, cleanup, persistence, and required profiles PASS. |

## Pixie

| Field | Delivered behavior |
|---|---|
| Families/tiers | Summon Nature's Ally IX |
| Unit identity | `396881ade24e4ddba188dc2e7ff481f9` (`KMG.Summoning.Unit.Pixie`) |
| Visual donor | `394610e32cfbc4f43a0efaab16faae49` (`CR1_Nixie`), prefab `6d6f3b81a5b50534399bb5cb778cd4e0`; view/rig only |
| Chassis | 4 fey HD; Small neutral good before SNA spawn-local caster alignment; Str 7, Dex 21, Con 12, Int 16, Wis 15, Cha 16; 60-foot airborne movement; natural armor +1; DR 10/cold iron; SR 15; attack-safe native natural invisibility |
| Sleep arrows | Body-mounted KMG longbow `0a1d8ac4be724595aa952471a9491975` uses the native arrow rig and deals zero weapon dice; 16 save-backed resource uses, Will DC 15, native Sleeping state for 50 rounds on failure; no ammunition item or inventory transfer |
| Irresistible dance | KMG spell-like ability `5cacf6a72c724b7fad8d7605cba1e790`, one resource-backed use, CL 8/spell level 6, direct touch-range delivery, and bounded KMG dance state `aa8b4284e12e49f0b37f327f665638d1`; target can do nothing but dance and takes -4 AC/-10 Reflex for 1d4+1 rounds on a failed Will save or one round on success. The spell-like ability does not clone Call of the Wild-only delivery/state blueprints. |
| Removed donor mechanics | Donor HD/stats/class progression/spells/brain/inventory/campaign behavior; no teleportation, summon/conjuration, permanent ammunition, transferable loot, or persistent external effect |
| Conservative deviations | The no-damage sleep bow preserves projectile/animation behavior while keeping the special effect bounded. |
| Qualification | `1009/1009` domain PASS; final structure, production cast, bow/dance use, resources, animation/projectile, combat, bounded fey view/navigation, cleanup, persistence, and required profiles PASS. |

## Salamander

| Field | Delivered behavior |
|---|---|
| Families/tiers | Summon Monster V; higher-tier same-kind quantity placements generated by the frozen matrix |
| Unit identity | `f8fb103168d74b4c93182437e5d2b4e4` (`KMG.Summoning.Unit.Salamander`) |
| Visual donor | Frozen Lizardfolk donor; view/rig only, with all donor equipment, progression, inventory, and campaign behavior removed |
| Chassis | 8 outsider HD; Medium chaotic evil; Str 16, Dex 13, Con 18, Int 14, Wis 15, Cha 13; 20-foot movement; natural armor +7; fire and extraplanar traits; DR 10/magic |
| Offense | Native standard spear in the primary hand; KMG tail weapon `93e097b8d3db42d3a37656502899e1a9` dealing 2d6; bounded combat fact `a47bc65d6b6b42b6a19610e22b13f171` supplies one hit-confirmed 1d6 fire rider and a cloned grab/constrict graph dealing 2d6+4 |
| Removed donor mechanics | Lizardfolk stats, HD, inventory, drops, class progression, donor brain, and campaign surfaces; no planar travel, summoning, or unrelated poison/web/spell behavior |
| Conservative deviations | The engine graph models spear and tail through native weapon slots and bounded hit triggers. Cold vulnerability is omitted because no exact safe bounded fact was proven. |
| Qualification | `1009/1009` domain PASS; final structure, production cast, spear/tail/heat/grab-constrict combat, bounded view, cleanup, persistence, and required profiles PASS. |

## Succubus

| Field | Delivered behavior |
|---|---|
| Families/tiers | Summon Monster VI; higher-tier same-kind quantity placements generated by the frozen matrix |
| Unit identity | `0c908145873f4b67a188397ca5f46da1` (`KMG.Summoning.Unit.Succubus`) |
| Visual donor | Frozen Nymph-preferred donor; view/rig only, with all donor spells, gaze, class progression, inventory, and campaign behavior removed |
| Chassis | 8 outsider HD; Medium chaotic evil; Str 13, Dex 17, Con 14, Int 18, Wis 13, Cha 27; 30-foot movement; natural armor +7; chaotic, evil, extraplanar traits |
| Offense | Two native 1d6 claws; bounded Dominate Person spell-like ability `1662d63944d94cdeaa62562dc9ac9349` with Charisma parameters, humanoid-only target contract, Will save, and three-round duration; single-action AI/brain `8109da6090a64cbcb02326fb08e8ce1f` / `38e57062576e4c9e97c2982972c81328` |
| Defenses/drain | DR 10/cold iron or good; acid/cold resistance 10; fire/electricity/poison immunity; SR 18. Combat fact `cd51ad31f8764d2797b59eb43da7a9f8` applies one temporary negative level for one round on the first qualifying hit only. Domination buff `6e1f6eb3e773451dbda9e0ecd07486d9` removes itself if the summoned caster disappears. |
| Removed donor mechanics | Nymph spells/gaze/class facts and campaign surfaces; native one-day/permanent-capable energy drain was rejected; teleportation, summoning, permanent profane gift, and external persistent target state are absent |
| Conservative deviations | Charm and energy-drain identity are represented by bounded domination and a one-round temporary first-hit drain. Profane gift is omitted because it can outlive the summon. |
| Qualification | `1009/1009` domain PASS; final structure, production cast, domination/drain combat and bounded cleanup, view, persistence, and required profiles PASS. |

## Shadow Demon

| Field | Delivered behavior |
|---|---|
| Families/tiers | Summon Monster VI; higher-tier same-kind quantity placements generated by the frozen matrix |
| Unit identity | `627c1841a5eb4e32b1a94c0f43ec8a60` (`KMG.Summoning.Unit.ShadowDemon`) |
| Visual donor | `1832be68f9814254dbbdab6df7fd5d0b` (`SoulEaterSummoned`); view only |
| Chassis | 7 outsider HD; Medium chaotic evil; Str 17, Dex 20, Con 14, Int 14, Wis 13, Cha 17; 40-foot airborne movement |
| Offense | Claw/claw/bite using native 1d6 claw and 1d8 bite weapons; KMG combat-traits fact `f81993d391054678a138227b91141eae` adds 1d6 cold to hit-confirmed natural attacks |
| Defenses/traits | Native incorporeal damage handling and critical/precision immunity; DR 10/cold iron or good; acid/fire resistance 10; cold/electricity/poison immunity; SR 17; chaotic, evil, extraplanar facts |
| Removed donor mechanics | Soul Eater HD/stats, Wisdom-damage feature, all-around vision, DR/magic, campaign facts, and donor brain; teleportation and summon/conjuration are absent |
| Conservative deviations | Possession is omitted because no duration-bound, save/load-safe control transfer was proven. Shadow blend and sprint are omitted because no safe light-state and cooldown primitives were proven. Demon subtype is represented by outsider plus chaotic/evil/extraplanar facts because no exact standalone native Demon subtype fact was found. |
| Qualification | `1009/1009` domain PASS; final structure, production cast, incorporeal/cold combat, bounded view, cleanup, persistence, and required profiles PASS. |

## Polish visual and publication deviations

| Surface | Final disposition | Evidence |
|---|---|---|
| Dire Bat | Phase 2 Sprint 9 publishes 14 preserved SM/SNA identities with an original skinned Bat mesh, albedo and distinct creature icon; dedicated native imprecise blindsense at 40 feet, no precise blindsight or immunity. Open-floor travel, Quickened own-tier RTWP/turn-based combat, disabled publication and save-backed module-off cleanup qualified; doorway traversal and bite contact qualified on the final guarded fixture. | Guarded cast Bat sense/visual 2/2; player path 813/813 roots; live inventory 48/48 (`20260927T0623527812622Z`); prepare/reload/expiry trio 14/14 each with original Bat renderer reattached and then absent (`20260927T0655003623332Z`, `20260927T0659022955480Z`, `20260927T0703025930540Z`). Native no-forced-path travel 6.195 m to within 1.26 m of destination (`20260927T0911579435138Z`). Exact-hostile weapon rules: turn-based 2 (`20260927T1007223013777Z`), RTWP 1 (`20260927T1010235825673Z`). Module-off eighteen-parent census PASS with zero added options and 46 native variants (`20260927T1046174341970Z`). Saved Bat reloaded on a visible native donor under the disabled module, then cleaned with the other 15 fixture summons and one authorized write (`20260927T1151452804617Z`); final load found zero KMG summons and wrote nothing (`20260927T1159482067877Z`). Exact live-tree restoration. Owner visual approval pending. |
| Eagle / Poisonous Frog | Eagle has an original feathered skinned mesh/albedo at 0.30 view scale; Poisonous Frog retains 0.48. Mechanical size/reach unchanged. | Guarded Eagle visual 2/2; live height 1.360 < Medium humanoid 1.926; selection/navigation, locomotion, attack and hit/death contracts 81/81 (`20260927T0446552603378Z`); Eagle renderer reattached after working-save reload and absent after cleanup (`20260927T0659022955480Z`, `20260927T0703025930540Z`). Native no-forced-path travel 7.943 m to within 0.18 m of destination (`20260927T0911579435138Z`). Exact-hostile Quickened weapon rules: turn-based 6 (`20260927T1000134559074Z`), RTWP 1 (`20260927T1017190134756Z`); live body-contact and native doorway reviews passed. |
| Dire Boar / Dire Bear | View-only scale 1.15; each reads larger than its non-dire analogue. | Live bounds 2.845 > 2.474 and 3.768 > 3.277. |
| Pteranodon | View-only scale 0.82 to keep the Roc rig bounded. | Live bound 8.368 < Roc 11.226. |
| Elephant / Mastodon | View-only 0.90 / 1.15; Elephant remains on shared Mastodon material rather than mutate a native asset. | Live bounds 9.568 < 12.226. Gray recolor intentionally deferred. |
| Roc | View-only scale 1.10; bounded camera/selection/navigation checks pass. | 67-view visual scenario PASS. |
| Icons | Exact donor/item/ability sprite, then immutable base-game category fallback. | Called-out canine, feline, reptile, flying, celestial and fiend group distinctions PASS live inventory. |

### Sprint 9 attack-anchor qualification update, 2026-09-27

Guarded own-tier Eagle combat `20260927T1219166168032Z` passed six
exact-hostile weapon rules. Dire Bat combat `20260927T1226258599811Z`
passed two exact-hostile bites. The first geometry probe reported roughly
2 m for Eagle rig anchors and 0.436-0.454 m for Bat jaw anchors, but the
baked-surface repeat `20260927T1240150768739Z` exposed that these distances
were measured to the hostile's `L_WeaponMarker`, not its body. The geometry
values are excluded from visual contact qualification; native attack evidence
remains valid. Both runs restored the original live mod tree exactly.

Corrected Eagle and Bat runs `20260927T1252224741767Z` and
`20260927T1259362389907Z` passed native combat and measured the same
substantial `Character` body renderer. Eagle's beak/talon weighted vertices
were 1.27-1.39 m from its bounds at six weapon events, a visual impact defect.
Bat's beak weighted vertices intersected the bounds at two bites (0 m).
Both runs restored the original live mod tree. Eagle impact and doorway
behavior remain open; Bat bite contact passed this bounded fixture.

The first baked Eagle values above applied its 0.30 view scale twice and are
excluded. Calibration `20260927T1314477538739Z` exposed this. Corrected
guarded Eagle combat `20260927T1326061196128Z` passed six exact-hostile
weapon events with facing dot 1; head/beak surface was 0.739-0.798 m from
the body on bites and nearest talon surface 0.864-0.906 m away on claws.
This is the valid Eagle attack-contact gap to resolve. Exact original-tree
restoration and 1,927 domain, clean Release, strict package PASS.

An Eagle-only, instance-local skeleton lunge now closes the measured contact
gap without altering native attack reach. Guarded six-attack turn-based result
`20260927T1345424022344Z` measured 0-0.144 m beak/head and 0-0.226 m talon
weighted-surface gap to the hostile body. Guarded Eagle/Bat creature review
`20260927T1353291620814Z` passed both native-travel and cleanup gates;
the Bat remains unchanged. Both wrappers restored the original live tree.
Repository validation, 1,928 domain tests, clean Release and strict package
passed. Guarded RTWP result `20260927T1404176347439Z` passed an exact
native bite with the Eagle head-weighted surface 0.106 m from the hostile
body, followed by verified live-tree restoration. A doorway route remains
open for Sprint 9.

Sprint 9 final native doorway run `20260927T1536450257132Z` passed Eagle and
Dire Bat together (12/12 assertions). Both crossed a blocked direct line by
native `UnitMoveTo`, travelled 12.357/12.360 m, finished 0.021/0.024 m
from the adjacent-room node and left zero reviewed summons after cleanup.
The earlier overlong destination failed and is excluded. Combined runtime
restored the exact original live tree; repository validation, 1,928 domain
tests, clean Release and strict package passed. Eagle's body-contact and
Bat's bite-contact gates had already passed. Sprint 9 internal technical
status: PASS; HumanReview: NOT_PERFORMED_NONBLOCKING. Sprint 10 remains next.

## Phase 2 Sprint 10 provisional Wasp row

Giant Wasp: SM IV and SNA IV; six legal quantity placements in each family;
Celestial/Fiendish Monster execution policy, caster-alignment Nature's Ally
policy. Dedicated Large 4-HD Vermin unit, Str 18/Dex 12/Con 18/Int 1
(engine substitute for no Intelligence score)/Wis 13/Cha 11, natural armor
+4, 60-foot airborne travel, and dedicated 1d8 sting. The 20-foot ground
speed is unavailable in the single-speed engine profile. Wasp has an
original project-owned six-leg/four-wing visual asset. A cloned native
poison lifecycle now applies an injury Fortitude DC 18 effect on a sting hit:
1d2 Dexterity damage, six total exposures, one successful save to cure.
The +2 racial DC is set on the on-hit poison context, which the native buff
retains for round saves. Guarded mechanical run
`20260927T1912579609072Z` passed the Wasp sting, DC, initial damage,
failed-save continuation, successful-save cure, and private 16-bone visual
attachment (2/2); the working save's enemy-damage scale of 0.8 truncated
one rolled 1 to zero on the next round, so that single tick is not evidence
of positive subsequent damage. Guarded working-save review
`20260927T1955333075608Z` passed native 12.345 m planar travel, connected
doorway crossing and complete cleanup. The four camera frames show the
original striped body and moving wings but are crowded by walls and shelves;
a fifth frame with its auxiliary renderer hidden retained cyan silhouettes,
consistent with native occlusion display. Clean attack-to-target contact,
combat cadence, immunities and lifecycle review remain open. The guarded
`20260927T2015561300256Z` fixture passed all four private Wasp quantity
commands (1d3 and 1d4+1 in both families), exact-kind counts, 14/14 visual
attachments and per-cast cleanup. All
twelve placements stay suppressed; no
published player choice or full Wasp combat qualification is claimed.
The guarded `20260927T2115391949167Z` probe passed native RuleApplyBuff
mind-affecting immunity on the Wasp against an eligible human control;
neither control buff was installed, so only native rule eligibility is
claimed. The native VerminType fact is present, but the cloned donor's
`EagleGiant` species marker requires correction before publication.
The subsequent owned unit type `682c4c25e772495e882fc2cacddc0c38`
replaces that marker on the Wasp only. Guarded
`20260927T2152154541742Z` passed the live type and native
`VerminImmunities` grant with paired Wasp-immune/human-eligible
RuleApplyBuff results. The native component mask did not directly match
the `MindAffecting` bit; the claim rests on the actual rule result and
granted fact. The type image is unassigned while the creature is hidden.
Guarded own-tier Summon Monster IV combat runs
`20260927T2217134616132Z` (turn-based) and
`20260927T2220168994981Z` (RTWP) each passed two exact-target native
sting rules. Baked stinger surface gaps were 0.643–0.654 m and
0.743/2.04 m respectively. Cadence is qualified in those fixtures, but
visual sting contact remains a defect. Wasp stays unpublished.
At native impact, guarded overhead captures from
`20260927T2251029714353Z` and `20260927T2254096344270Z` show the
abdomen/stinger pointing away from the hostile. The party-camera frames
are wall-occluded; the first RTWP overhead frame is partly dissolved.
The intact second RTWP frame confirms the pose defect. A forward body
lunge alone is insufficient; target-directed tail motion remains to be
proved. Mechanical two-mode cadence continues to pass.
Guarded request-local Tail-bone probes `20260927T2310527552766Z`
(turn-based) and `20260927T2314172123373Z` (RTWP) passed their native
two-sting scenarios. The probe restored the bone after each capture. Its
baked tip moved from 2.04/2.049 m to inside the target bounds in turn-based;
RTWP moved from 2.137 to 0.012 m and 3.51 to 1.198 m. Same-frame overhead
renders did not establish a visibly improved sting pose, and the second
RTWP geometry still missed. This is diagnostic only, not a contact pass or
a production animation. All twelve Wasp choices remain suppressed.
The follow-up two-frame request-local pose runs `20260927T2331283868029Z`
and `20260927T2334367843649Z` again passed two native stings in each mode.
The baked tip entered target bounds in both turn-based samples and the first
RTWP sample, but the second RTWP gap stayed 1.193 m. Delayed renders show
a changed pose, yet the doorway clips the Wasp body/wings and the target
contact is obscured by impact effects. This pose is not a credible, clear
production sting. Wasp remains hidden pending a visual-rig/contact repair.
Source commit `9f011014` and guarded inventory
`20260927T1804369885102Z` passed 48/48 structural/menu assertions with
exact live-tree restoration. Source rules:
[Paizo Giant Wasp](https://legacy.aonprd.com/bestiary/wasp.html).

The original Wasp now uses a sword-length Tail-weighted stinger and a
view-local, native-weapon-event pose. The guarded open-floor turn-based run
`20260928T0751193161358Z` placed its baked stinger at the hostile on both
native hits (0/0 m); RTWP `20260928T0754382265441Z` recorded 0/0.191 m.
The 0.25 m contact gate passed in both modes. The earlier shorter-stinger
diagnostics `20260928T0738095891433Z` and `20260928T0741288985771Z`
failed that gate at 0.418 and 0.775 m respectively and are excluded from
qualification. The guarded creature review `20260928T0817537337846Z`
passed 12.334 m native travel, connected doorway crossing, retained painted
renderer, full unit cleanup and zero private Wasp meshes/materials after
view destruction. Normal party-camera renders read as a striped flying
wasp; overhead impact frames still carry native effects that obscure the
needle. The Wasp remained hidden at this visual checkpoint. HumanReview:
NOT_PERFORMED_NONBLOCKING.

The later original painting has a manifest-backed 128 px export with 26
exact consumers. All twelve SM/SNA Wasp placements are now published.
Guarded `20260928T0955440745921Z` passed all 825 published generated
spellbook roots, 29 native wrappers, one-slot/quantity contracts, and
request-local cleanup. Guarded `20260928T1044279887208Z` passed 50/50
inventory assertions: all 18 menu equations, zero missing icons, exact
inspectable unit-type sprite and zero prohibited references. The original
136-file live installation was restored after both runs. This closes the
Wasp technical publication gate; owner visual approval remains pending.

## Phase 2 Sprint 10 Stirge row

Final disposition: the corrected session-attachment implementation is
published and internally technically qualified. The dated paragraphs below
retain the incremental evidence trail; references there to a hidden choice,
reciprocal native attachment, open art, or pending requalification describe
superseded checkpoints rather than current behavior.

Historical progression: Stirge is SNA I with nine legal quantity placements
through SNA IX. The
hidden unit is Tiny, one-HD magical beast with Str 3/Dex 19/Con 10/Int 1/
Wis 12/Cha 6, 40-foot airborne speed, Weapon Finesse and a zero-base-dice
touch carrier. Its Eagle donor is temporary rigging only. The 10-foot
ground speed is omitted because the engine exposes one unit speed. The
direct touch hit and reciprocal native attachment are live-qualified, as
is one actual Constitution drain tick and explicit link cleanup. The +8
maintain bonus is installed but has not been exercised in a native grapple
check. Four-point detachment passed guarded `20260928T0247164550495Z`: four
actual one-point losses and automatic reciprocal-part/buff cleanup. The 10%
disease chance uses one per-victim check on first actual drain; guarded
`20260928T0434428779441Z` forced an eligible exposure, observed a failed
native Fortitude save and applied exact `FilthFever` with retained DC 12,
then rejected a
repeat check and retained that result through another drain. Native cure
timing, original visual and icon remain open.
Guarded `20260928T0610511271269Z` passed native prey death through
`UnitLifeController` and Stirge release without another drain. It also
passed exact timed-marker expiry while attached, followed by the Stirge's
next round callback releasing both sides. The paused fixture did not observe
native destruction at that boundary; the existing turn-based summon-lifecycle
scenario is the separate native retirement witness. Native `Unlootable`
prevents the touch proboscis from becoming scene loot; exact cleanup passed.
The guarded working-save trio `20260928T0629198538034Z` / `0633408624544Z`
/ `0637555596273Z` saved an attached Stirge and Pony, then found both
freshly loaded without grapple parts, hold buffs or immobilizing conditions;
the cleanup save left zero fixture summons. This is a safe grapple reset on
load, as allowed by the accepted engine limitation, not preservation of an
active hold through reload.
Guarded
`20260928T0304163413890Z` passed the
victim's native break-free rule with controller-equivalent target-part removal
and a separate area-leave safeguard sweep. Actual controller timing remains
to be observed.
Guarded `20260928T0332419433992Z` passed destruction of an attached summon
after the native entity-destroyer queue advanced: victim held state and part
were gone with no extra damage. This exercises the shared teardown path,
not the timer's scheduling.
The registered primary
carrier now clones the native held-touch weapon; live inventory
`20260928T0113159269094Z` verifies `AttackType.Touch`, zero base dice and
the exact unit binding (49/49 assertions). This is structural touch-AC
qualification. Guarded own-tier combat `20260928T0142219533559Z` then
resolved a native hit at touch AC 6 against ordinary melee AC 14 with no
HP damage (0 to 0). Guarded combat `20260928T0232376506044Z` observed the
reciprocal native link, Stirge's lost Dexterity to AC, Constitution damage
0 to 1, cumulative meal 1, retained attachment and explicit clean release.
No full gameplay fidelity or publication is claimed. All nine choices stay
suppressed. The guarded
startup smoke `20260928T0029440999424Z` passed; the corrected live
inventory `20260928T0052069434655Z` passed 48/48 structural assertions
with exact installation restoration. Rules baseline:
[Paizo Stirge](https://legacy.aonprd.com/bestiary/stirge.html).
Original 512-vertex Stirge skinned mesh, rust-red/ochre 1024 px albedo,
four fleshy wings, six legs, forward proboscis and 0.25 view-only scale
are now installed on the private Eagle-donor renderer swap. Guarded
`20260928T0919388380835Z` passed exact attachment, 12.339 m native
travel/doorway crossing, render lifecycle and zero private view resources
after cleanup; its read-only overhead frame shows the creature with a
small furniture-occlusion patch. At that checkpoint the icon was source-only
and all nine placements were hidden.

Final guarded turn-based `20260928T1227208122214Z` and RTWP
`20260928T1230343024504Z` native UnitAttack runs passed touch hit,
reciprocal attach, forward proboscis aim and 0.05 m baked clearance outside
the target bounds. The final disposable run `20260928T1307116237968Z`
passed 33/33 assertions, including four-point drain, native DC 12 disease
exposure, escape, prey death, dismissal, timed expiry, area sweep, visual
attachment and exact cleanup. Its per-view rig has 15 bones and released
zero private meshes/materials in the lifecycle run.

The original 128 px Stirge icon is distinct from Giant Wasp and assigned
to the unit and all nine registered SNA choices; its source/export hashes and
exact symbols are in the icon manifest. The zero-damage touch weapon and attack
trait are mechanics-only; the hold buff is internal and Filth Fever keeps
its native icon. The nine choices are now hidden. The earlier guarded player path
`20260928T1344385887684Z` passed 834/834 generated roots and 29/29 native
wrappers. The no-save live inventory `20260928T1405023387599Z` passed 50/50
assertions, all 18 menu equations, zero missing or misordered icons and zero
prohibited references. Original installation restoration was exact after
both runs. That Sprint 10 technical PASS is reopened by the 2026-09-29 owner
correction: native target grapple suppresses prey movement incorrectly.
Stirge publication was withdrawn on this branch pending requalification; the
replacement attachment and removal route are required before
Sprint 10 can pass again. Native Filth Fever cure timing was not measured;
exposure uses the exact installed buff. Owner visual approval remains pending.

The [primary Paizo Stirge stat block](https://legacy.aonprd.com/bestiary/stirge.html)
explicitly says that a victim cannot be infected by the same Stirge once its
exposure check is made. This is the owner's specified exception to the proposed
per-drain change. Diagnostic `20260929T1228251279908Z` is superseded for
rules acceptance. The source now preserves the one-check-per-victim gate and
requires actual Constitution loss; zero loss does not consume the check.
Guarded Steam `20260929T1241118852906Z` passed 35/35, including an initial
zero-damage suppression, one native DC 12 Filth Fever exposure, and no repeat
check after a later actual drain. Filth Fever is the explicit single bounded
Kingmaker disease adaptation.

The corrected final implementation uses no reciprocal target grapple. The
Stirge owns a nonserialized session link and feeding state; the prey keeps
normal movement and actions, and can target and kill the Stirge.
Standard-action removal chooses the better current CMB or Mobility modifier
before rolling. Native prey movement, bounded follow, short-translocation
detach, independent quantity links, four-drain detach, all cleanup boundaries,
both turn modes, the original view/icon, 834/834 generated player paths, 29/29
native wrappers and the 50/50 inventory contract passed in the final evidence
set listed at the top of this matrix. Owner visual approval remains pending and
nonblocking.

## Phase 2 Sprint 11 ungulate qualification history, 2026-09-29 to 2026-09-30

| Creature | Registered role and mechanic | Live evidence | Remaining before publication |
| --- | --- | --- | --- |
| Aurochs | SM/SNA III meat/trample; original Horse-rig view | DC 17 native trample contact, successful Reflex half branch and same-round replay suppression; original view, 12.3 m doorway travel and natural 4-member group expiry | Victim choice, turn-based cadence, impact and UI/art review |
| Bison | SM/SNA IV heavier trample; original Horse-rig view | DC 20 native trample contact; original view, 12.3 m doorway travel and natural 4-member group expiry | Victim choice, turn-based cadence, impact and UI/art review |
| Rhinoceros | SM/SNA IV powerful charge; original Mastodon-rig view | Native `ChargeAbility` moved 3.88 m and queued first gore hit for 25 damage; original view, 12.3 m doorway travel and natural 4-member group expiry | Turn-based and quantity charge, impact and UI/art review |
| Woolly Rhinoceros | SM/SNA V stronger charge and trample; distinct original Mastodon-rig view | Native charge moved 3.42 m and queued first gore hit for 30 damage; DC 23 trample contact; original view, 12.3 m doorway travel and natural 5-member group expiry | Victim choice, turn-based and quantity charge, impact and UI/art review |

All four remain hidden. Guarded cast run `20260929T0713346420017Z` passed
16/16 SM/SNA `1d3`/`1d4+1` routes, 48/48 original-view attachments,
204/204 native casts, 280 spawned units, caster-level duration and exact
per-cast cleanup. This is sequential quantity casting, not formation travel.
The 35/35 run, 1,952-test build and exact restoration are indexed in
`EXPANDED-SUMMONING-PHASE2-EVIDENCE-INDEX.md`.

Guarded working-save group review `20260929T0805061632581Z` passed
28/28 assertions: SNA `1d4+1` groups of 5/5/4/2 Aurochs/Bison/Rhinoceros/
Woolly Rhinoceros moved all sixteen members 6.43–13.18 m on distinct native
paths to within 1.5 m of separate connected-floor destinations. All views,
single-subject doorway travel, group cleanup and zero save writes passed.
This closes simultaneous movement on the surveyed floor. Concurrent charge,
turn-based cadence and impact review remain open at that checkpoint.

Guarded working-save `20260929T0920575813821Z` passed 32/32: all 17
members in 4/4/4/5 simultaneous groups moved to distinct connected-floor
goals, then all canonical summon timers and units retired under natural game
updates (111.98–113.14 game seconds). Pause state, no-save-write cleanup and
exact original-install restoration passed. Concurrent charge, turn-based
cadence, victim AoO choice, impact contact and clear art review remain open.

The statements above are dated hidden-candidate checkpoints. The final
disposition is published and internally qualified. The automatic Trample
response run `20260929T2156585120329Z` passed the full AoO/Reflex
discrimination in RTWP and turn-based mode: one legal melee AoO at exactly -4,
normal resource consumption, no save, no ordinary duplicate, full damage after
hit or miss if the trampler continued, no contact or later damage after a
stopping response, and the Reflex full/half branch only when no response could
execute. Direct/quantity summons and player/hostile targets passed. Stampede
`20260929T2311031955741Z` passed the faithful active-command trio at +2 DC and
same-size eligibility, plus two-member, idle, separated, interrupted and
stale-command rejection in both modes.

Final corrected art uses compact cloven cattle hooves and articulated,
overlapping tapered rhinoceros leg spans with separate broad feet. Unobstructed
idle, motion, attack, overhead and four-direction oblique reviews accepted the
result; final Rhino review `20260930T0350131904135Z` passed 18/18. Quantity
review `20260930T0400494836310Z` passed 32/32 with distinct selectable units,
simultaneous native travel, original renderers, natural expiry and zero
resource/save residue. Final rules, visual-contract and lifecycle results
`20260930T0422334006971Z`, `20260930T0426570686599Z` and
`20260930T0430006142872Z` passed 58/58, 15/15 and 7/7.

Publication inventory `20260930T0457205706691Z` passed 50/50 with all 882
placements visible/executable, all 18 menu equations, exact icons and zero
prohibited references. Player path `20260930T0508037596496Z` passed 10/10
across 882 roots and 29 wrappers. Working-save prepare, cleanup and absence
`20260930T0524086567797Z`, `20260930T0528280228527Z` and
`20260930T0532419916125Z` passed 15/15 each, with exact publication, identity,
context, duration, control, safe session-link reset, native expiry and final
zero residue. Restoration record `20260930T0535320622642Z` verifies the
original 136-file installation exactly. Owner visual approval is
NOT_PERFORMED_NONBLOCKING.

## Phase 2 Sprint 12 hidden mechanics checkpoint, 2026-09-30

All 68 Dire Rat, Dog, Hyena and Goblin Dog placements remain suppressed. The
Dire Rat and Goblin Dog mechanics candidate passed focused guarded run
`20260930T1234331263883Z-disposable-expanded-summoning` at 39/39 assertions.
Dire Rat required an exact hit and positive bite damage, then exercised both
DC 11 save outcomes against the exact native Filth Fever payload, a miss, and
same-event replay suppression. Goblin Dog exercised disease immunity, both DC
12 outcomes, exact one-day duration and -2/-2 penalties, replacement stacking,
ordinary-healing preservation, positive magical-healing removal, native Remove
Disease removal, and the exact native-Goblin exemption. Separate quantity
sources retained their own caster/victim contexts, and exact cleanup passed.

This qualifies only the focused injury-disease contracts. Dog and Hyena still
use their checked-in bite/trip profiles, but all four need original distinct
silhouettes and the complete direct/quantity player-path, RTWP/turn-based,
movement/contact, natural-expiry, save/load, module-disabled, compatibility,
inventory and internal visual matrix before publication.

The later hidden-view candidate adds deterministic original Dire Rat, Hyena
and Goblin Dog meshes/albedos while preserving the audited native Dog view.
Guarded run `20260930T1626175528422Z-disposable-expanded-summoning` passed
41/41 direct, quantity, attachment and lifecycle assertions. This closes the
basic live renderer-attachment check only; all 68 placements remain
suppressed, and Sprint 12 remains unpublished until direct/quantity player
paths, both combat modes, movement/contact, natural expiry, save/load, module
disable, compatibility, inventory and the complete internal visual review
pass. The 0.0.141 release boundary is Sprints 9-11: 832 published generated
choices and 861 total choices including 29 retained native wrappers.
