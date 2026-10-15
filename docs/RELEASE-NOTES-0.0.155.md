# v0.0.155 — Expanded Summoning Sprint 24: theropod dinosaurs

`0.0.155-expanded-summoning-sprint24-theropods`

This release adds two Pathfinder First Edition theropods to the Expanded
Summoning roster: the Deinonychus and the Tyrannosaurus. Both are adult stat
blocks with original project-authored bodies and icons; no native Owlcat mesh,
texture or animation is redistributed.

| Creature | Family and tier | Roots |
| --- | --- | ---: |
| Deinonychus | Summon Monster IV, Summon Nature's Ally IV | 12 |
| Tyrannosaurus | Summon Monster VII, Summon Nature's Ally VII | 6 |

Quantities follow the usual progression: one at the base tier, 1d3 at the next,
then 1d4+1. Monster routes use the existing celestial/fiendish template policy;
Nature's Ally routes stay untemplated under caster alignment.

## Deinonychus

Four attacks, and exactly four: two primary talons, a primary bite and one
secondary foreclaws roll — the plural label is a single roll, not two. At
Strength 15 and BAB +3 that is +5/+5/+5/+0 with +2/+2/+2/+1 damage. Pounce is
the exact native carrier and exposes that ordinary routine only after a legal
charge; an illegal or aborted charge, an attack of opportunity and an ordinary
single attack do not become free routines. Skills are the printed adult values
derived from real carriers — Perception +14, Stealth +15, Mobility +10 — and CMD
is 17 with no borrowed quadruped trip defence. It has no Rake, Rend, Grab,
Swallow Whole, poison, fear or Powerful Charge.

## Tyrannosaurus

One 4d6 bite with a 19–20 threat range. Powerful Bite makes the final signed
Strength contribution exactly twice the live modifier, so Strength 32 gives +22;
it never doubles dice, enhancement, added damage or any critical rider, and zero
or a penalty keeps correct behaviour. Critical Focus and Improved Critical
(bite) are the exact native facts, and a confirmed bite critical applies the
native 2d6 bleed — Kingmaker has no Bleeding Critical feat, so the project owns
only the bite-and-confirmation predicate, not a second bleed implementation.

Grab and Swallow Whole are separate gates. A bite hit may grab a target of its
own size or smaller at +4 on the grapple check. Swallowing needs a target at
least one size smaller that it already held in its mouth at the start of a later
round, plus a new successful check; there is no immediate or Fast Swallow path.
Interior damage is its own once-per-round 2d8+11 packet, distinct from the bite
and its riders. It has no claws, tail attack, Pounce, trample, roar or Power
Attack, and never confirms a critical automatically.

## Save compatibility and reload behaviour

Both creatures are appended with stable identities; existing saves, keys and
route ordering are unchanged. The published roster becomes 107 creatures and
1,093 generated placements — Summon Monster 95 creatures and 543 placements,
Summon Nature's Ally 95 and 550 — plus the 29 retained native wrappers, for
1,122 visible choices.

## Honest limits

- `ACTIVE_SUMMON_GRAPPLES_RESET_SAFELY_ON_RELOAD`: an active grab, hold, swallow
  or engulf link and the mouth occupancy that goes with it are session-scoped
  for every summon in this mod, the Tyrannosaurus included. A save and reload
  reset them cleanly — no lingering condition, occupied mouth, delayed interior
  damage or dangling link, and both creatures stay usable. This is a clean reset,
  not grapple persistence, and nothing is re-established on load. The native
  bleed is a separate effect and follows its own save and healing rules.
- `SWALLOW_WHOLE_INTERIOR_AC_HP_UNMODELED`: the printed interior AC 17 and 15 hp
  are recorded in the fidelity record without claiming an attackable stomach.
  Size, the initial bite, periodic damage, native escape, exact ownership and
  cleanup are all implemented.
- `PASSIVE_CREATURE_SENSES_UNMODELED`: both theropods print low-light vision and
  scent, which have no faithful Kingmaker carrier. Nothing is substituted for
  them and no record claims they work.
- `RUN_NATIVE_SUMMON_AI_DISPOSITION`: no invented Run action is added, the same
  boundary the Triceratops carried.

No new waiver is introduced. The owner's visual review of the finished art is
still to come and was not a release blocker.
