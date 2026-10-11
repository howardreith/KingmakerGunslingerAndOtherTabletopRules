# 0.0.150-expanded-summoning-sprint21

Kingmaker Gunslinger 0.0.150 is the Expanded Summoning Sprint 21 candidate and
the last of the series: the Giant Crab, and a full overhaul of the released
Bebelith. It retains the qualified native Wwise firearm bank, SHA256
`0E9F88C562F4F937A8941ACE0F241BB31A7ED56B46FBCA549C98F764392EDF18`, and changes
no firearm audio asset. The installation artifact is
`KingmakerGunslinger-0.0.150-expanded-summoning-sprint21.zip`. Existing
`CraftMagicItems.dll` compatibility is inherited; no new crafting integration is
claimed. Historical content and fatigue-authority records retain their 1,288 and
1,325 test counts; neither describes this candidate's larger current suite.

It is built on the released v0.0.149 master, commit
`d0ceafae75c1d74a1e0d4fa318193ef002240da1`. Sprint 18's Ape and Dire Ape,
Sprint 19's Girallon and Xill and Sprint 20's Giant Scorpion stay exactly as
v0.0.149 published them: their roots, their identities, their rules and their
ten shipped body files are all inside this sprint's protected boundary.

## Two creatures at different stages of life

| | Summon Monster | Summon Nature's Ally | Roots | State |
| --- | ---: | ---: | ---: | --- |
| Giant Crab | — | III | 7 | New |
| Bebelith | VII | — | 3 | Released in v0.0.147; rebuilt here |

That difference shapes everything. The Giant Crab is new in every respect and
the question about it is whether it arrives correct. The Bebelith is already
registered, published and runtime qualified — a player can summon one today —
and the question about it is whether an overhaul that deliberately keeps its
identity has actually changed its mechanics.

Its identity does not move. The unit and all three Summon Monster roots keep
the exact GUIDs they shipped with, at the same tiers and quantities, published
as they already were, so a save made against v0.0.149 resolves every one of
them. The released spelling is kept as well: a source may print "Bebilith",
and renaming the shipped key would retire a GUID and migrate every save that
holds one. Changing the code behind a stable identity is a creature
correction; changing the identity would be a migration.

Sprint 21 appends 23 identities: one unit, seven placements and five facts for
the crab, and ten for the Bebelith. Registered generated placements rise from
1056 to 1063 and the ledger from 3039 to 3062.

## The contract corrected itself first

The frozen contract is at schemaVersion 2 and records what it supersedes. Its
first draft scoped the Bebelith to a body and mis-copied three Giant Crab
numbers: Dexterity 13 rather than 12, and natural armour 5 rather than 4 —
two errors that cancelled into the same armour class of 16 and so looked right
— and constrict omitted outright. It also recorded a racial +8 Swim the block
does not print, which is removed: recording more than the source says is as
wrong as recording less.

## Printed profiles

**Giant Crab**, N Medium vermin (aquatic), 3d8+6 (19 hp), AC 16/11/15, Fort +5
Ref +2 Will +1, immune to mind-affecting effects, speed 30 ft., swim 20 ft.,
two claws +4 (1d4+2 plus grab), Space 5 ft., Reach 5 ft., constrict 1d4+2, Str
15 Dex 13 Con 14 Int — Wis 10 Cha 2, BAB +2, CMB +4 (+8 grapple), CMD 15 (27
vs. trip), Perception +4, water dependency.

Both claws are primary, at the same bonus, and each adds the whole Strength
modifier rather than one and a half times it. With no Intelligence score there
are no ranks and no feats, so every number falls out of hit dice, ability
scores, size and racial bonuses alone, and all sixteen derivations closed at
intake.

