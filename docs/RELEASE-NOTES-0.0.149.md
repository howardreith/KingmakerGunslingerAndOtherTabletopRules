# 0.0.149-expanded-summoning-sprint20

Kingmaker Gunslinger 0.0.149 is the Expanded Summoning Sprint 20 candidate: the
Giant Scorpion. It retains the qualified native Wwise firearm bank, SHA256
`0E9F88C562F4F937A8941ACE0F241BB31A7ED56B46FBCA549C98F764392EDF18`, and changes
no firearm audio asset. The installation artifact is
`KingmakerGunslinger-0.0.149-expanded-summoning-sprint20.zip`. Existing
`CraftMagicItems.dll` compatibility is inherited; no new crafting integration is
claimed. Historical content and fatigue-authority records retain their 1,288 and
1,325 test counts; neither describes this candidate's larger current suite.

It is built on the released v0.0.148 master, commit
`37eb4ee5656ec1f59def1ad2dc2b0892edd22c83`, and changes nothing that release
qualified. Sprint 18's Ape and Dire Ape and Sprint 19's Girallon and Xill stay
exactly as v0.0.148 published them: their roots, their identities, their rules
and their eight shipped body files are all inside this sprint's protected
boundary.

**This release is runtime qualified and the Giant Scorpion is published.** Its
twelve roots are selectable, and the visible summon surface rises from the 1073
choices v0.0.148 published to 1085. Every number below was read back off a live
creature in a running game, in both real time and turn-based combat, on
`20261010T1615114043487Z-disposable-expanded-summoning-sprint20-review` and
re-run against the published surface on
`20261010T1648433410286Z-disposable-expanded-summoning-sprint20-review`; the
body was reviewed under the party camera on
`20261010T1535502794822Z-working-save-expanded-summoning-creature-review`.
`HumanReview: NOT_PERFORMED_NONBLOCKING.`

## What Sprint 20 adds

| | Summon Monster | Summon Nature's Ally | Roots |
| --- | ---: | ---: | ---: |
| Giant Scorpion | IV | IV | 6 + 6 = 12 |

One creature, twelve roots, on both tables at tier 4. Unlike either Sprint 19
creature it **is** templated, which is what every other vermin on the Summon
Monster table is, so each of its six Monster roots owns a celestial and a
fiendish execution child. Twelve roots therefore cost 34 appended identities
where Sprint 19's ten cost 22: one unit, twelve placements, twelve execution
children, and nine of its own blueprints. Summon Nature's Ally never templates,
so its six roots own no children.

Both tiers follow the charter's quantity propagation: single at tier 4, 1d3 at
tier 5, 1d4+1 from tier 6 through 9. Registered generated placements rise from
1044 to 1056, and with publication all 1056 are visible: 1056 generated plus 29
retained native wrappers — 1085 visible choices, up from 1073.

Identities were allocated once at registration and never moved. Publication was
the removal of one suppression key and nothing else: no GUID moved, nothing was
renamed, nothing was reallocated, and nothing published earlier was withheld to
make room.

Three of those nine blueprints exist because the first guarded review found
three printed lines with a derivation and no carrier. The registration commit
described all three as working. The arithmetic closed in every case, which is
exactly why an offline test could not see it.

## Printed profile

**Giant Scorpion**, N Large vermin, 5d8+15 (37 hp), AC 16/10/15, Fort +7 Ref +2
Will +1, immune to mind-affecting effects, speed 50 ft., two claws +6 (1d6+4
plus grab) and a sting +6 (1d6+4 plus poison), Space 10 ft., Reach 10 ft. (5 ft.
with claws), Str 19 Dex 12 Con 16 Int — Wis 10 Cha 2, BAB +3, CMB +8 (+12
grapple), CMD 19 (31 vs. trip), poison (sting, injury, Fort DC 15, 1d2 Strength
for 6 rounds, cure 1 save).

All three attacks are primary, at the same bonus, and each adds the **whole**
Strength modifier rather than one and a half times it — 1d6+4, not 1d6+6. The
37 hit points are 22 racial plus 15 from Constitution, with no third term,
because vermin have no feats and so no Toughness.

This creature is unusually well behaved arithmetically: it has no Intelligence
score, so it has no skill ranks and no feats, and every number in the block
falls out of hit dice, ability scores, size and racial bonuses alone. Eleven
independent identities close on those numbers, which pins Dexterity at 12 three
separate ways — through armour class 16, through touch 10 and through Reflex +2.

## Mechanics

**Grab** is on the two claws and never on the sting. It reuses the grapple
lifecycle Sprint 6 built and Sprint 19 taught to hold several limbs, and it
takes the multi-target hold rather than the single one because two claws hold
two foes. The spec counts limbs rather than naming weapons: the primary limb
plus one additional limb is both claws and stops short of the sting, which is
the second additional limb. Both Giant Ants read the other way round — a
primary bite that grabs and an additional sting that never does — so the gate
is the same one counting differently.

