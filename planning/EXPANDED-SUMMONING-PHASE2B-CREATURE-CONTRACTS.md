# Expanded Summoning Phase 2B creature contracts (Sprints 14-17)

The owner's acceleration order asks for the primary-source creature contract to
be frozen before implementation, and for donor and rig research to run once per
tranche rather than once per creature. This document is that freeze. Every stat
block below is transcribed from the Pathfinder reference text captured on
2026-10-01; anything the implementation later cannot deliver exactly must be
raised against this page rather than quietly adapted.

Placement tiers come from `ExpandedSummoningIdealRosterCatalog`, which already
carries each creature's Summon Monster and Summon Nature's Ally tier and its
sprint. A creature with a tier in only one family is published in that family
alone.

| Sprint | Creature | Key | SM | SNA | Rig family |
| --- | --- | --- | ---: | ---: | --- |
| 14 | Fire Beetle | `fire-beetle` | 1 | 1 | Insect |
| 14 | Giant Ant (Worker) | `giant-ant-worker` | 2 | 2 | Insect |
| 14 | Giant Ant (Soldier) | `giant-ant-soldier` | 3 | 3 | Insect |
| 15 | Giant Ant (Drone) | `giant-ant-drone` | 4 | 4 | Insect |
| 15 | Giant Stag Beetle | `giant-stag-beetle` | - | 4 | Insect |
| 16 | Crocodile | `crocodile` | 3 | 3 | Crocodilian |
| 16 | Dire Crocodile | `dire-crocodile` | 7 | 7 | Crocodilian |
| 17 | Viper | `viper` | 1 | 1 | Snake |
| 17 | Constrictor Snake | `constrictor-snake` | 3 | 3 | Snake |
| 17 | Salamander | `salamander` | 5 | - | Outsider |

Two of the ten already exist in the shipped catalog as **visual proxies**:
`crocodile` is published wearing the Monitor Lizard, and `salamander` wearing
the Lizardfolk. Sprints 16 and 17 therefore replace a proxy on an identity that
already ships, which means their placements and GUIDs must be preserved exactly
and the work is art and mechanics rather than registration.

## Charter errata confirmed against the primary source

Three charter statements do not survive the source text and are corrected here
rather than carried into implementation.

1. **The Fire Beetle deals no fire damage.** Its only attack is a bite for 1d4,
   and its signature ability is luminescence: glowing glands that light a
   10-foot radius and keep glowing for 1d6 days after death. A fire-damage bite
   would be stronger than the printed creature.
2. **The Fire Beetle and the Giant Stag Beetle both fly** - 30 feet poor and 20
   feet poor respectively. "Ground insect family" is therefore not a rules fact,
   and the rig family cannot be chosen on the assumption that these creatures
   are ground-bound.
3. **The Dire Ape has a bite and two claws with rend, not dual slams.** That
   creature belongs to Sprint 18, but the erratum is recorded here because it
   was found in the same reading pass.
4. **The Giant Ant Drone's advanced simple template is applied as a rebuild,
   not as a rebuild plus the quick bonuses.** The Sprint 15 section below
   describes that template as "+4 to each ability score except Intelligence,
   +2 natural armour, and +2 to all skills", and also requires that the exact
   numbers be derived from the soldier. Those two instructions conflict: a
   simple template's flat bonuses and its rebuild are alternative routes to the
   same creature, so a +2 to all skills applied on top of skills already
   rederived from a Wisdom raised by 4 counts the same increase twice. The
   implementation takes the rebuild this section demands and omits the flat
   skill bonus, which puts the Drone's Perception at +7 rather than +9. Nothing
   else in the section is affected, and the ability scores, natural armour and
   fly speed are implemented exactly as written.

## Sprint 14

### Fire Beetle - CR 1/3, N Small vermin

Init +0. Low-light vision. Perception +0.
AC 12, touch 11, flat-footed 12 (+1 natural, +1 size). hp 4 (1d8).
Fort +2, Ref +0, Will +0. Immune mind-affecting effects.
Speed 30 ft., fly 30 ft. (poor).
Melee bite +1 (1d4).
Str 10, Dex 11, Con 11, Int -, Wis 10, Cha 7. CMD 9 (17 vs. trip).

