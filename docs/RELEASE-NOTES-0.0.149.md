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

**This candidate is NOT runtime qualified.** The Giant Scorpion is registered
and withheld: no player can select it, and the visible summon surface is
exactly what v0.0.148 published. The sections below describe what the source
contains, not what has been proved in game.

## What Sprint 20 adds

| | Summon Monster | Summon Nature's Ally | Roots |
| --- | ---: | ---: | ---: |
| Giant Scorpion | IV | IV | 6 + 6 = 12 |

One creature, twelve roots, on both tables at tier 4. Unlike either Sprint 19
creature it **is** templated, which is what every other vermin on the Summon
Monster table is, so each of its six Monster roots owns a celestial and a
fiendish execution child. Twelve roots therefore cost 31 appended identities
where Sprint 19's ten cost 22: one unit, twelve placements, twelve execution
children, and six of its own blueprints. Summon Nature's Ally never templates,
so its six roots own no children.

Both tiers follow the charter's quantity propagation: single at tier 4, 1d3 at
tier 5, 1d4+1 from tier 6 through 9. Registered generated placements rise from
1044 to 1056, all 12 new roots are suppressed, and the published surface stays
at 1044 generated plus 29 retained native wrappers — 1073 visible choices,
unchanged.

Identities are allocated once, now, and never move: publication will be the
removal of one suppression key and nothing else.

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
lifecycle Sprint 6 built and Sprint 19 taught to hold several limbs, gated on
the claw weapon type, rather than a second mechanism.

**Poison** is on the sting and never on a claw, gated on the sting's own weapon
type exactly as the Giant Ant's sting poison is. It needs its own carrier rather
than sharing one: the printed graph runs six rounds and every poison this
project ships runs four. The difficulty class is never written down — a context
action sets it live from the scorpion's own Constitution immediately before the
game's own save, so a buffed or weakened scorpion poisons for what it supports.
At the printed Constitution 16 and five hit dice that is the printed DC 15.

**The eight-legged trip defence** is the project's existing carrier, the one the
Giant Centipede and both Giant Ant castes already hold. A scorpion has eight
legs and the printed difference between CMD 19 and CMD 31 against trip is
exactly that bonus, so nothing new is built for it.

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

**Not authored.** The original project-owned Giant Scorpion body and its icon
are NOT in this candidate. The creature currently rides the native Giant Spider
renderer, which is why it is withheld.

The donor is already chosen and the reasoning is anatomical rather than
approximate. The Sprint 14 census established that Kingmaker has no scorpion and
that the Giant Spider is the only compact many-legged arthropod in the game,
with four leg chains a side, seven-bone pedipalps, chelicerae and an abdomen
chain. A scorpion has eight legs and the donor has eight — the fourth pair,
which the ants leave empty and the beetles use for wings, is a real leg here, so
this will be the first creature in the project to weight all four chains a side
as legs. A scorpion's claws **are** its pedipalps, so the chelae go on the
seven-bone pedipalp chains the donor already has. The metasoma and telson go on
the abdomen chain, which sits behind and above the body where a scorpion's tail
attaches.

Unlike Sprint 19, this sprint must spend one bounded read-only donor census.
Sprint 19 skipped one because Sprint 18's capture survived and could be
re-decoded offline; the Sprint 14 arachnid capture did not survive — its
provenance records that the request-local capture stays outside Git and the
package. The shipped insect meshes give the bone names those creatures used and
the rig fingerprint, but not the full bone roster, the bind-pose transforms, or
how many segments the abdomen chain has, and a scorpion's tail is authored onto
that chain. Guessing it is not acceptable.

`HumanReview: NOT_PERFORMED_NONBLOCKING.`

## Uninstall

To uninstall, remove the mod directory, exactly as before. Withheld creatures
register no player-visible choice, so a save made with this build loads without
the Giant Scorpion exactly as it loads without any other unreleased content.
