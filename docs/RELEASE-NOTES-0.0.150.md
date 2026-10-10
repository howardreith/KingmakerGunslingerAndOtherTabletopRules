# 0.0.150-expanded-summoning-sprint21

Kingmaker Gunslinger 0.0.150 is the Expanded Summoning Sprint 21 candidate, and
the last sprint in the series: the Giant Crab, and the Bebelith's own body. It
retains the qualified native Wwise firearm bank, SHA256
`0E9F88C562F4F937A8941ACE0F241BB31A7ED56B46FBCA549C98F764392EDF18`, and changes
no firearm audio asset. The installation artifact is
`KingmakerGunslinger-0.0.150-expanded-summoning-sprint21.zip`. Existing
`CraftMagicItems.dll` compatibility is inherited; no new crafting integration is
claimed. Historical content and fatigue-authority records retain their 1,288 and
1,325 test counts; neither describes this candidate's larger current suite.

It is built on the released v0.0.149 master and changes nothing that release
qualified. The Sprint 20 Giant Scorpion, its nine own blueprints and its two
shipped body files are inside this sprint's protected boundary, as is every
creature before it.

**This candidate is NOT runtime qualified.** The Giant Crab is registered and
withheld: no player can select it, and the visible summon surface is exactly
what v0.0.149 published. The sections below describe what the source contains,
not what has been proved in game.

## Two creatures at different stages

This sprint has a shape no earlier one had. The **Giant Crab** is new in every
respect: seven Summon Nature's Ally roots, its own rules, its own icon and its
own original body. The **Bebelith** is already registered, published and
runtime qualified - it shipped with a complete live qualification in an earlier
phase - and this sprint owns exactly one thing for it: the original
project-owned body it has never had, because it currently wears the native
Doomspider's view. Its rules, chassis, identities, placements and recorded
deviations are released and are not touched, which this sprint's gate checks by
name rather than by trust.

The shipped Bebelith's chassis also differs from the printed Bestiary Bebilith
on purpose - Huge rather than Large, with its own claw dice - and this sprint
does not reconcile the two. The released creature is the qualified one, its
deviations were accepted when it shipped, and re-opening them would move a
creature that has already published. Recorded here so a reader who compares the
shipped creature with the book finds the difference explained rather than
apparently overlooked.

## What Sprint 21 adds

| | Summon Monster | Summon Nature's Ally | Roots |
| --- | ---: | ---: | ---: |
| Giant Crab | — | III | 0 + 7 = 7 |

One creature, seven roots, on one table. It is the first creature in the roster
to sit at Summon Nature's Ally III with no Summon Monster tier, which is why it
owes seven placements rather than the twelve a two-table creature owes - parents
3 through 9, single at its own tier, 1d3 at the next and 1d4+1 thereafter - and
why it owns no execution children at all: Summon Nature's Ally never templates,
and this creature is not on the other table.

Thirteen appended identities: one unit, seven placements, and five of its own
blueprints. Registered generated placements rise from 1056 to 1063, all seven
new roots are suppressed, and the published surface stays at 1056 generated plus
29 retained native wrappers — 1085 visible choices, unchanged.

Identities are allocated once, now, and never move: publication will be the
removal of one suppression key and nothing else.

## Printed profile

**Giant Crab**, N Medium vermin (aquatic), 3d8+6 (19 hp), AC 15/11/14, Fort +5
Ref +2 Will +1, immune to mind-affecting effects, speed 30 ft., swim 20 ft., two
claws +4 (1d4+2 plus grab), Space 5 ft., Reach 5 ft., Str 15 Dex 12 Con 14 Int —
Wis 10 Cha 2, BAB +2, CMB +4 (+8 grapple), CMD 15 (27 vs. trip), Perception +4,
Swim +10, racial +4 Perception and +8 Swim, amphibious.

Both claws are primary, at the same bonus, and each adds the **whole** Strength
modifier rather than one and a half times it — 1d4+2, not 1d4+3. The 19 hit
points are 13 racial plus 6 from Constitution, with no third term, because
vermin have no feats and so no Toughness.

Sixteen independent identities close on this transcription and every one of them
closed at intake. Unlike Sprint 20's, nothing in this block is recorded as a
question against the book.

## Mechanics