**Bebelith**, CE Huge outsider (chaotic, demon, evil, extraplanar), 12d10+84
(150 hp), AC 22/9/21, Fort +15 Ref +11 Will +7, DR 10/good, speed 40 ft., climb
20 ft., bite +19 (2d6+9 plus rot) and two claws +19 (2d4+9, crit 19–20), Space
15 ft., Reach 15 ft., Str 28 Dex 12 Con 24 Int 11 Wis 13 Cha 13, BAB +12, CMB
+23, CMD 34 (46 vs. trip), web (ranged +11, DC 23, 12 hp), Dismantle Armor,
Penetrating Strike, rot (bite, injury, Fort DC 23, 2 Constitution, five
exposures, cure 2 consecutive saves), racial +8 Stealth.

## What the Bebelith overhaul changes

**Rot** is a bite-only injury effect on the engine's own poison lifecycle,
cloned and rewired exactly as the viper's, the ant's and the scorpion's
poisons are. Three of the four rewired values are what make it rot rather than
a poison: two Constitution flat on each failed save, five exposures — the bite
round and four after it — and two consecutive successes to cure, where every
poison this project ships cures on one. That last value is the whole reason it
cannot share a carrier. The difficulty class is never written down: a context
action sets it live from the creature's own Constitution immediately before the
game's own save, so an already-applied rot keeps the number the bite that
caused it produced even after the Bebelith is dismissed or killed.

Its two asset ids were the sprint's most expensive mistake. They were written
from memory rather than copied from the released builder that already clones
them, and the poison FEATURE's id ended up in the BUFF's constant. Nothing
offline noticed — the gate checks that the clone happens and the suite checks
the rewired values, and both are satisfied by a pair of plausible
thirty-two-character strings. The game refused it in the first second of mod
initialization and rolled back 2,432 registrations. The fix ships with a test
that is an equality between two files rather than a new pin.

**Penetrating Strike** replaces an invented +2 against chaotic evil outsiders —
which is neither what the ability says nor something the printed block can
produce — with the printed descriptors. Every natural attack counts as chaotic
and as magic; cold iron and good are added against demons only. That asymmetry
is invisible against a demon and wrong against everything else, so the live
review's case that can see it hits an ordinary target and reads the damage
bundle back.

**Dismantle Armor** is the sprint's highest-risk mechanic and the one place the
contract could not be met literally. The printed sequence is a combat manoeuvre
check and then a Reflex save; the released build had neither and went straight
to an invented difficulty class of 25. The check is now the engine's own
sunder-armour manoeuvre, which is also what keeps the whole feature away from
equipment, and the save is the printed 23 derived from the same formula rot
uses.

What cannot be done is recorded rather than approximated. A reflection pass
over the game's own assembly finds no broken condition, no durability value and
no item-damage component a mod can reach: `CombatManeuver.SunderArmor` exists as
a manoeuvre the engine will roll, and nothing behind it. Building the missing
half would be the global item-durability system this sprint is forbidden to
invent, so the piece keeps existing and stops protecting — its entire
armour-class contribution gone, permanently, computed live from the equipped
item rather than stored, which is what lets a permanent state survive a save
and a reload. Every equipment question the mechanic asks is a read. Recorded as
`ITEM_DESTRUCTION_UNMODELED`: the object survives, so the wearer keeps its
armour check penalty, maximum Dexterity limit, arcane spell failure chance and
any enchantment that is not an armour-class bonus.

**The web** reuses the Sprint 6 seam rather than a second copy of one. That
seam is now parameterized, and the Giant Spider's five released values are
checked by name on the new code path — which proves the spider's web arrives at
the numbers it already had, rather than merely that the old path is gone. The
Bebelith also gets a brain that can cast: the released creature took the native
brain that casts nothing, so an ability alone would have been an ability the
creature never used.

**Three carriers** exist because printed lines had a derivation and no carrier,
which is the defect class Sprint 20 paid two guarded runs to find. The printed
+12 against trip is the creature's own, because the shared native eight-leg
fact delivers +8 — the stat-block convention is four per pair of legs beyond
the first, so a six-legged insect prints +8 and an eight-legged creature prints
+12. The printed racial +8 Stealth existed nowhere. And the chassis correction
below.

