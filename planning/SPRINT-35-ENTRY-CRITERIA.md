# Sprint 35 entry criteria: grit resource and deed framework

## Authority

- `AUTONOMOUS-GUNSLINGER-MISSION.md` sections 4.2, 4.3, and 6.
- `planning/ROADMAP-SPRINTS-29-38.md`, Sprint 35.
- Authorized local `GUNSLINGER_PFSRD.md`, Grit and Deeds sections.

## Exact resource rules

- Base maximum grit is the Wisdom modifier, minimum 1.
- Daily reset restores current grit to that maximum.
- Explicit future bonuses may increase the maximum but cannot reduce the
  minimum or produce a negative maximum.
- Current grit is always between zero and maximum, inclusive.
- Spending rejects nonpositive costs and insufficient balances atomically.
- Restoration rejects nonpositive amounts and clamps at maximum.
- Maximum changes clamp current grit without granting an implicit refill.
- Explicit operation identities prevent duplicate spend or restoration from
  applying twice; unrelated unit stores never share state or operation history.

## Observable source checkpoint

- A dependency-free domain model represents bounded grit state.
- Deterministic services implement daily reset, maximum reconciliation, spend,
  restore, and duplicate-operation protection.
- Focused tests cover negative/zero/positive Wisdom modifiers, bonuses, bounds,
  insufficient spend, capped restore, deduplication, and unit isolation.

## Remaining Sprint 35 work

- Bind maximum to an exact Kingmaker per-unit ability resource.
- Persist current grit through save/load and preserve it through ordinary
  level-up and multiclass behavior.
- Reset through the exact daily/rest contract without resetting on encounter or
  mode changes.
- Add exact firearm critical/killing-blow recovery eligibility and event dedupe.
- Add reusable deed availability/cost plumbing and guarded runtime acceptance.

## Non-goals for the first source checkpoint

- No deed behavior, firearm-event recovery, UI, save mutation, balance choice,
  alternative grit/panache pooling, favored-class bonuses, or speculative
  Harmony patch.