It was missing at registration. Three places said the creature had it: this
section, the printed +12 grapple derivation, and the registration commit's own
message. None of them is the thing that grabs. The live creature attempts a
grapple on a claw hit now and the printed figure reads +12 against its
printed +8 manoeuvre bonus, measured with nothing held.

**Poison** is on the sting and never on a claw, gated on the sting's own weapon
type exactly as the Giant Ant's sting poison is. It needs its own carrier rather
than sharing one: the printed graph runs six rounds and every poison this
project ships runs four. The difficulty class is never written down — a context
action sets it live from the scorpion's own Constitution immediately before the
game's own save, so a buffed or weakened scorpion poisons for what it supports.
At the printed Constitution 16 and five hit dice that is the printed DC 15.

**The anti-trip defence** is this creature's own carrier, and that is a
correction rather than a preference. It was given the shared native
`TripDefenseEightLegs` fact — the one the Giant Centipede and both Giant Ant
castes hold — on the strength of its name, and the first guarded review
measured the live creature at CMD 27 against trip where its stat block prints
31. The shared fact delivers +8, which is correct for the creatures holding it:
the stat-block convention is four per pair of legs beyond the first, so a
six-legged insect prints +8 and this eight-legged arachnid prints +12. The
fact's name says eight legs and its value says six.

Nothing about the shared fact changes — it is native, and the creatures using it
are released — so the Giant Scorpion carries its own +12 on the engine's own
manoeuvre-defence component, gated on trip alone, which is why its ordinary
manoeuvre defence stays at the printed 19. The rules policy records the shared
carrier's measured value beside the printed one and refuses the case where they
are equal, because then the shared one would do.

**The printed racial +4** on Perception and Stealth is its own carrier too, and
it was missing entirely. The live creature read Perception 0 against a printed
+4 and Stealth −3 against a printed +1 — the same absent bonus twice. The
derivation asserted ability plus racial and the racial half existed only in the
derivation. The Giant Ants' carrier is theirs: it covers their printed
Perception alone and records their Survival as omitted. Stealth is now +1 on a
live creature, from Dexterity +1 plus racial +4 less 4 for Large size, which
also answers the one number the frozen contract could not close at intake —
`STEALTH_DERIVES_TO_PLUS_ONE` recorded the derivation against a recalled +0,
and the engine agrees with the derivation.

**The printed immunity to mind-affecting effects** rides the game's own
descriptor immunity, the same component the Goblin Dog uses for disease. It is
carried explicitly rather than inferred from an Intelligence score, and that
distinction matters: the printed Intelligence is an em dash, Kingmaker cannot
hold an absent score, so this creature ships at Intelligence 1 like every vermin
before it — and 1 is not mindless as far as the engine is concerned. The vermin
that shipped earlier do not carry this fact; they are released and inside this
sprint's protected boundary, so the gap is recorded rather than quietly fixed
across creatures nobody re-qualified.

Both weapons are project-owned at the printed dice with overridden damage dice,
because the engine scales a shared native 1d6 one step up for a Large wielder —
the exact defect the guarded Sprint 18 review measured on the Dire Ape — and no
native blueprint carries a scorpion sting at all.

## Honest omissions

- `PASSIVE_CREATURE_SENSES_UNMODELED`: darkvision 60 feet and tremorsense 30
  feet are omitted under the owner's accepted limitation. Nothing is
  substituted — no blindsense, no blindsight, no vision-range override — and no
  record claims they work. The Sprint 6 Giant Spider stands native blindsight in
  for its tremorsense; that shipped decision is left exactly as it is and
  deliberately not extended here, because substituting one sense for another is
  what the sprints after it stopped doing.
- `ORDINARY_MAP_LAND_USE_SCOPE`: Kingmaker has no Climb skill, so the printed
  Climb +8 is omitted and neither Athletics nor Mobility is raised to stand in
  for it. Nothing is forfeited: this creature has no Intelligence score and so
  no ranks, and the +8 is Strength plus its racial bonus.
- `PER_LIMB_REACH_UNREPRESENTED`: the printed line is Space 10 ft., Reach 10 ft.
  (5 ft. with claws), and Kingmaker gives a creature one reach rather than one
  per limb. The creature keeps the printed 10, which is also the engine's own
  reach for a Large creature, so the claws threaten at 10 feet instead of 5.
  That is a consequence of the engine's single-reach model and is not presented
  as faithful. The project's reduced-reach carrier is deliberately not applied:
  the Giant Stag Beetle uses it because that creature's whole printed Reach line
  is 5 feet, where this one's is 10.
- Vermin have no feats, so no feat is dropped and none is invented. Recorded so
  the absence is not mistaken for an omission.

## Visuals

**Original and project-owned.** One procedurally generated mesh, one procedural
painting and one icon, all authored in this sprint. No native vertex, triangle,
texture, material or animation is copied, and nothing proprietary is
redistributed: the shipped files carry original geometry, UVs, weights, the
reviewed bone names and the painting's hash. Blender sources, FBX previews,
reports, the census capture and the review renders stay machine-local.

