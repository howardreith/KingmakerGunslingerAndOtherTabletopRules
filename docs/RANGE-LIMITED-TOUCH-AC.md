# Range-limited firearm touch AC

## Rule implemented in Sprint 9

An exact early firearm attacks touch AC only in its first firearm range increment. At greater distances it attacks ordinary AC.

The Test Musket marker defines:

```text
Era: Early
Kind: Musket
Range increment: 40 feet
Engine boundary: 12.192 meters
```

A 0.1-millimeter boundary tolerance prevents floating-point representation noise from classifying an exact-boundary shot as the next increment.

The implementation does not use the borrowed Heavy Crossbow category to identify a firearm. It requires exactly one `FirearmDefinitionComponent` on the concrete weapon type.

## Why the implementation uses a delta

Kingmaker's calculated `TargetAC` may contain contextual changes beyond the target's base ordinary AC, including cover or flat-footed adjustments. Replacing it with raw touch AC would discard that context.

Sprint 9 therefore applies:

```text
delta = target touch AC - target ordinary AC
selected TargetAC = current rule TargetAC + delta
```

Examples:

| Situation | Ordinary AC | Touch AC | Current rule TargetAC | Selected firearm AC |
|---|---:|---:|---:|---:|
| No extra context | 20 | 12 | 20 | 12 |
| +4 contextual cover | 20 | 12 | 24 | 16 |
| Flat-footed reduction already applied | 20 | 14 | 16 | 10 |

This keeps the ordinary `RuleAttackRoll` and weapon-damage path intact. It does not alter concealment, mirror images, attack bonuses, damage, criticals, or range penalties.

## Runtime sequence

```text
RuleAttackRoll prefix
    identify concrete weapon by marker
    push immutable attack frame

RuleCalculateAC OnTrigger
    Kingmaker calculates normal TargetAC

RuleCalculateAC postfix
    resolve exact firearm marker
    read participants and distance
    read ordinary/touch/current AC
    select ordinary or touch AC
    write only the adjusted TargetAC
    weak-stamp the event; duplicate callbacks are skipped
    optionally log the decision

RuleAttackRoll postfix
    pop attack frame
```

## Fail-closed conditions

Ordinary AC is retained when:

- There is no weapon.
- Marker count is not exactly one.
- The firearm definition is missing.
- The firearm is advanced in Sprint 9.
- Distance is unavailable or invalid.
- The attack is beyond the first increment.
- AC arithmetic would overflow.
- `TargetAC` is missing, non-Int32, read-only, or ambiguous.
- Ordinary or touch AC cannot be read.
- The same AC event has already been adjusted.

## Diagnostics

With combat tracing enabled:

```text
[firearms][ac.touch-selected]
[firearms][ac.ordinary-selected]
```

Fields include:

- `weaponType`
- `distanceMeters`
- `rangeIncrement`
- `previousTargetAC`
- `selectedTargetAC`
- `adjustment`
- `targetMember`
- `reason`

The rule remains active while tracing is disabled.
