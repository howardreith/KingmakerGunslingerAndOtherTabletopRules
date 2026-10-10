# 0.0.148-expanded-summoning-sprint19

Kingmaker Gunslinger 0.0.148 is Expanded Summoning Sprint 19: the Girallon
and the Xill. It retains the qualified native Wwise firearm bank,
SHA256 `0E9F88C562F4F937A8941ACE0F241BB31A7ED56B46FBCA549C98F764392EDF18`, and
changes no firearm audio asset. The installation artifact is
`KingmakerGunslinger-0.0.148-expanded-summoning-sprint19.zip`. Existing
`CraftMagicItems.dll` compatibility is inherited; no new crafting integration
is claimed. Historical content and fatigue-authority records retain their 1,288
and 1,325 test counts; neither describes this release's larger current suite.

It is built on the released v0.0.147 master, commit
`95d610348d7d2322ddc00ef04fa06d2cd352dbdd`, and changes nothing that release
qualified. Sprint 18's Ape and Dire Ape stay exactly as v0.0.147 published
them: their roots, their identities, their rules and their four shipped body
files are all inside this sprint's protected boundary.

**Both creatures are runtime qualified and published.** They were registered
with all ten of their roots withheld and stayed withheld through every guarded
runtime review; publication removed the two suppression keys only after the
complete hidden candidate passed. The visible summon surface rises from 1063
choices to 1073 and nothing v0.0.147 published moved.

## What Sprint 19 adds

| | Summon Monster | Summon Nature's Ally | Roots |
| --- | ---: | ---: | ---: |
| Girallon | — | V | 0 + 5 = 5 |
| Xill | V | — | 5 + 0 = 5 |

Neither creature appears on both tables, and neither is templated. The
Girallon is a magical beast with no Summon Monster entry at all, so the
celestial and fiendish templates never reach it; the Xill is already an evil
outsider. Ten roots are therefore ten identities rather than thirty.

Both follow the charter's quantity propagation: single at their own tier, 1d3
one tier higher, 1d4+1 above that. Registered generated placements rise from
1034 to 1044 and all ten are published, so the surface is 1044 generated plus
29 retained native wrappers — 1073 visible choices, ten more than v0.0.147.

Identities were allocated once and never moved. Publication was the removal of
two suppression keys and nothing else: no GUID was reallocated, no symbol was
renamed, and every placement the player saw in v0.0.147 is the same placement
it was.

## Printed profiles

**Girallon**, N Large magical beast, 7d10+35 (73 hp), AC 18/12/15, Fort +9
Ref +8 Will +5, speed 40 ft., bite +10 (1d6+4) and four claws +10 (1d4+4),
Space 10 ft., Reach 10 ft., rend 1d4+6, Str 19 Dex 17 Con 18 Int 2 Wis 12
Cha 7, BAB +7, CMB +12, CMD 25, Improved Initiative, Iron Will, Skill Focus
(Perception), Toughness. All five attacks are primary, at the same bonus and
the whole Strength modifier. The 73 hit points are 38 racial plus 28 from
Constitution plus 7 from Toughness.

**Xill**, LE Medium outsider (evil, extraplanar), 9d10+18 (67 hp), AC 21/14/17,
Fort +8 Ref +10 Will +6, SR 17, speed 40 ft., four claws +13 (1d4+3 plus grab)
and bite +12 (1d3+3 plus paralysis), Space 5 ft., Reach 5 ft., Str 17 Dex 18
Con 14 Int 15 Wis 12 Cha 11, BAB +9, CMB +12 (+16 grapple), CMD 26, Combat
Reflexes, Improved Initiative, Iron Will, Weapon Focus (claw and short sword).
The claws are +13 rather than +12 because of Weapon Focus; the bite has none.
The printed AC includes a +2 shield bonus from carried gear, and that is
carried here rather than quietly dropped, so armour class, touch and
flat-footed read 21, 14 and 17 exactly.

## Rend

The Girallon's rend needs **all four** claws to hit the exact same creature in
one attack sequence. It does not fire from three claws, from claws that hit
different creatures, from a bite plus three claws, across turns or commands, or
from a replayed rule event, and no sequence rends more than once.