The donor rig is the Giant Spider's, measured rather than recalled. Sprint 19
could skip a census because Sprint 18's capture survived and could be re-decoded
offline; the Sprint 14 arachnid capture did not survive, and the shipped insect
meshes give bone names and a rig fingerprint but not the bone roster, the bind
poses, or how many segments the abdomen chain has — and a scorpion's tail is
authored onto that chain. So the census ran:
`20261010T1328444995657Z-observe-expanded-summoning-arachnid-census`, bounded
and read-only, with three independent Giant Spider prefabs agreeing. It
confirmed the Sprint 14 conclusion that Kingmaker has no scorpion, and it cost
four guarded transactions, three of them spent on defects in the census itself
rather than in the game.

Three measurements make this the best donor fit the project has found:

- **Eight legs for eight legs.** The fourth chain is a complete leg here —
  upper, lower and foot on both sides — where the Sprint 14 ants leave it empty
  and the beetles hang wings on it. This is the first creature in the project to
  weight all four chains a side as real legs, and the party-camera review read
  all four back off the live creature.
- **A scorpion's claws ARE its pedipalps**, and the rig has a seven-bone
  pedipalp chain a side that already curls forward and down. The chelae are
  anatomy here, not substitution. They extend past the chain's end along its own
  direction, because the chain spans about a fifth of a leg's length and a
  scorpion's chelae are its largest limbs; following the bones exactly rendered
  two nubs beside the mouth.
- **The abdomen chain runs backward from the body**, which is where a metasoma
  attaches.

`METASOMA_DRIVEN_BY_A_TWO_BONE_CHAIN` is the measured limitation, and the number
that would have been guessed wrong. A scorpion's metasoma is five segments and a
telson; this rig's abdomen chain is `Tail1_M` and `Tail3_M` with `UpperTorso`
between them — three driver bones, all level, running backward. Every other tail
this project has authored runs `Tail0_M` through `Tail4_M`, so five was the
natural assumption and it is wrong here. The tail is therefore authored arching
up and forward over a level chain and weighted along it, so the whole metasoma
sways as the abdomen sways rather than articulating segment by segment, and the
sting cannot be driven as a strike of its own. No bone is invented and no rig is
built.

The palette is deliberately not the Sprint 14 insects'. Those share this rig and
a player chooses between them in one menu: the ants are red-brown and the
beetles near-black, so the scorpion is pale warm sand. The one saturated thing
on the creature is the bead of venom at the sting's tip.

`HumanReview: NOT_PERFORMED_NONBLOCKING.`

## Runtime qualification

Three guarded mechanics reviews and one party-camera art review, on one
candidate, with the owner installation restored exactly to
`216A9DC2B8E95CD644BA3CADC69A638463C25E60F40A11F8D4B2065C69D5AAF3` after every
run. The art review passed first time. The mechanics review did not, and what it
found is recorded above rather than summarised away: 17 of 20 on the first run,
18 of 20 on the second, 20 of 20 on the third, and 20 of 20 again against the
published surface.

Two of the three failures were the creature and are fixed. The third was the
review, twice over, and both halves were the dice rather than the rule. The
profile read the printed grapple figure as 17 because a claw case before it had
legitimately seized the hostile and the holder's side of a hold is not cleared
by resetting the victim — the maintain bonus was sitting on top of the grab, and
the sum of two right numbers is a wrong answer. And a natural twenty made a save
the fixture had chosen to fail, then landed limbs the fixture had not chosen,
spoiling the isolation a rider case depends on. Each of those is now an
abandoned trial retried under the same bounded re-attempt that already handled a
natural one, and every save's natural roll is recorded so an abandoned trial is
visible rather than inferred.

What the passing review measures, in real time and in turn-based combat:

- Every printed number off the live creature — ability scores, size, hit dice,
  hit points, all three armour classes net of the native difficulty term, all
  three saves, manoeuvre bonus and defence including the trip case, the grapple
  figure, both mapped skill totals, and each limb's own attack bonus and damage.
- Grab on both claws and on neither the sting nor anything else, worth exactly
  the printed difference between +8 and +12, measured with nothing held.
- Poison on the sting at the live Constitution-scaled DC 15, proved on both
  sides of the save, and absent from a claw: a claw that lands forces no
  Fortitude save at all and a sting that lands attempts no grapple at all.
- The printed three-limb routine, each limb exactly once, none secondary, all at
  one bonus. It is measured with the grab carrier off this one request-local
  creature, because a creature that has just seized its target stops swinging —
  Sprint 14 proved that on the Giant Ant Soldier — and the routine could
  otherwise never be measured whole. The grab itself is proved in the rider
  cases with the carrier live.
- The mind-affecting immunity read off the component the rules layer consults,
  not from the fact being present.
- The original body attached, all eight leg chains weighted, the authored
  metasoma on the abdomen chain, and no bone outside the reviewed list.
- Twelve roots live and published, registered equal to published at 1056, and
  nothing outside this creature withheld.

## Uninstall

To uninstall, remove the mod directory, exactly as before. Withheld creatures
register no player-visible choice, so a save made with this build loads without
the Giant Scorpion exactly as it loads without any other unreleased content.
