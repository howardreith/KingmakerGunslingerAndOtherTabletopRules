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

## Followup result: policy and observation only

Disposition: **PARTIAL-SAFE-FOUNDATION**. There is no registered Whiteout marker,
buff, attack patch, acquisition path, icon or public setting. The policy/state/
replay models have deterministic coverage; they are not bound to gameplay.

Guarded scene run `20261005T0540413348351Z-observe-whiteout-weather` observed
indoor mansion `2849fdde28fe50f4d935bf2cf3405051` as Normal/Clear and outdoor
Oleg `ead426a6c23d39548a670ee515d77df4` as visual Rain with ActualWeather=Clear.
The exact native indoor predicates agreed. The proposed activation was false
in both, two load/unload pairs were observed, and no weather-change notification
was fabricated. The round trip restored the observed state with zero save writes.

The safer no-save catalog run
`20261005T0556341614778Z-observe-whiteout-weather-catalog` then inspected 607
registered area parts. Eight have authored precipitation overrides at Light or
higher; five are marked indoor: FinalDungeon, FinalDungeon2, FinalDungeon3,
CultistsVillage, and HouseAtTheEdgeOfTime_FB. Native scene projection contains
an indoor OverrideWeather exception. These configured identities are evidence
against assuming all indoor areas are clear, but are **not loaded-scene proof**
that precipitation remains active indoors. Neither adding an indoor gate nor
omitting one is runtime-qualified by these observations.

**Exact remaining prerequisite:** a safely controlled, native active-precipitation
indoor/outdoor observation, followed by real weather-event and attack qualification.
The existing harness has no deterministic weather fixture; source inspection
found no qualified WeatherType/intensity setter or weather-event injector.
The working-save round trip stays Clear. The authored alternatives are late,
scripted or unqualified area/part destinations, outside the established safe
round-trip fixture. Loading one indiscriminately or forcing shared weather would
substitute an unproven fixture for the missing contract. Mission section 11.4
requires stopping this slice at policy plus observation when indoor evidence is
ambiguous. No indoor rule is chosen. This is a current qualification boundary,
not a claim that a future narrowly designed fixture or patch is impossible.

The native private instance method
`RuleAttackRoll.TryOvercomeTargetConcealmentAndMissChance(): bool` does exist
(zero parameters, 149 IL bytes on native MVID
`07fa1e4d-8618-41b3-9b8d-faa17d3b26f7`). A missing patch point is **not** the blocker.
No attack patch was installed, so no two-process combat qualification is claimed.
See the mission handoff for exact assembly identities, failed attempts, final
integrated reruns and restoration records.


## 2026-10-05 native-foundation mission — superseding disposition

**BLOCKED-WITH-EVIDENCE / BLOCKED-WEATHER-FIXTURE. WhiteoutPublished: false.**
The owner now explicitly freezes **ADAPTED — OUTDOORS ONLY**: exact native
Rain/Snow and ActualWeather Light+ qualify only at an observed outdoor point.
This supersedes the older undecided indoor interpretation above; historical
catalog observations remain historical rather than loaded-scene proof.

A synchronous request-local mutation of the existing native visual weather
carrier, followed by `WeatherController.OnUpdateWeatherSystem(true)`, produced
actual Rain/Light and Snow/Light at the mansion and Oleg. Indoor policy remained
inactive, outdoor policy became active, repeated notification was idempotent,
and both weather scopes restored captured values/references/listeners without
changing the saved schedule or campaign time. This is new native weather
evidence, not a Whiteout attack or complete fixture qualification.

The overall fixture failed its stricter exact original unit/fact-reference
restoration check. The native cross-area loader disposed and rehydrated 127
preexisting non-party unit objects, added three objects, and emptied original
buff collections on eight disposed objects. Equal IDs on replacement units do
not restore original references. Every process exited automatically with zero
save writes, and its exact leased live-mod snapshot was restored. The rejected
weather writer and scenario registration were removed from final source.

[The native weather contract](WHITEOUT-NATIVE-WEATHER-CONTRACT.md) records the
exact setter/event route, thresholds, subscriber safety and scene-lifecycle
blocker. [The attack contract](WHITEOUT-NATIVE-ATTACK-CONTRACT.md) records an
inspected exact narrow boolean seam; it remains unpatched and has no Whiteout
combat qualification. The pure policy/state/replay foundation is unchanged.
No marker, provider, icon, localization, acquisition or production binding exists.