The damage is the engine's own: Kingmaker's `RendFeature` deals the feature's
dice plus one and a half times the live Strength modifier as a single damage
event, which is the printed 1d4+6 for an unmodified Girallon and follows a
buffed, enlarged or weakened one exactly.

Only the decision is this project's, and it reuses the bounded gate Sprint 18
wrote for the Dire Ape rather than inventing a second one. That gate is
generalised from two hard-coded claws to a claw count; the two creatures keep
separate features, so the Dire Ape still rends on two and the Girallon on four,
and the released Dire Ape feature keeps its own identity. No general multi-hit
or arbitrary-limb framework is built. As with the Dire Ape, the engine's
command-level rend gate also selects the rend attack animation variant, so that
variant is not played; the rend is still a visible damage event, and the
shortfall is recorded rather than replaced with something invented.

## Honest omissions

- `MULTIWEAPON_ARMED_ROUTINE_UNREPRESENTED`: the Xill's printed armed routine
  is four short swords at +13/+13/+8 with a claw and a bite, plus two longbows
  at +13. Kingmaker's body has one primary hand, one secondary hand and
  additional limbs, and applies its two-weapon rules to those two hands only;
  six wielded weapons cannot be expressed on it. The natural four-claw and
  bite routine is implemented exactly instead, and the armed routine is
  recorded as unrepresented rather than approximated with a substitute that
  would print the wrong numbers. Multiweapon mastery is therefore visible only
  in what it says about the natural routine, which is that every limb adds the
  whole Strength modifier.
- `IMPLANT_UNREPRESENTED`: the Xill's implant lays 2d6 eggs in a helpless
  creature which hatch a day later and drain one Constitution an hour. It has
  no faithful bounded carrier here and no ordinary-map consumer, and it is
  omitted rather than approximated. Nothing stands in for it.
- `ETHEREAL_TRAVEL_UNMODELED`: planewalk shifts the Xill between the Ethereal
  and Material planes, with the attendant miss chances, and may carry one
  creature along. Kingmaker has no ethereal state for a summoned unit, so it is
  omitted. No displacement, blur or invisibility is substituted for it.
- `ORDINARY_MAP_LAND_USE_SCOPE`: Kingmaker exposes one movement speed, so the
  Girallon's 40-foot ground speed is used and its 40-foot climb speed is
  omitted. Its printed Climb +12 is Strength plus the racial climb-speed bonus
  and costs it no skill rank, so unlike the Sprint 18 apes nothing is forfeited
  here: all seven of its ranks are spent where the stat block spends them, four
  on Perception and three on Stealth. Mobility is not raised to stand in for
  Climb and Athletics is not raised to simulate the racial bonus.
- `PASSIVE_CREATURE_SENSES_UNMODELED`: the Girallon's darkvision, low-light
  vision and scent, and the Xill's darkvision, are omitted under the owner's
  accepted limitation. Nothing is substituted for them — no blindsense, no
  vision-range override — and no record claims they work.
- The Xill's paralysis is implemented, with its printed Fortitude DC 16, but
  its printed 1d4-hour duration has no ordinary-map meaning for a creature
  summoned for rounds. The condition is bounded to the summon's own lifetime
  and that bound is recorded here rather than presented as the printed
  duration.

## Runtime qualification

Twelve guarded runtime transactions were spent. Two passed and are this
release's evidence:

- **Mechanics, 39 of 39.**
  `20261010T0649575611737Z-disposable-expanded-summoning-sprint19-review`,
  608 seconds, on a disposable save. Every printed number for both creatures
  read back from the live engine — hit points, armour class, touch,
  flat-footed, saves, spell resistance, CMB, CMD, every skill, every limb's
  attack bonus and damage — plus the nine rend cases in both combat modes and
  the Xill's paralysis.
- **Bodies, 12 of 12.**
  `20261010T0744422373426Z-working-save-expanded-summoning-creature-review`,
  on the authorized working save. All eight party-camera captures came back
  `renderer=true dissolve=0 intact=true`.