Signature: **Luminescence (Ex)** - the glowing glands light a 10-foot radius,
and continue to glow for 1d6 days after the beetle dies. The creature is
nocturnal and has no darkvision; it relies on its own glands to see.

The after-death glow has no mechanical consumer in a summon whose body vanishes
with it, so the implementable part is the light itself. Sprint 13 established
that Kingmaker has **no mechanics-layer illumination model at all**, so this is
the sprint's open engineering question and must be answered before the
candidate: either a visual light source on the creature's own view, or an
owner-visible statement that the game cannot carry it.

### Giant Ant (Worker) - CR 1, N Medium vermin

The soldier below with the **Worker** template applied: no poison sting and no
grab. That leaves a bite alone.

### Giant Ant (Soldier) - CR 2, N Medium vermin

Init +0. Darkvision 60 ft., scent. Perception +5.
AC 15, touch 10, flat-footed 15 (+5 natural). hp 18 (2d8+9).
Fort +6, Ref +0, Will +1. Immune mind-affecting effects.
Speed 50 ft., climb 20 ft.
Melee bite +3 (1d6+2 plus grab), sting +3 (1d4+2 plus poison).
Str 14, Dex 10, Con 17, Int -, Wis 13, Cha 11. CMD 13 (21 vs. trip).
Feats Toughness (bonus). Racial +4 Perception, +4 Survival.

Signature: **Poison (Ex)** - sting, injury; Fort DC 14; frequency 1/round for 4
rounds; effect 1d2 Str; cure 1 save. And **grab** on the bite.

Both have qualified project carriers already: the grab lifecycle from Sprint 4
and 6, and the injury-poison lifecycle from the Giant Wasp and the Giant
Spider. This is the sprint's main reuse opportunity and the reason the three
creatures are grouped.

## Sprint 14 architecture review, 2026-10-02

Done before the candidate rather than after it, because the cheapest place to
discover that a signature mechanic was misunderstood is here. The finding is
that **Sprint 14 needs no new mechanism**: two carriers this project already
qualified compose to give exactly what the three stat blocks ask for.

### The soldier's grab and its poison are on different attacks, and that works

The Giant Ant Soldier bites with a grab and stings with a poison, and nothing
may leak between them. Two independent gates already exist and they gate on
different things:

- **Grab is gated by limb position.** `SummonGrabComponent` takes
  `GrabWithPrimaryHand` and `GrabAdditionalLimbCount`, and `ConfigureGrabber`
  is keyed by unit symbol, so it is available to a Nature's Ally creature - the
  Monitor Lizard, Grizzly Bear, Leopard and Lion are all on this path already.
  The soldier takes `Primary: true, Additional: 0`, which puts the grab on the
  bite and nowhere else.
- **Poison is gated by weapon type.** The Giant Wasp's
  `AddInitiatorAttackWithWeaponTrigger.WeaponType` is set to its sting's own
  type, so the trigger fires only for that weapon. The soldier's bite derives
  from a native bite template and its sting from the native sting template, so
  they are different types and the poison cannot reach the bite even though the
  sting sits in an additional limb slot.

The two gates are orthogonal, so the bite grabs without poisoning and the sting
poisons without grabbing. This is the reuse the sprint was grouped around and
it survives the review.

### The soldier's poison is the Giant Spider's poison, exactly

Primary source, Giant Ant (Soldier): sting, injury; Fort DC 14; frequency
1/round for 4 rounds; effect 1d2 Str; cure 1 save. The native Giant Spider
poison Kingmaker already ships is the same graph in every field but the damaged
ability, so the Wasp's construction applies with `Stat` set to Strength,
`Ticks` 4 and `SuccesfullSaves` 1.

DC 14 is also what the Constitution-scaled formula produces on its own: 10 +
half of 2 hit dice + a +3 Constitution modifier. The DC is therefore not
hard-coded; it is scaled the way the Wasp's is and it lands on the printed
number. If a future change to the chassis moves it off 14, that is a defect the
rules gate will catch rather than something a constant would hide.