**Grab** is on both claws, which is every limb this creature has. It reuses the
grapple lifecycle Sprint 6 built and Sprint 19 taught to hold several limbs, and
takes the multi-target hold rather than the single one because two claws hold
two foes. The spec counts limbs rather than naming weapons, and the count is
still written as the printed claw count rather than "all of them", so a creature
that grew a third limb would fail rather than absorb it.

**The anti-trip defence** is its own +12 carrier. A crab walks on eight legs and
the convention is four per pair of legs beyond the first, so its printed CMD 27
is twelve above its 15. The shared native eight-leg fact delivers +8 — Sprint 20
measured that on a live creature — and the Sprint 20 carrier that does deliver
+12 is named and described for a scorpion, so this creature owns one rather than
wearing the wrong creature's feature.

**The printed racial +4 Perception** is its own carrier for the same reason.
Sprint 20 shipped this exact kind of line with a derivation and no carrier, and
the live creature read Perception 0; this one is wired, and the review reads the
bonus out of the live skill's modifier list rather than out of the builder.

**The printed immunity to mind-affecting effects** rides the game's own
descriptor immunity, carried explicitly rather than inferred from an
Intelligence score: the printed Intelligence is an em dash, Kingmaker cannot
hold an absent score, so this creature ships at 1 like every vermin before it,
and 1 is not mindless as far as the engine is concerned.

**It owns no weapon.** This is the first creature in the last four sprints that
does not, and the reason is size: it is Medium, so the engine does not scale the
shared native 1d4 claw for it. That scaling is exactly why the Large Sprint 18
and Sprint 20 creatures had to own theirs, and the shared claw already carries
the printed dice.

## Honest omissions

- `ORDINARY_MAP_LAND_USE_SCOPE`: the printed swim 20 ft., the aquatic subtype,
  the amphibious quality and the racial +8 Swim. Kingmaker has no Swim skill and
  models neither swimming nor drowning, and this mission's hard boundaries
  forbid building an aquatic subsystem. Nothing is substituted: the swim speed
  is not folded into the 30-foot ground speed, no skill is raised to stand in
  for Swim, and no breathing or drowning state is invented. The creature
  forfeits no rank — it has no Intelligence score and so none to spend, and the
  +8 is entirely racial. A player who summons a Giant Crab gets a land creature,
  which is smaller than the book's in exactly one dimension, and the dimension
  is named here rather than left to be discovered.
- `PASSIVE_CREATURE_SENSES_UNMODELED`: darkvision 60 feet, omitted under the
  owner's accepted limitation with nothing substituted.
- `PER_LIMB_REACH_UNREPRESENTED`: recorded as inherited rather than newly
  incurred. This creature's printed Space and Reach are both 5 feet, which is
  the engine's own for a Medium creature, so it has nothing to omit — the label
  stays in the series record because the Sprint 20 creature does.
- Vermin have no feats, so no feat is dropped and none is invented. Recorded so
  the absence is not mistaken for an omission.

## Visuals

**Not yet authored.** The original project-owned Giant Crab body and icon, and
the Bebelith's original body, are NOT in this candidate. The crab currently
rides the native Giant Spider renderer and the Bebelith still wears the
Doomspider, which is why the crab is withheld.

The crab's donor is chosen for the reasons the Sprint 20 scorpion's was, and
they are anatomical rather than approximate: a crab's pincers **are** its
pedipalps, and the rig has a seven-bone pedipalp chain a side; and a crab walks
on eight legs, which is the four chains a side Sprint 20 proved are all real
legs on that rig. It carries no tail, so the abdomen chain holds the back of the
carapace rather than a metasoma — the one structural difference from the
scorpion's body on the same rig.

The Bebelith's body must be weighted to the rig its own view uses, which is the
Doomspider's rather than the Giant Spider's. Whether those are the same skeleton
is a measurement and not an assumption, and the Sprint 20 capture does not
survive — it is request-local and stays outside Git and the package — so this
sprint needs its own census regardless. One bounded read-only run measures both
anchors.

`HumanReview: NOT_PERFORMED_NONBLOCKING.`

## Uninstall

To uninstall, remove the mod directory, exactly as before. Withheld creatures
register no player-visible choice, so a save made with this build loads without
the Giant Crab exactly as it loads without any other unreleased content.
