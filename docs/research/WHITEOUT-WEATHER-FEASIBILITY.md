# Whiteout: unpublished foundation contract

Reconciled from weekend commit `ee88a63612fdf7d2e63550830f06e835bff5a32d`
for the 2026-10-04 mission on Phase 2A commit
`5482db429bd3c4009a031aa733a3091edfa5fe5e`. The owner has frozen the decisions
below. This document is a contract, not evidence that a live patch has passed.
Current implementation and exact runtime results belong in the mission handoff.

## Publication boundary

Whiteout is not a shipping/selectable trait. No Favored Class `racial_traits`
publication, alternate-trait slot, feat, automatic Undine grant, public setting,
visible trait localization or icon is authorized. A safe foundation may register
stable hidden marker/state identities for a guarded in-memory fixture only.
Eventual character-trait publication is separate work requiring owner review.

## Installed engine findings retained from the weekend

`Player.Weather` exposes `CurrentWeather`, `NextWeatherChange` and computed
`ActualWeather`. `InclemencyType` is Clear/Light/Moderate/Heavy/Storm.
`WeatherSystemBehaviour.Instance` projects `WeatherType.Normal/Rain/Snow` and
rain/snow intensity through `BlueprintRoot.WeatherSettings` thresholds. A
missing weather behavior or Normal yields Clear. The controller raises
`IWeatherChangeHandler.OnWeatherChange`; `UnitPartPartyWeatherBuff` and
`AddBuffInBadWeather` provide event-driven reconciliation precedents.

The native weather concealment fallback is ranged-only and grants Partial
only when no other concealment applies. `RuleConcealmentCheck` hardcodes
Partial=20% and Total=50%. `UnitPartConcealment` combines entries by maximum.
Consequently a fake Partial entry cannot implement this trait's independent
10% chance. No native concealment tier or native chance is to be changed.

There is no Fog weather enum. Magical Fog-descriptor spell concealment alone
must not activate Whiteout. Visual fog is not a gameplay precipitation carrier.
The weekend found no reliable waterfall-spray state; unrelated particles/VFX
have no activation path. Excluding magical fog and waterfall spray is the
bounded Kingmaker adaptation chosen by this mission.

## Frozen weather and attack policy

Activation requires the hidden marker, specifically Rain or Snow, and
`Player.Weather.ActualWeather >= InclemencyType.Light`, subject only to an
indoor exclusion proven necessary by runtime observation. Clear, Normal, fog,
waterfall/VFX and unmarked targets are inactive.

Both melee and ranged attacks are covered. At the actual concealment stage,
ordinary concealment runs first. Only if it has not already caused a miss,
and the attack does not ignore concealment, may active Whiteout roll its own
independent d100. Rolls 1 through 10 inclusive miss; 11 through 100 continue.
Do not add a second miss mutation/processing when ordinary concealment missed.
Seeking/`IgnoreConcealment` bypasses Whiteout as well as ordinary miss chances.

The independent sequential interpretation is
`1 - (0.80 * 0.90) = 28%` for ordinary 20% plus Whiteout 10%, not additive 30%.
Tests must prove the two-stage decision table deterministically, not infer it
from Monte Carlo samples. Native attack, cover, sight, mirror images, critical,
damage and other concealment behavior must remain intact. Unrelated attacks
must consume no additional random roll.

## Runtime prerequisites and patch boundary

Before choosing indoor behavior, observe a reliably outdoor and reliably indoor
scenario through the guarded harness. Record exact area identity,
`BlueprintAreaPart.IsIndoor` or the exact point-based indoor predicate,
visual WeatherType, CurrentWeather, ActualWeather and proposed activation.
The weekend inferred clear indoor weather from the getter; it did not observe
that behavior in game. Do not turn that inference into a runtime claim.

If indoor areas reliably project clear/normal, add no speculative indoor gate.
If precipitation persists indoors, use the narrowest observed exact exclusion.
Ambiguous evidence leaves only pure policy plus observation, unpublished.

Inspect the exact existing `SeekingConcealmentPatch` contract before any patch.
Use only the smallest proven attack/concealment boundary; fail closed if its
signature/ordering is absent. Guard repeated rule events by exact event identity.
No broad patch sweep, generic reflection enumeration in the hot path, global
dice patch, frame polling or periodic broad unit scan is authorized.
Weather/party/area events reconcile per-owner state idempotently; invalid
weather, marker removal and area transition must remove stale active state.

Runtime deterministic hooks must remain guarded and request-local. Qualify
absent-marker control, Light Rain and safe Light Snow, melee/ranged boundary
rolls 10/11, native concealment short-circuit and success-then-Whiteout-miss,
Seeking bypass, clearing, repeated events and exact cleanup. Use only existing
reversible fixture conventions, no arbitrary global weather mutation. A new
attack-pipeline patch needs two consecutive fresh-process PASS runs of the
same assembly before runtime qualification. Save writes are forbidden.

If the exact patch or deterministic weather fixture cannot be proven safe,
retain the strongest safe policy/observation foundation and record the exact
missing contract. Successful compilation is not mechanical runtime evidence.
