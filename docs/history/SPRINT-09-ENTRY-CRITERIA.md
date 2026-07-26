# Sprint 9 entry criteria — range-limited touch AC

## Goal

Implement the smallest firearm rule mutation: an early firearm attacks touch AC in its first range increment and ordinary AC beyond it, while remaining a normal ranged weapon attack.

## Preferred runtime gate

Before the mutation is promoted as runtime-complete, a compiled Sprint 8 UMM build should demonstrate at least one coherent Test Musket trace containing:

- Concrete Test Musket item and marked weapon type.
- Natural d20 or a documented unavailable result.
- `RuleCalculateAC.TargetAC` before and after calculation.
- Ordinary and touch AC from the same target.
- Distance and calculated range increment.
- Full/standard attack shape where exposed.
- No corresponding trace for a native Heavy Crossbow.

If those logs are unavailable, Sprint 9 may produce a source implementation and expanded contract checks, but it must remain labeled unverified and not **READY FOR KINGMAKER**.

## Allowed work

- A firearm-specific AC rule service with pure tests.
- One marker-scoped Harmony adapter or weapon enchantment component.
- Distance/range checks using the observed engine convention.
- An opt-in diagnostic showing selected AC and reason.
- Regression checks for native Heavy Crossbows.

## Forbidden work

- Ammunition, reload, empty-fire prevention, misfires, grit, class progression, vendors, crafting, models, sounds, or animations.
- Applying touch AC beyond the first range increment for the early Test Musket.
- Replacing the entire attack pipeline or converting the shot into an ability.
- Firearm identity based only on Heavy Crossbow category.
- Suppressing cover, concealment, mirror images, range penalties, or ordinary weapon modifiers.

## Acceptance

1. A close Test Musket attack uses touch AC.
2. A distant Test Musket attack uses ordinary AC.
3. A native Heavy Crossbow uses ordinary AC at both distances.
4. Deadly Aim and normal weapon damage remain on the standard attack path.
5. The AC mutation occurs exactly once per attack.
6. All behavior is covered by pure rule-selection tests and a documented in-game matrix.
