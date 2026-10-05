# Sprint 16 blocker: swallow-whole interior armour class and hit points

## Current disposition, 2026-10-05

**Accepted; not a blocker.**
`OwnerAcceptedEngineLimitation: SWALLOW_WHOLE_INTERIOR_AC_HP_UNMODELED`.
The fresh live census in
`20261005T1254571775971Z-disposable-expanded-summoning-crocodilians` confirms
no faithful carrier. See `EXPANDED-SUMMONING-SPRINT16-SURVEY-REVIEW.md` for the
complete type/component audit, correction of the old `SwallowWholeSettings`
claim and exact hashes. The acceptance covers Dire Crocodile and Purple Worm
without reopening the Worm's qualified mechanics. No custom interior subsystem
or replacement save/check is authorized.

Size legality, initial grapple/maintain/bite, exactly one later-round damage
bundle, native escape timing/checks, exact source ownership and every cleanup
boundary remain mandatory. This decision waives none of those gates. Once the
census confirms the omission and the other gates pass, the interior numbers
alone must not withhold Dire Crocodile or be re-marked BLOCKED.

Dire's explicit creature-owned graph passed its live audit: exactly the native
two stat penalties and one NewRound 3d6+13 physical action; no activation or
deactivation actions. Its **cadence and mechanics remain NOT QUALIFIED** until
the full candidate. The historical rewrite wording below is superseded.

## Historical audit and pre-decision disposition (superseded)

**Status: BLOCKED — submechanic only.** The Dire Crocodile's printed swallow
whole is `3d6+13, AC 16, 13 hp`. The damage is implemented. The interior armour
class and the interior hit points are not, because Kingmaker has nowhere to put
them, and this records the audit rather than letting the policy's stored numbers
stand in for an implementation.

This blocks publication of the Dire Crocodile and nothing else. Every other part
of Sprint 16 continues.

## What the printed rule requires

A creature swallowed whole can cut its way out: it attacks the swallower's
interior, which has its own armour class, and when it has dealt enough
cumulative damage to that interior it is cut free. For this creature the
interior is armour class 16 and 13 hit points — so twelve points of qualifying
damage leaves the victim inside and thirteen cuts it out.

That is three separate things the engine would have to model: a target that is
not the swallower's own body, an armour class for it, and a damage pool
accumulated against it.

## The audit

Against the installed `Assembly-CSharp`, every type the engine's swallow model
is built from:

| Type | Entire surface |
| --- | --- |
| `Kingmaker.UnitLogic.Parts.UnitPartSwallowed` | fields `Swallower`, `SecondsToBreakFreeAttempt`, `ShouldBeFree`, `m_Buff`; methods `Init`, `OnRemove`, `TryToBreakFree` |
| `Kingmaker.UnitLogic.Parts.UnitPartSwallowWhole` | field `SwallowedUnits`; `SpitOutAnimation`; `HasSwallowedUnitsForSpitOut`; methods `Swallow`, `Free`, `SpitOut`, and the spawned/destroyed/death handlers |
| `Kingmaker.Controllers.Units.UnitSwallowWholeController` | one method, `TickOnUnit` |
| `Kingmaker.UnitLogic.Mechanics.Actions.ContextActionSwallowWhole` | one field, `TargetBuff` |
| `SwallowWholeSettings` | **does not exist** — the name appears in the assembly's strings but resolves to no type in any namespace |

There is no interior. There is no armour class field, no hit-point pool, no
accumulator, and no second target to attack. The engine's model of being
swallowed is a state with a timer: `SecondsToBreakFreeAttempt` schedules
`TryToBreakFree`, and the victim either breaks free or does not. Escape is a
periodic attempt, not a damage race.

The project's own swallowers confirm it from the other side. The Purple Worm
has shipped and qualified since Sprint 4 and carries no interior armour class
or hit points either; `PurpleWormSwallowSizeDelta` is the only swallow constant
it has. Nothing in this repository has ever read an interior armour class,
because nothing could.

## What was implemented instead

The Dire Crocodile now has **its own swallowed state** rather than the Purple
Worm's. At the reviewed head it shared `KMG.Summoning.Special.PurpleWorm.Swallowed`,
so a victim inside a dire crocodile was in a worm's stomach taking a worm's
crush. The new state deep-clones the same native components — the break-free
behaviour a victim depends on is the engine's, unchanged — and retargets the
per-round damage to this creature's own `3d6+13`. The Purple Worm's buff and the
Giant Flytrap's engulf are untouched.

So of the printed line, the damage is this creature's and the interior numbers
are absent.

## What was deliberately not done

- **No interior subsystem.** Modelling a second attackable target with its own
  armour class and hit-point pool is a new mechanic, not a wiring job, and the
  mission's scope forbids it.
- **No substitute.** Translating AC 16 into something else the victim rolls
  against, or 13 hit points into a different escape threshold, would be a
  number that looks like the printed one and behaves like something else.
- **No silent inheritance.** The policy stores the printed 16 and 13 as the
  contract an implementation would have to meet. It is not evidence that
  anything reads them, and no record may say the interior is implemented
  because the policy holds the numbers.

## Confirmation still owed

The type audit above is offline, against the reference assembly. The Sprint 16
guarded runtime review carries a census of the live swallowed buff's components
on a spawned creature, which is what can show that no component supplies an
interior armour class or hit-point pool at run time either. If that census
contradicts this note, this note is wrong and the implementation follows.

## Disposition

The Dire Crocodile stays **withheld**. The rest of Sprint 16 — the Crocodile's
grab, death roll and sprint, both original visuals, the skills, the Run
disposition and the whole runtime review — continues and is qualified on its own
terms. If this is still the only thing keeping the Dire Crocodile hidden once
all of that is finished, it goes to the owner as the smallest decision needed:
publish the creature with its interior numbers recorded as an engine omission,
or keep it hidden until the engine offers a seam.

`HumanReview: NOT_PERFORMED_NONBLOCKING`
