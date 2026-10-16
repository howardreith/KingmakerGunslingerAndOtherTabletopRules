# v0.0.156 — Firearm automatic continuation after a kill

`0.0.156-firearm-rtwp-kill-continuation`

This patch fixes a real-time stall for firearm Gunslingers. After you ordered a
full attack and its last target died, the Gunslinger stopped: party AI would
not reload or attack a remaining enemy until you clicked again, because the
finished order stayed active and blocked every later automatic reload and
attack.

An explicit firearm order now ends normally once its attack has finished and
its target is dead, provided the equipped firearm is currently in Normal
condition. The Gunslinger then reloads and keeps fighting under party AI, as
with any other weapon. This also holds after a misfire broke the firearm and
Quick Clear repaired it, and when the target dies between an automatic reload
and the attack it was resuming.

| Firearm when the ordered target dies | What happens next |
| --- | --- |
| Normal | AI reloads and attacks a remaining enemy without a click |
| Broken, then repaired to Normal with Quick Clear | Same as Normal |
| Currently Broken | Unchanged: a new explicit order is needed |

Unchanged: misfire and Broken-firearm rules, retargeting within a full attack,
inline reloads, attack counts and iterative penalties, action costs,
ammunition use, Dead Shot, scatter, ordinary weapons and turn-based combat. No
blueprint, identity, route, asset or save data changes; v0.0.155's 1,122
summon choices are unchanged.

## Limits

- The originally reported symptom, later attacks of the *same* full attack
  stopping after the first kill, was not reproduced in the automated save-free
  fixture, where all four attacks continue to new targets. That fixture
  supplies animation timing itself, so a live animation or mod-interaction
  cause is not ruled out. If you still see it, please report it with the
  combat log from that moment.
- The patch was not observed in a live game with normal animation playback
  before release.
- A firearm that is Broken without a misfire in the current session (for
  example, loaded from a save while Broken) still keeps its order after the
  target dies until you click.
- While an ordered target is still alive, AI stays on that target, as before.

Checked with the order-ledger domain tests (including the misfire and Quick
Clear item transitions) and the guarded save-free native firearm scenario,
including a genuine ally kill during the reload gap.
