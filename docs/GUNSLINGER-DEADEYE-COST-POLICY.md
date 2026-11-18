# Deadeye's cost: the flat one-grit adaptation (POL-02 / S4 T4)

Status: **Accepted and closed for its reviewed development scope** as part of Sprint 4 (principal review, 2026-11-18). Acceptance is not merge or release: not merged, not released. Branch
`codex/pol02-deadeye-cost`, draft PR #181, accepted head `c638de23857724dfaea39c2270467f0a79b12b78`, stacked on draft PR #180 at `ee65d09e9d`. Native evidence comes from the final Sprint 4
candidate (see "Native evidence").

## Contract

Epic S4 T4, inventory POL-02: "Runtime and current player text charge one grit, while a separate computed
range-increment cost is unused." The owner's decision (2026-11-17): **retain the documented flat-one-grit CRPG
adaptation.** Tabletop Deadeye charges one grit per range increment beyond the first, decided at the shot; this mod's
Deadeye is a free-action arming transaction performed before the future attack's target and distance are known, so it
costs one ordinary grit at activation, adjusted only by True Grit. It is not redesigned into a deferred variable-cost
deed. This is an explicit adaptation, not tabletop-exact.

## What the source did

- **The charge.** The registered ability's components are `DeadeyeGritResourceLogic` (the native resource logic) and
  the native apply-buff action. `Spend` charged True Grit's effective cost of a literal 1, once, when the activation
  committed. Player text: "As a free action, spend 1 grit…".
- **Availability and displayed cost** were not set by the mod: with `CostIsCustom` and no cost-calculator component,
  the native `AbilityResourceLogic.CalculateCost` logged an error and returned 1, and `IsAvailableFor` compared that 1
  with the pool. It agreed with the rule by coincidence (True Grit's reduced cost of 0 still requires positive grit).
- **The unused calculation.** `DeadeyeService` computed `cost = increment − 1` and an `InsufficientGrit` status from
  `DeadeyeRequest.CurrentGrit`. Its only production caller, `DeadeyeRuntime.BeforeAttackRoll`, passed `int.MaxValue`
  as the current grit, so the calculation never decided anything; the decision's `GritCost` was read by no production
  code (only by a runtime-test assertion and domain tests). The range increment itself is needed: it decides the
  touch-AC benefit.
- **A second, dead copy.** `DeadeyeAbilityLogic` (an availability check and a delivery, each with a literal 1) was
  referenced by nothing in production: the registered ability never carried it.
- **The armed state** is consumed in `DeadeyeRuntime.BeforeAttackRoll` by any eligible exact-firearm attack, hit or
  miss, within the first increment or beyond it; only beyond the first increment does Deadeye authorize touch AC.
  Non-firearm actions leave it armed. It ends by itself after one round. Nothing refunds it.

## What changed

- `Deeds/DeadeyeCostPolicy.cs` (new, engine-free): `OrdinaryArmingCost = 1` and `Arming(currentGrit, trueGritSelected)`
  through the existing `TrueGritService`. The single authority.
- `Deeds/DeadeyeGritResourceLogic.cs` (renamed from `DeadeyeAbilityLogic.cs`; the dead `DeadeyeAbilityLogic` class is
  removed): `IsAvailableFor`, `CalculateCost` (True Grit's `NativeCheckCost`) and `Spend` all read the policy. No
  player-visible change: availability and the debit are the same as before, now set by the mod instead of the native
  fallback.
- `DeadeyeService`, `DeadeyeRequest`, `DeadeyeDecision`, `DeadeyeStatus`: the affordability (`CurrentGrit`,
  `GritCost`, `InsufficientGrit`) is removed; the service decides only touch-AC eligibility from the range increment.
  Normal range penalties and touch-AC eligibility are unchanged.
- Blueprint identities, names and descriptions are unchanged ("spend 1 grit" already states the rule). No save data.

## Tests

| Command | Result | Time |
|---|---|---|
| `scripts/Invoke-KmgGate.ps1 -Filter 'deadeye;steady-aim;true-grit;archetype'` | production compiled; 49 of 49 | 21 s |

New `deadeye-cost.*`: the arming cost is one grit while the later shot's range moves across the first-increment
boundary (5, 20, 20.5, 40, 40.5, 60.5, 100.5 ft for a pistol), touch AC applying exactly beyond the first increment; no
grit refuses arming (native check cost 1); True Grit arms for 0 with positive grit and refuses at 0. Updated
`deadeye.*`: second-increment and fourth-increment shots stay eligible with the flat cost; a first-increment shot gains
nothing; arming needs grit while the shot's eligibility does not depend on the grit left. Every `DeadeyeRequest` call
site was edited explicitly, because removing the grit argument would otherwise have reinterpreted a six-argument call's
last value as the range bonus.

## Native evidence

The loader-compat `deadeyeCost` probe (`tools/LoaderCompatHarness/DeedLifecycleProbes.cs`) runs on the final Sprint 4
candidate: a Gunslinger levelled by the level-up controller, the registered ability through the ability command,
musket shots through the attack command. Cases: refused activation with no grit; activation spends one grit once; an
armed hit beyond the first increment uses touch AC, consumes the exact instance and spends nothing; an armed miss
beyond it consumes and spends nothing; a first-increment shot consumes and spends nothing; an attack order cancelled
before it starts keeps the state and refunds nothing, and its unaided end refunds nothing.

Result on the final Sprint 4 candidate `16708bd5e237` (package `e017a5f3d8ec…`, DLL `cbca54912642…`, MVID `90ce7e7d-9c5a-46ec-9c95-c0734d2faaec`), session `20261118T014826Z-owner-0.33.0-deadeye-cost`, PASS (also passed on the first
closure candidate `fd4087c312bd`, session `20261118T013600Z`). Musket, target 45 ft away, grit maximum 5:

| Case | Observed |
|---|---|
| No grit | not available; command `Fail`; grit 0 to 0; not armed |
| Armed hit beyond the first increment | grit 3 to 2 at activation; shot measured 43.2 ft, natural 19, hit against touch AC; exact instance removed; grit still 2 |
| Armed miss beyond the first increment | grit 2 to 1; 43.2 ft, natural 10, missed against touch AC without misfiring; instance removed; grit still 1 |
| Armed first-increment shot | grit 4 to 3; 7.18 ft, hit; instance removed; grit still 3 |
| Order cancelled before it starts | grit 3 to 2; order taken, not started, not acted, `Interrupt`; no attack roll; state kept; grit 2; ended by itself after 6.01 s; grit still 2 |

The first-increment shot is also resolved against touch AC: that is the ordinary first-increment firearm rule, not
Deadeye, which authorizes touch AC only beyond the first increment.

## Limits

- True Grit's reduced cost is shown by the domain tests through the production `TrueGritService`; it is not exercised
  natively (that needs a True Grit selection, which is S4 T6's subject).
- The ability's text still says "spend 1 grit" without mentioning True Grit's reduction, as before.
- Deadeye's re-arm rule and pricing questions recorded as NEW-05 and NEW-08 are separate and unchanged.
