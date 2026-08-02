# Sprint 56 entry criteria: Cheat Death

## Authority

The authoritative local Gunslinger text requires Cheat Death at Gunslinger
level 19. Whenever the Gunslinger is reduced to 0 or fewer hit points, she may
spend all remaining grit, with a minimum of 1 grit required, and instead finish
at exactly 1 hit point.

## Exact Kingmaker contract

- The installed `RuleDealDamage.OnTrigger` calculates final damage after native
  reductions and difficulty modifiers, applies that amount through the target's
  public `UnitEntityData.Damage` setter, and then completes the rule event.
- A `RuleTargetLogicComponent<RuleDealDamage>` receives the completed event for
  its owning unit and can inspect the final `Target.HPLeft` and `evt.Damage`.
- `UnitEntityData.HPLeft` is exact maximum hit points minus the descriptor's
  damage total. Setting `Damage` to `MaxHP - 1` therefore leaves exactly 1 HP.
- The damage setter clamps only at zero and does not perform unrelated healing,
  damage delivery, save mutation, inventory mutation, or persistence I/O.

## Required behavior

1. Grant one stable Cheat Death feature exactly once at Gunslinger level 19.
2. React only to a completed, non-null `RuleDealDamage` whose exact target is
   the owning unit and whose final `HPLeft` is 0 or lower.
3. Require current Gunslinger level 19 or higher and at least 1 current grit.
4. Spend every currently remaining grit point. This variable cost is the full
   current pool and is not reducible by True Grit or any other deed-cost effect.
5. After the exact spend succeeds, set damage to `MaxHP - 1` and verify both
   current grit 0 and final `HPLeft` 1.
6. Apply at most once per exact damage-rule reference. A duplicate callback must
   not spend or mutate again.
7. If grit spending or HP replacement fails, restore the prior grit and damage
   values atomically and record a bounded runtime failure.

## Fail-closed boundaries

- Do nothing for positive final HP, zero grit, Gunslinger level below 19, a
  different target, null rule/owner/resource/class/fact, fake observation that
  does not reduce HP, or a duplicate rule callback.
- Cheat Death does not prevent non-damage death effects, ability-score death,
  Constitution reduction, death conditions, scripted death, dismissal, or
  already-dead state unrelated to the completed damage rule.
- It does not alter the incoming rule's damage result, native reductions,
  temporary hit points, source attribution, attack correlation, or damage log.
- Runtime qualification must use detached disposable units, must not load or
  write a save, and must restore/dispose all test state.

## Qualification evidence

- Focused domain cases must cover lethal application, all-grit cost, positive-HP
  rejection, zero-grit rejection, level gate, duplicate rule protection, and
  invalid input.
- Repository validation, complete domain suite, clean exact-reference Release
  build, and strict standalone package validation must pass.
- The exact commit must pass guarded `mod-load-smoke`.
- Two independent guarded Cheat Death runs must each prove a native completed
  damage rule leaves exactly 1 HP, spends the full nonzero grit pool to zero,
  rejects zero grit without changing the lethal result, isolates another unit,
  and cleans up without save access.