## What the guarded reviews measured

Everything in this section was read back off a live creature in a running game,
in both real time and turn-based combat.

The first review found three numbers the native outsider chassis cannot
produce, and they are all one finding: the chassis this Bebelith stands on is
not the chassis its block was written for. A live creature read Fortitude +11,
Reflex +11 and Will +11 against a printed +15, +11 and +7 — Reflex right, and
the other two assigned to each other's slots — and 162 hit points against a
printed 150, with the same 84 from Constitution in both, so the whole
difference is one point a die in the racial half.

One creature-owned carrier corrects all three, and every value in it is a
difference from what the engine actually applied rather than a number typed to
make a total come out. The shared native outsider class is untouched: it is a
dozen released creatures' chassis, and correcting one of them by editing what
all of them stand on is how a sprint reaches past its own boundary.

That review also proved the hard half of penetrating strike correct, and the
authored bodies entirely.

## Original bodies

Both creatures ride the Giant Spider rig, and the census that decided it
surveyed them independently — separate anchors, discovery terms and prefab
budgets — because the easiest wrong decision available was to notice that both
are arthropods, survey once, and author both against whichever rig the single
survey liked. They landed on the same skeleton for different measured reasons:
the Bebelith because its released Doomspider donor is that same 51-bone rig
bone for bone and the only other demonic candidate the library offers is a
biped, and the crab because centipedes report four leg pairs of nothing and
this is the only rig in the installation with eight real leg chains and two
forward articulated ones.

The Bebelith's body is the first in the series authored for a creature that
already ships wearing something, so its authoring problem is inverted: not what
the rig can carry but what the body must do so it is not a repaint. It walks on
**six** — the fourth leg chain carries no geometry at all, and the generator
refuses to build a body that uses it — carries two elevated barbed foreclaws on
the pedipalp chains, has a real jaw on the two cheliceræ, and ends in
spinnerets. The crab's width over length is measured from the built mesh and
must exceed one, which no other creature on this rig does, and its generator
refuses to build a crab that reaches the bone a spider's abdomen hangs on.

Recorded limitations: `BEBELITH_ABDOMEN_RIDES_A_TWO_BONE_CHAIN`, so the
spinnerets cannot aim; and `CRAB_WALKS_A_SPIDER_RIG_FRONT_TO_BACK`, so the
stance reads laterally but the walk cycle is the donor's.

## Accepted limitations

- `PASSIVE_CREATURE_SENSES_UNMODELED` — darkvision and scent on both creatures.
- `ORDINARY_MAP_LAND_USE_SCOPE` — the Bebelith's 20-foot climb and the crab's
  20-foot swim speed, the Climb and Swim skills, the aquatic subtype and water
  dependency. Nothing is substituted: no speed is folded into another, and no
  skill is raised to stand in for one Kingmaker does not have.
- `SUMMONED_PLANAR_TRAVEL_FORBIDDEN` — plane shift, under the summon spell's
  own restriction. No other mobility or concealment ability replaces it.
- `ITEM_DESTRUCTION_UNMODELED` — Dismantle Armor, as described above.

One limitation the series carried does **not** recur here, and a reader is
owed that as plainly as the ones that do. Sprint 20 recorded
`PER_LIMB_REACH_UNREPRESENTED` because the Giant Scorpion prints Reach 10 ft.
(5 ft. with claws) and Kingmaker gives a creature one reach rather than one
per limb, so its claws threaten at ten feet instead of five. Neither Sprint 21
creature has a per-limb reach line: the Giant Crab prints Space 5 ft., Reach
5 ft. and the Bebelith prints Space 15 ft., Reach 15 ft., both uniform across
every limb. The engine's single-reach model costs these two creatures nothing,
and the project's reduced-reach carrier is correctly not applied to either.
