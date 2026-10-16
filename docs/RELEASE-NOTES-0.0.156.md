# v0.0.156 — Expanded Summoning Sprint 25: giant bodies

`0.0.156-expanded-summoning-sprint25-giant-bodies`

This release adds the Brachiosaurus to the Expanded Summoning roster and
completes the Elephant that was already in it. Both are adult Pathfinder First
Edition stat blocks with original project-authored bodies; no native Owlcat
mesh, texture or animation is redistributed.

| Creature | Family and tier | Roots |
| --- | --- | ---: |
| Brachiosaurus | Summon Nature's Ally VII | 3 |
| Elephant | Summon Monster VI, Summon Nature's Ally VI | 8 existing, completed |

Quantities follow the usual progression: one at the base tier, 1d3 at the next,
then 1d4+1. Brachiosaurus is untemplated under caster alignment and is not on
the Summon Monster table, so it has no celestial or fiendish execution
children. Elephant's eight existing roots, their children and their order are
unchanged — this is a completion of a released creature, not a re-registration,
and no actual Mastodon unit or asset is touched.

## Brachiosaurus

One natural weapon and exactly one: a tail at +23 for 4d6+19 at the adult stat
block. Because it is the creature's only natural attack it takes the native
one-and-a-half Strength multiplier, so Strength 37 contributes +19, and zero or
a penalty keeps correct behaviour rather than being multiplied.

The printed tail bonus differs between sources — the current Archives of
Nethys entry reads +22, the legacy PRD reads +23 — and this release uses +23.
That is the value the inputs both sources print actually derive: BAB +13,
Strength 37 for +13, Gargantuan −4 and Weapon Focus (tail) +1. The same inputs
independently derive the CMB +30 and CMD 40 that both sources also print, so
the ability score and base attack bonus are not in doubt, and +22 would require
an unexplained −1 that no printed input supplies. Nothing invents a hidden
modifier and Weapon Focus is not removed; both readings are recorded in the
sprint ledger.

Trample is 2d6+19 against a DC derived live as 10 + half its hit dice + its
Strength modifier, which is the printed 32, and it is a separate effect from
the ordinary tail attack. Twenty-foot space and twenty-foot reach, 30-foot
land speed, AC 18, 171 hit points, Fort +18 / Ref +11 / Will +9, CMD 40 with
the quadruped +4 against trip, and Perception +28 from real carriers rather
than an adjusted total.

It has no bite, no stampede, no tail-sweep area attack and no frightening
roar. It does not borrow the Ankylosaurus daze or stun, the Stegosaurus trip,
or the Tyrannosaurus bite multiplier, Grab or Swallow Whole.

## Elephant

Two primary natural attacks: a gore at +16 for 2d8+10 and a slam at +16 for
2d6+10. Both are primary, so each adds the live Strength modifier once rather
than one and a half — the lone-weapon routine the Brachiosaurus uses does not
apply here. There is no stamp routine and no grab; the adult Pathfinder
Elephant has a gore and a slam and nothing else.

Space is 15 feet but reach is 10 feet, exactly as printed. A Huge body is not
evidence of 15-foot reach and this creature is not used to grant one
generally. Trample is 2d8+15 at DC 25, reusing the existing d8 trample support
without granting powerful charge and without changing released d6 tramplers.
40-foot land speed, AC 17, 93 hit points, Fort +13 / Ref +7 / Will +6, CMD 30
with the quadruped +4 against trip, and Perception +21.

Existing template and caster-alignment behaviour is retained.

## Shared trample behaviour

Both creatures use the already-accepted automatic enemy-aware response: one
legal native opportunity attack at its configured penalty, or a Reflex save for
half, and never both. Target-size legality, one contact per target per round,
exact command ownership, interrupt and death behaviour, action spending and the
ordinary-attack fallback are all preserved, and both new registrations
participate in the existing interruption and duplicate-response guards.

## Accepted limitations, unchanged

The four owner-accepted engine limitations carry forward at their existing
scope with no new waiver:

- `ACTIVE_SUMMON_GRAPPLES_RESET_SAFELY_ON_RELOAD` — a save and reload reset
  active grabs, holds, swallowed and engulfed relationships for every KMG
  summon. This is a clean reset, not grapple persistence.
- `SWALLOW_WHOLE_INTERIOR_AC_HP_UNMODELED`
- `PASSIVE_CREATURE_SENSES_UNMODELED` — Scent, Darkvision and Low-light Vision
  have no faithful native carrier and nothing substitutes for them.
- `RUN_NATIVE_SUMMON_AI_DISPOSITION`

Owner visual review remains `NOT_PERFORMED_NONBLOCKING`.