### The worker is a template, not a creature

The Worker is the Soldier with the Worker template applied: no poison sting and
no grab, which leaves a bite alone. So it takes the same chassis with the sting
limb and both carriers absent. Nothing about it needs its own graph, and the
one thing a player must be able to see - which caste is in front of them - is
carried by the mesh and the painting rather than by a tooltip.

### Luminescence is a view-local light and is not a mechanics claim

Sprint 13 established that Kingmaker has **no mechanics-layer illumination
model at all**: nothing in the rules layer consults light level, so a 10-foot
radius of light cannot grant or deny anything. The source's after-death glow
has no consumer either, because a summon's body vanishes with it.

What remains implementable is the light itself, and the owner has authorized a
view-local one on the conditions that it be instance-owned, cleaned up,
bounded, and visually useful. The existing visual patch already owns
project-created Unity objects per view and destroys them on
`UnitEntityView.OnDestroy`, which is the same ownership the light needs, so the
light is created beside the swapped renderer, registered on the same attachment
record, and destroyed in the same teardown. It is bounded by an explicit range
and intensity rather than by anything the rules consult.

**No record may claim that a mechanics-layer illumination system exists.** The
light is a visual that matches the creature's painted glands; the tooltip says
the beetle glows, and says nothing about what that does, because it does
nothing.

### What is left genuinely open

Only whether the donor's real gait and flight animation carry the six-legged
ant and the winged beetle, which no amount of source reading can answer and
which the sprint's single batched runtime review exists to settle.

## Sprint 15

### Giant Ant (Drone) - CR 3, N Medium vermin

The soldier with the **Drone** template: the advanced simple template and a fly
speed of 30 feet (average). The advanced template is +4 to each ability score
except Intelligence, +2 natural armour, and +2 to all skills; the exact numbers
must be derived from the soldier above and written down in the profile rather
than assumed.

### Giant Stag Beetle - CR 4, N Large vermin

Init +0. Darkvision 60 ft. Perception +0.
AC 17, touch 9, flat-footed 17 (+8 natural, -1 size). hp 45 (7d8+14).
Fort +7, Ref +2, Will +2. Immune mind-affecting effects.
Speed 20 ft., fly 20 ft. (poor).
Melee bite +8 (2d8+6). Space 10 ft.; Reach 5 ft.
Special Attacks **trample (1d6+6, DC 17)**.
Str 19, Dex 10, Con 15, Int -, Wis 10, Cha 9. CMD 20 (28 vs. trip).

Trample has a qualified project carrier from the Sprint 11 ungulates.

## Sprint 16

### Crocodile - CR 2, N Large animal

Init +1. Low-light vision. Perception +8.
AC 14, touch 10, flat-footed 13 (+1 Dex, +4 natural, -1 size). hp 22 (3d8+9).
Fort +6, Ref +4, Will +2.
Speed 20 ft., swim 30 ft.; sprint.
Melee bite +5 (1d8+4 plus grab) and tail slap +0 (1d12+2).
Space 10 ft.; Reach 5 ft.
Special Attacks **death roll (1d8+6 plus trip)**.
Str 19, Dex 12, Con 17, Int 1, Wis 12, Cha 2. CMD 18 (22 vs. trip).
Feats Skill Focus (Perception, Stealth). SQ hold breath.

Signatures: **Death Roll (Ex)** - while grappling a foe of its size or smaller,
on a successful grapple check the crocodile inflicts its bite damage, knocks the
creature prone, and keeps the grapple. **Sprint (Ex)** - once per minute, land
speed rises to 40 feet for one round. **Hold breath** - rounds equal to four
times its Constitution score.

Death roll is the sprint's new mechanic: it rides the already-qualified grapple
lifecycle but adds a per-grapple-check effect that both damages and knocks
prone without ending the hold.

### Dire Crocodile - CR 9, N Gargantuan animal

