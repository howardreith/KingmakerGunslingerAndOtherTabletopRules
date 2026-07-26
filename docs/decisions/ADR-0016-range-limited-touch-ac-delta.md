# ADR-0016 — Apply range-limited touch AC as a contextual delta

## Status

Accepted for Sprint 9; runtime verification pending.

## Context

Firearms need to remain ordinary ranged weapon attacks so normal attack modifiers, damage, feats, criticals, cover, concealment, and other Kingmaker systems continue to participate.

Cowboys and Demons demonstrates the useful principle of changing firearm AC selection through `RuleCalculateAC` rather than replacing the attack with an ability. Its Wrath component adds the difference between touch and ordinary AC to the event. Kingmaker's exact event and component contracts are not assumed to match Wrath.

A raw assignment to the target's touch AC would risk erasing contextual changes already calculated by Kingmaker.

## Decision

For an exact early firearm in range increment one, with a 0.1-millimeter tolerance at range boundaries to absorb floating-point conversion noise:

```text
selected TargetAC = current TargetAC + (touch AC - ordinary AC)
```

Apply that selection in a guarded postfix after the exact installed `RuleCalculateAC.OnTrigger(RulebookEventContext)` callback and before the optional after-trace snapshot.

Use a short-lived `RuleAttackRoll` context to carry only immutable marker metadata. Prefer a directly resolved weapon on the AC event when available. Use a weak per-event stamp to prevent duplicate application.

Fail closed to ordinary AC if any required runtime contract is unavailable or ambiguous.

## Consequences

Positive:

- Normal weapon attack and damage paths remain intact.
- Contextual AC changes are preserved.
- Native Heavy Crossbows are excluded by marker identity.
- The range rule is pure and independently testable.
- Duplicate callbacks cannot stack the delta.

Negative:

- The approach depends on callback nesting or direct reason-item resolution.
- It requires reflective access to one writable `TargetAC` member.
- Postfix timing must be verified against Kingmaker and other mods.
- Advanced firearm penetration is deliberately deferred.