Ten runs before those found real defects, all of them this project's own, and
each is recorded rather than smoothed over:

1. The scenario name was refused by a fourth allowlist inside the mod itself,
   which only answers after a deploy and a launch. An offline test now proves
   the name is wired at every gate, so that failure cannot cost another run.
2. Mod initialization threw on an unknown natural weapon key and rolled back
   2,376 registrations: the Girallon's two weapons were never added to the
   builder's key resolver. An offline test now resolves every creature's every
   profile weapon key.
3. A harness mistake of mine — the run's timeouts were left at their defaults
   rather than the values a review of this size needs — and the run stalled.
4. 29 of 39. Four invented placeholder body GUIDs meant neither body was ever
   attempted; the Girallon kept the donor's unit type; its hit points and two
   skills were wrong because the exact-rank component was gated to primates
   only; and the Xill was missing its shield bonus, its spell resistance and
   any paralysis trigger at all.
5. 31 of 39. The Xill's configuration overwrote its donor prefab with a human
   one, copied from the method it was modelled on, and the paralysis cases
   measured the condition left over from the case before.
6, 7. Two engine `NullReferenceException`s. A round-waiter I had added idled
   real-time combat for a full six-second round, through which the engine ends
   the fight and despawns the target's view. The waiter is gone; the one case
   that genuinely needs a round boundary now runs turn-based only, and its
   real-time row records why instead of going quiet.
8. 38 of 39. Two Xill skills were each exactly three short: Persuasion and
   Knowledge (arcana) are outsider class skills and the donor's class list
   does not carry them.
9. 38 of 39. Retry accumulation. The engine's rend tracker is scoped to the
   attack command, not to the review's idea of an attempt, so claws from
   abandoned attempts still counted and four distinct claws eventually reached
   one target although no single attempt landed four. A rend case is now one
   atomic trial, abandoned whole and retried from nothing.
10. 38 of 39. `UnitAttack.TryMergeInto` folds a second attack on one target
    into a live command, so the review's "second sequence" could be the same
    command as its first. The live command is now interrupted and waited out
    before each later sequence, which forces no roll and no damage and only
    makes a second command be a second command.

The art review also failed once at 11 of 12, with the Girallon captured
mid-dissolve: the crowd review's native-fade wait was gated to three earlier
creature families and did not cover a donor-rig body. The gate was widened and
the re-run passed 12 of 12.

The owner installation was restored to its exact recorded state after every
transaction, and no process, lease or lock was left outstanding.

## Visuals

Both creatures ship two new original project-owned 128x128 icons, rendered
procedurally with the project's own Blender creature-icon tool.

Both creatures also have original project-owned bodies: one generated mesh and
one painted albedo each, built against a bind-pose skeleton and nothing else.
No proprietary mesh, texture or animation is redistributed.

The donor rig is the one the Sprint 18 guarded census chose — the Troll guard,
the only surveyed Large rig actually built like a primate, with an upright
spine, long two-segment arms ending in real hands, a separate jaw and toed
feet. No new census was run, because both Sprint 19 creatures are bipeds of the
same broad build and re-deriving the same answer would spend an owner runtime
transaction to learn nothing. The reuse is verified rather than assumed: the
capture's bone graph, driver set and bind height are re-checked before any
geometry is authored.

**`FOUR_ARMS_SHARE_TWO_DRIVER_CHAINS`.** Both creatures print four arms and the
donor rig has two. No four-arm rig is built and no bone is invented. Each
side's two arms are skinned to that side's single existing driver chain, with
the lower arm offset down and out from the upper, so both right arms swing
together and both left arms likewise. For a creature whose printed routine is
four claws striking one target in one sequence this reads correctly, but the
lower arms cannot be posed independently of the upper arms, and that is stated
here rather than left for a player to notice. A faithful four-armed rig would
be a general arbitrary-limb and animation system, which this sprint does not
build.

`HumanReview: NOT_PERFORMED_NONBLOCKING.`

## Uninstall

To uninstall, remove the mod directory, exactly as before. A save made with
the Girallon or the Xill summoned loads without them, exactly as a save made
with any other summon from this mod does.
