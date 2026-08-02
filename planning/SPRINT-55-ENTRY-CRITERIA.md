# Sprint 55 entry criteria - Slinger's Luck

## Authority

The local Gunslinger rules require the level-fifteen deed to reroll a saving
throw for exactly 2 grit or a skill check for exactly 1 grit. The second result
is mandatory even when lower, and neither cost can be reduced.

## Kingmaker adaptation

Kingmaker cannot pause after an arbitrary completed rule roll to request this
tabletop choice. Use explicit personal pre-roll arming abilities for the next
owned saving throw or skill check. Consume only the matching unit-owned marker
and spend the fixed cost immediately before requesting the second native roll.
Never choose the better result and never route the cost through a reducible deed
discount.

## Acceptance criteria

- The feature is granted exactly at Gunslinger level 15.
- Separate player-facing arming choices distinguish saving throws (2 grit) from
  skill checks (1 grit), expose their fixed costs, and cannot arm without enough
  current grit.
- A matching rule event consumes the marker once, spends the exact fixed cost,
  and replaces the first result with a second native d20 result even when lower.
- Unrelated roll categories, other units, duplicate callbacks, insufficient
  grit, and malformed or unsupported rule contracts fail closed without spend.
- The marker is unit-owned and reconstructable; it does not touch firearm,
  ammunition, save-file, or campaign state.

## Deterministic tests

- Pure policy tests cover both fixed costs, level and grit gates, matching and
  nonmatching event kinds, mandatory second-result selection, duplicate use,
  unit isolation, and invalid input.
- Static validation pins every exact native rule/roll member used by the runtime
  adapter and the two non-reducible cost paths.
- Request, preflight, and runner tests pin a guarded save-free feature scenario.

## Required runtime evidence

After a clean source checkpoint and exact-assembly mod-load PASS, two independent
fresh launches must observe level 15, one saving-throw reroll costing 2 grit,
one skill-check reroll costing 1 grit, a deliberately lower second result being
retained, marker consumption, other-unit isolation, and cleanup.

## Entry observation and non-goals

Before implementation, a save-free exact-contract observer must identify the
installed `RuleSavingThrow` and skill-check rule types, their native d20/result
members, and the narrow event phase where a replacement native roll can be
requested without replaying unrelated effects. This sprint does not alter
generic luck/panache pooling, implement post-roll UI interruption, reroll attack
or damage rolls, reduce deed costs, or change any firearm state.

An ambiguous roll lifecycle, result setter, or replay boundary is a failed
observation and blocks implementation until narrowed; it is never guessed.
