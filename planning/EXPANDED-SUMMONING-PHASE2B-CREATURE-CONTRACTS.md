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

## Open questions to answer before the Sprint 14 candidate

1. Can Kingmaker carry the Fire Beetle's luminescence at all? Sprint 13 proved
   there is no mechanics-layer illumination model. A view-local light source is
   the likely answer; if none is possible, the limitation is owner-facing and
   must be raised rather than dropped.
2. Which native donors carry the insect, crocodilian and snake rigs, and do the
   three ant castes genuinely share one rig? The tranche donor census answers
   this once. If a family's shared-rig hypothesis fails, the implementation
   splits rather than forcing reuse.
3. Does any native creature already carry a death roll, a sprint burst, or the
   salamander's heat, or must each be project-owned?
4. For the two proxy replacements, exactly which placements and GUIDs are
   already published and must be preserved unchanged.
