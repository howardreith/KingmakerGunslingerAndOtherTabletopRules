# Sprint 16 laptop publication attempt and missed-bite fixture repair

## Disposition

**Publication NOT QUALIFIED.** Exact source
`3d13108ea941af4e736ca30e6d9cc78a67796d63` passed all prelaunch gates,
then smoke **11/11 PASS** and the complete publication scenario **207/209 FAIL**.
All twenty public routes passed (fourteen Crocodile, six Dire); that does not
waive the two combat failures or close Sprint 16. Hidden qualification of
`e3aeae630d51d27c45694fb9e92e9093048b1af9` remains recorded separately.

The actual leased installation was restored exactly at
`2026-10-06T00:24:45.7342342Z`. No game, runtime lease, shared compatibility
lock or staging remains. No save write was requested by this two-stage
publication batch; the working save remains the native clean result of the
preceding closed hidden protocol. No save file or protected baseline was accessed.

[Exact artifact, request, result and restoration hashes](EXPANDED-SUMMONING-SPRINT16-LAPTOP-PUBLICATION-RETRY-EVIDENCE.json).
Raw evidence and immutable package are retained under ignored
`artifacts/laptop-runtime-20261006T0009112031605Z` and the named runtime directories.

## Every failure classified

| Assertion | Evidence and disposition |
| --- | --- |
| `sprint16-crocodile-rtwp-manual-hold` | Fixture prerequisite/observation defect. Issued non-opportunity bite missed at frame 195; bonus 103. The later tail hit at frame 270 for 1d12+2. Zero maneuvers, one attack attempt, 671 frames until the existing 60-second deadline. The retry predicate incorrectly required a grapple check, which a missed bite never emits. Native die/AC were not recorded, so no natural-1 explanation is claimed. |
| `sprint16-crocodile-rtwp-manual-hold-contact-death-roll` | Consequence of the same missing hold/rider, not evidence of a new contact-binding defect. No Death Roll occurred to measure. |

Mechanics 37/37, speed 19/19, icons 9/9, other combat assertions 53/55,
and final review 81/81 passed. The full public census is 20 published / 0
private; source remains 976 generated + 29 wrappers = 1005 visible choices.
No environment, restoration or additional production defect was observed.
The only production delta from hidden e3 is Dire suppression removal.

## Bounded repair in the containing descendant

- Retry an observed, completed issued attack even when it produced no
  grapple check. Preserve readiness, living target, no relationship/rider,
  no pending command, maximum four commands and 60-second bounds.
- Do not advance a fresh turn before queuing that retry. Advance only the
  fixture owner's spent attack turn or a natively established relationship's
  idle turn, through the existing native turn controller.
- Add a disclosed deterministic negative input: first bite `AutoMiss` only
  for the exact owned Crocodile/manual-hold pair, only while its issued
  command is active, once per mode. No positive hit/check/rider override.
- Add two runtime assertions, one per mode: the miss is an issued non-AoO,
  emits zero maneuvers, and a bounded later command hits without `AutoHit`
  or `AutoMiss`, establishes the relationship and resolves its native rider.
  Existing 209 assertions remain; the next main scenario has 211.
- Record native die, target AC, result and override flags, plus retry timing
  and pre-retry maneuver count. This replaces the ambiguous first-miss
  observation instead of increasing waits or waiving any assertion.
- Behavior tests cover every retry prerequisite/cap, fresh-turn handling,
  and exact-pair/driver/weapon/once-only negative-input policy.

No production mechanic, visual, AI brain, identity, asset, publication count,
save policy or version changes. No DATA imports. Source checks are not runtime
qualification; next freeze/push the corrected exact candidate, run all exact
prelaunch gates, then the complete affected smoke/publication batch and restore.
Sprint 17 waits. HumanReview: NOT_PERFORMED_NONBLOCKING.

## Exact restoration

Snapshot `20261006T0009119881581Z`; lease
`runtime-20261006T000911Z-a56b01d61dff4bcc8a1740fcc78e7399` Completed,
recoveryRequired=false. Before and after: **136 files / Info 0.0.117**,
tree `216A9DC2B8E95CD644BA3CADC69A638463C25E60F40A11F8D4B2065C69D5AAF3`.
Batch journal SHA-256
`7A4109860C9EA7F445C0A5AC1D129BAA16758B92B2FF80E5BAD999791AC8AD5A`.
Post-restoration fetch found active remote exactly equal to 3d13108e.
Sole laptop source ownership remains active/session-scoped.