Init +4. Low-light vision. Perception +14.
AC 21, touch 6, flat-footed 21 (+15 natural, -4 size). hp 138 (12d8+84).
Fort +15, Ref +8, Will +8.
Speed 20 ft., swim 30 ft.; sprint.
Melee bite +18 (3d6+13/19-20 plus grab) and tail slap +13 (4d8+6).
Space 20 ft.; Reach 15 ft.
Special Attacks **death roll (3d6+19 plus trip)**, **swallow whole (3d6+13, AC
16, 13 hp)**.
Str 37, Dex 10, Con 25, Int 1, Wis 14, Cha 2. CMD 36 (40 vs. trip).
Feats Improved Critical (bite), Improved Initiative, Iron Will, Run, Skill
Focus (Perception, Stealth). SQ hold breath.

Swallow whole has a qualified project carrier from the Purple Worm.

## Sprint 17

### Viper (Venomous Snake) - CR 1, N Medium animal

Init +5. Low-light vision, scent. Perception +9.
AC 14, touch 11, flat-footed 13 (+1 Dex, +3 natural). hp 13 (2d8+4).
Fort +5, Ref +4, Will +1.
Speed 20 ft., climb 20 ft., swim 20 ft.
Melee bite +2 (1d4-1 plus poison).
Str 8, Dex 13, Con 14, Int 1, Wis 13, Cha 2. CMD 11 (can't be tripped).
Feats Improved Initiative, Weapon Finesse (bonus).

Signature: **Poison (Ex)** - bite, injury; Fort DC 13; frequency 1/round for 6
rounds; effect 1d2 Con; cure 1 save.

Note the bite's **negative damage bonus**, 1d4-1, which follows from Strength 8
and must not be rounded up to 1d4.

### Constrictor Snake - CR 2, N Medium animal

Init +3. Scent. Perception +12.
AC 15, touch 13, flat-footed 12 (+3 Dex, +2 natural). hp 19 (3d8+6).
Fort +4, Ref +6, Will +2.
Speed 20 ft., climb 20 ft., swim 20 ft.
Melee bite +5 (1d4+4 plus grab).
Special Attacks **constrict (1d4+4)**.
Str 17, Dex 17, Con 12, Int 1, Wis 12, Cha 2. CMD 18 (can't be tripped).
Feats Skill Focus (Perception), Toughness.

Constrict has a qualified project carrier from Sprint 6.

### Salamander - CR 6, CE Medium outsider (extraplanar, fire)

Init +1. Darkvision 60 ft. Perception +16.
AC 18, touch 11, flat-footed 17 (+1 Dex, +7 natural). hp 76 (8d10+32).
Fort +10, Ref +7, Will +6. DR 10/magic. Immune fire. Vulnerability to cold.
Speed 20 ft.
Melee spear +11/+6 (1d8+4/x3 plus 1d6 fire), tail slap +6 (2d6+1 plus 1d6 fire
and grab).
Space 5 ft.; Reach 5 ft. (10 ft. with tail).
Special Attacks **constrict (2d6+4 plus 1d6 fire)**, **heat**.
Str 16, Dex 13, Con 18, Int 14, Wis 15, Cha 13. CMD 22 (can't be tripped).
Feats Cleave, Iron Will, Power Attack, Skill Focus (Perception).
Languages Common, Ignan.

Signature: **Heat (Ex)** - the salamander's mere touch deals an additional 1d6
fire damage, and its metallic weapons conduct that heat too.

Three things make this the hardest creature in the tranche. It **wields a
manufactured weapon**, a spear, rather than fighting with natural attacks
alone. Its tail reaches ten feet while its body reaches five, so it has two
different reaches. And heat applies to the spear as well as to the body, which
means the fire rider belongs on the wielder rather than on a single weapon
blueprint.

## Tranche donor census, answered 2026-10-02

Run `20261002T1440160968919Z-observe-expanded-summoning-native-donors`, 4/4
PASS on `cbfc7c13`, live tree restored to the 136-file baseline. It enumerated
599 native units, 2,931 facts, 356 abilities and 426 buffs, and the evidence
file `native-donor-audit.json` is the record. This is the tranche's donor
research: later sprints read it rather than relaunching to rediscover a fact
already captured.

### The game has no beetle, ant, crocodile or snake at all

Searching all 599 units for beetle, ant, crocodile, alligator, snake, viper,
serpent, python and cobra returns **nothing native**. The only matches are the
project's own `KMG_Summoning_Unit_Crocodile`, which wears the Monitor Lizard,
and one `SerpentineWaterElementalEidolonUnit`. Every creature in this tranche
therefore needs an original mesh on a borrowed rig, exactly as Sprints 9 to 13
did; none of them can be adopted from a native creature of the same species.

### The rig families that do exist

| Family | Donor | Prefab | Body plan |
| --- | --- | --- | --- |
| Insect | Giant Spider | `54e0335882f2dea4188214ea` | Compact many-legged arthropod, Medium |
| Insect, elongated | Giant Centipede | `bf09aef8864bed844a2353f7` | Segmented, many-legged, Medium and Huge |
| Crocodilian | Monitor Lizard | `2310dff1067d2d34caa2015e` | Four-legged lizard with a tail, Medium |
| Serpentine | Serpentine Water Elemental Eidolon | `dc296683c2a3d2648afa516a` | Limbless, Medium, bite plus tail |
| Serpentine with limbs | Tatzlwyrm | `c6b7d72e25039f44e9801e8c` | Large, bite and two claws |
| Humanoid reptile | Lizardfolk | `9b1744531a4428e44aa9837c` | Biped with a weapon hand and a bite |

**The insect shared-rig hypothesis holds.** A beetle and an ant are both
compact arthropods with a segmented body and mandibles, which is the Giant
Spider's shape rather than the centipede's; all five Sprint 14 and 15 creatures
can ride that one rig, and the Giant Stag Beetle rides it at Large. That makes
the grouping of those two sprints real rather than nominal.

**The snake family has no good rig.** The only limbless native body is the
Serpentine Water Elemental Eidolon, which is Medium and carries a bite and a
tail - the right shape for the Viper and the Constrictor Snake, and the only
candidate. Its suitability has to be confirmed from a live bind frame before
Sprint 17 commits to it.

**The Salamander has no rig of its own shape either.** It is a humanoid torso
on a serpentine tail, and nothing native is both. The Lizardfolk it already
wears is the closest available.

### Two creatures are already most of the way there

The census shows both proxies are far more complete than "a proxy" suggests,
which changes what Sprints 16 and 17 actually have to do.

- `KMG_Summoning_Unit_Crocodile` already carries `BiteDragonLarge1d8` with a
  `KMG_Summoning_Natural_Tail1d12` secondary, `NaturalArmor4`, `ReducedReach`,
  `TripDefenseFourLegs` and both Skill Focus feats, on the Monitor Lizard
  prefab. Sprint 16 is therefore an original mesh, the death roll, the sprint
  burst, grab and hold breath - not a registration.
- `KMG_Summoning_Unit_Salamander` already carries its own spear and tail
  weapons, `NaturalArmor7`, `DRMagic10`, `SubtypeFire`, Weapon Focus (spear)
  and a combat-traits carrier. Sprint 17 is an original mesh, the heat rider
  and constrict.

### Carriers for the signature mechanics

- **Sprint**: the project already owns
  `KMG_Summoning_Special_Cheetah_Sprint`, which is the crocodile's once-per-
  minute speed burst in all but name. Reuse rather than reinvent.
- **Swallow whole**: `PurpleWormSwallowWholeFeature`
  (`dee864aec4a0d344b913dd27a4b504cb`) exists natively and the project already
  uses it, which covers the Dire Crocodile.
- **Constrict**: the native constrict features are all summoner eidolon
  evolutions (`ConstrictEvolutionFeature`
  `9e4080be49044200b1543e620c54b923` and its selection wrappers). The project
  already has its own constrict from Sprint 6, which is the better carrier.
- **Death roll**: nothing native. Project-owned, riding the qualified grapple
  lifecycle.
- **Heat**: nothing usable. The fourteen matches are all oracle and shaman hex
  features. Project-owned.
- **Luminescence**: nothing native, and Sprint 13 already proved Kingmaker has
  no mechanics-layer illumination model. This remains the tranche's one open
  engineering question and is answered in Sprint 14 before its candidate.

## The Giant Spider rig, measured 2026-10-02

Run `20261002T1916589198635Z-disposable-expanded-summoning`, 51/51 PASS on
`69e33417`, live tree restored. The donor's own bind frame is
`sprint14-giant-spider-bind-rig.json`: one skinned renderer, 51 bones, rooted
at `Position`, in a Z-up frame whose forward is negative Y - the chelicerae and
pedipalps sit at y between -0.28 and -0.48, so the mouthparts are the front.

| Group | Bones | What it is |
| --- | --- | --- |
| `Position` -> `LowerTorso` | 2 | Root and cephalothorax |
| `Tail1_M` -> `UpperTorso` -> `Tail3_M` | 3 | The abdomen, carried up and back |
| `L/R_Leg0..3_Upper/Lower/Foot` | 24 | **Eight legs**, three bones each |
| `pedipalp1..7_L/R` | 14 | Two seven-bone palps sweeping forward and down |
| `femur1..3_L/R` | 6 | A short three-bone chain per side at the leg roots |
| `chelicera_L/R` | 2 | **The fangs**, at the front of the cephalothorax |

### What this settles for the ground slice

The hypothesis survives, and better than expected.

- **Six legs out of eight is subtractive, not additive.** `Leg0` to `Leg2` on
  each side give exactly the six an ant needs, in three bones each. `Leg3_L`
  and `Leg3_R` simply receive no geometry: a bone with no vertices weighted to
  it draws nothing, so there is no eighth leg to hide and no phantom contact to
  suppress. The donor's animation still moves those two bones; nothing is
  attached to them. What the ground slice has to judge is therefore not whether
  a spider leg shows through, but whether a six-legged body walking on a
  spider's gait timing reads as an ant.
- **The chelicerae are mandibles.** `chelicera_L` and `chelicera_R` sit at the
  front of the cephalothorax and are exactly where an ant's mandibles go, which
  gives the bite a real articulated carrier rather than a static lump.
- **The pedipalps are antennae.** Seven bones each, sweeping forward and down,
  is more articulation than an antenna needs and far more than the donor's own
  palps would suggest. They are the one part of this rig that will clearly
  move, which is what an ant's antennae should do.
- **The abdomen chain is the gaster.** `Tail1_M` is the petiole, `UpperTorso`
  and `Tail3_M` the gaster, which is the ant's most recognisable shape after
  its mandibles.

### What this leaves open for the flying slice

**The rig has no wing bones at all.** Nothing in the 51 is a wing, and the
abdomen chain is the only thing above the body. So a beetle's elytra and the
membranous wings beneath them have to be authored onto bones that were never
meant to carry them - the abdomen chain for the elytra, and the pedipalps,
which are the only strongly articulated pair, for the wings themselves.

That is a real risk and it is the reason the owner's order asks for a flying
slice before five models are authored. Elytra on the abdomen will hold still,
which is correct for wing covers; wings on the pedipalps will sweep forward and
down rather than beating, which is not what a wingbeat looks like. If the
flying slice cannot be made credible on this rig, the ground and flying insects
split and the flyers take a different donor, rather than the family being
forced together because one census found one arthropod.

## Open questions to answer before the Sprint 14 candidate

Questions 2 to 4 are answered by the census above. What remains open is the
one the census could not settle and one it raised.

1. **Can Kingmaker carry the Fire Beetle's luminescence at all?** Sprint 13
   proved there is no mechanics-layer illumination model, and the census found
   no native carrier. A view-local light source on the creature's own renderer
   is the likely answer; if none is possible the limitation is owner-facing and
   must be raised rather than dropped.
2. **Does the Serpentine Water Elemental Eidolon's rig actually suit a snake?**
   It is the only limbless Medium body in the game, so Sprint 17 has no second
   choice, and its bind frame must be captured and inspected before that sprint
   commits to it. If it does not suit, the snakes are blocked on an owner
   decision rather than quietly reshaped into something else.
