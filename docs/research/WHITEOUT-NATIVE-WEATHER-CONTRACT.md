# Whiteout native weather contract — 2026-10-05

Disposition: **BLOCKED-WEATHER-FIXTURE / BLOCKED-WITH-EVIDENCE**.
WhiteoutPublished: false.

The synchronous native weather mutation worked at both qualified sites. The
complete fixture did not qualify: the native mansion/Oleg round trip replaces
preexisting scene units and disposes their original facts. The mission requires
exact original unit/area/scene references, so restoring matching IDs, blueprint
identities, party positions and weather is insufficient. The rejected executable
fixture and its scenario registration were removed from final source. Only
read-only native contract tests and curated research remain.

## Native identity and members

Installed `Assembly-CSharp.dll` MVID:
`07fa1e4d-8618-41b3-9b8d-faa17d3b26f7`; SHA-256:
`3b6450ffec440e296e586f71c711b195aed144b28d53e1cbb29406d18fef5afb`.
Private installed IL was inspected locally; no native assembly, IL dump, save,
raw runtime artifact, or machine configuration is committed.

| Surface | Exact contract and consequence |
|---|---|
| Saved weather | `Player.Weather: Player.WeatherData`; `CurrentWeather: InclemencyType`, `NextWeatherChange: TimeSpan` remained untouched |
| Actual precipitation | `Player.WeatherData.get_ActualWeather()` reads `WeatherSystemBehaviour.Instance.WeatherType`, `RainIntensity`, `SnowIntensity`; 150 IL bytes; it does not read the saved schedule |
| Visual carrier | Existing `WeatherSystemBehaviour.Instance`; `WeatherType: WeatherType`, `RainIntensity: float`, `SnowIntensity: float`; no replacement singleton or visual `Init`/`Update` |
| Thresholds | `BlueprintRoot.Instance.WeatherSettings.RainIntensitites.Values` and `SnowIntensitites.Values`; five increasing finite values |
| Threshold mapping | First index i with intensity less than Values[i] minus 0.01 yields inclemency i−1; otherwise Storm. Normal/absent carrier is Clear |
| Rain Light | Exact Values[1] observed as 0.15; other precipitation intensity zero |
| Snow Light | Exact Values[1] observed as 0.75; other precipitation intensity zero |
| Native controller | `Game.GetController<WeatherController>(includeInactive: true)` returns the existing mode-owned instance; pause removes its active event subscription but does not require creating or activating another controller |
| Native notification | `WeatherController.OnUpdateWeatherSystem(bool overrideWeather)`, 105 IL bytes; true sets `m_Overridden`, refreshes `m_SeasonData`, skips its intensity setter, and invokes native `EventBus.RaiseEvent<IWeatherChangeHandler>` |
| Controller restoration | Exact private `m_SeasonData: WeatherRoot.SeasonalData` reference and `m_Overridden: bool` captured before the first write and restored in finally |
| Unsafe alternative | `WeatherController.Tick()` can write `CurrentWeather` and `NextWeatherChange`; it was never invoked by the fixture |
| Indoor point | `LocalMapArea.IsIndoor(Vector3)`, 46 IL bytes, resolves `GetClosest(point).AreaPart.IsIndoor`; absent map/part returns false and must be treated as unqualified evidence, not outdoors |

## Frozen interpretation and observed route

**ADAPTED — OUTDOORS ONLY** is the owner's frozen contract. A future exact
provider requires Rain or Snow, ActualWeather Light through Storm, and an exact
qualified map point whose indoor predicate is false. Visual Rain with actual
Clear, Normal, magical fog, waterfall VFX, or unknown map identity cannot qualify.
Forced actual precipitation indoors remains excluded even in authored scenes
with indoor precipitation overrides. This supersedes the earlier undecided
indoor interpretation; the earlier read-only catalog remains historical evidence.

The only exercised route was the existing guarded, no-save mansion/Oleg route:

| Site | Area / exact area part | Native indoor | Rain Light policy | Snow Light policy |
|---|---|---|---|---|
| Jamandis mansion | `2849fdde28fe50f4d935bf2cf3405051` | true | inactive | inactive |
| Oleg | `ead426a6c23d39548a670ee515d77df4` | false | active | active |

The activation column is the existing pure policy with a conceptual marker input.
No Whiteout marker or protected actor was granted, and no attack was altered.
Both executed sites produced exactly five weather-change deliveries: Rain,
repeated Rain, Snow, Clear, and original-state restoration. Repeated Rain did not
duplicate a policy transition. Original mansion weather was Normal/Clear; Oleg
was visual Rain with ActualWeather Clear. All eight explicit mutations occurred
synchronously within two using/finally scopes, without a yielded frame, scene
change, visual Update, controller Tick, campaign-time advance or transition
coroutine while fields were changed.

## Native subscriber safety and exact weather rollback

Before mutation the rejected fixture inspected only the exact native global
`IWeatherChangeHandler` subscription list while idle, through
`SubscriptionManager<IGlobalSubscriber>.m_Listeners`, the exact typed dictionary,
and `SubscribersList<IGlobalSubscriber>.List`. It did not replace the event bus,
remove foreign listeners, or synthesize a substitute notification.

The mansion had one native SoundState, 18 UnitPartPartyWeatherBuff instances,
124 AddBuffInBadWeather components and one owned observation probe. SoundState
has its side effect at Storm, which was not exercised. Every party weather part
had null m_LastBuff. Every conditional component had WhenCalmer=false and
threshold Heavy, with the target buff absent; its exact native expression
`WhenCalmer == (ActualWeather < Weather)` selected the removal/no-op branch for
both Clear and Light. An unknown listener, an existing party weather buff, or
any conditional grant/removal that could change existing facts was rejected
before the first write. The first run discovered these conditional listeners;
inspection narrowed the admitted state without suppressing any native handler.

For each site, before-state included visual type/rain/snow, saved CurrentWeather,
NextWeatherChange and ActualWeather, the exact singleton and controller,
m_SeasonData reference, m_Overridden value, ordered effective weather listeners,
and existing unit buff references. Finally restored the three visual fields,
called the exact native notification for the original state, then restored both
controller fields. Both per-site restoration checks passed; saved weather and
campaign time never changed. The owned weather/scene probe was unsubscribed.

## Exact scene-reference blocker

The fourth guarded run reported 13/14 assertions PASS and overall FAIL:
`whiteout-fixture-preexisting-units`. Original unit count was 130; after the
round trip it was 133. There were 127 missing original object references and
130 added references. All 127 replaced objects retained matching unit IDs;
three were additional objects. Eight old objects had changed buff collections:
seven formerly carried blueprint `5898bcf75a0942449a5dc16adc97b279` and one
`c98d765d063f57a49a03f13d4f697c33`; their old collections were empty on return.
No feature investigation or modification was performed for those identities.

`PersistentState.Units.All` is a HashSet, so enumeration order alone is not a
valid blocker. The observed replacement references and disposed facts are.
The exact installed chain explains this independently of the weather writer:

1. Private `Game.LoadArea(BlueprintArea, BlueprintAreaEnterPoint, AutoSaveMode,
   bool forceUnload, SaveInfo)` selects cross-area unloading even with
   forceUnload=false and AutoSaveMode.None.
2. `SceneLoader.<UnloadAreaCoroutine>d__35.MoveNext()` (942 IL bytes) calls
   `AreaDataStash.StashAreaState` after deactivation.
3. `AreaDataStash.StashAreaState` (267 IL bytes) disposes the area state.
   `SceneEntitiesState.Dispose` / `RemoveEntityData` dispose original entities.
4. `UnitEntityData.Dispose` (110 IL bytes) clears unit subscriptions and disposes
   UnitDescriptor; `UnitDescriptor.Dispose` (193 IL bytes) disposes its fact
   collections and unit parts. Re-granting an equal blueprint cannot restore
   the original objects and event registrations.
5. `SceneLoader.<LoadAreaCoroutine>d__24.MoveNext()` calls UnstashAreaState;
   `AreaDataStash.UnstashAreaState` (150 IL bytes) deserializes and installs
   replacement scene state.

The native loader manages its own area cache; Codex did not access, parse,
copy, rewrite or otherwise manipulate those files or any raw save. The
save-writing guards observed zero save writes in all four runs. Native area
cache activity must not be misreported as exact in-memory reference restoration.

Alternatives inspected: override notification needs no saved-schedule change;
the existing paused controller can be obtained exactly; these resolved the first
two safe preflight failures. Using only the mansion cannot prove a loaded outdoor
point. The existing Oleg no-save transition still disposes original scene state.
ForceUnload=true cannot preserve it. Hot-scene preservation exists for compatible
native same/shared-scene transitions, but no qualified mansion/Oleg route avoids
the measured replacement. Loading late/scripted destinations, inventing an
additive scene/party fixture, bypassing native stash/dispose, resurrecting disposed
facts, weakening the reference assertion, or writing/reloading a save does not
provide an already-qualified reversible alternative. None was attempted.

The original area blueprint, party references/positions, inventory/counts,
money, clock, pause state, camera values, weather singleton/saved-weather
references and values, and owned listener cleanup passed. Exact original scene
unit/fact references did not. Each failed process exited automatically; the
unwritten in-memory scene changes were discarded with that process, and the
exact leased live-mod snapshot was restored afterward. This is a blocker,
not a complete scene-restoration PASS.

No production weather adapter, attack patch, marker, icon or acquisition path
was installed. The four evidence identifiers and exact artifact identities are
recorded in the [mission handoff](../../CODEX-WHITEOUT-UNPUBLISHED-FOUNDATION-HANDOFF-2026-10-05.md).


## Disposable-process continuation — 2026-10-05 owner decision

The original failure and rejected reference-restoration requirement above remain
historical evidence. The owner explicitly accepted native non-party scene
replacement only inside an automatically exiting, guarded, disposable no-save
process. No claim is made that disposed mansion NPC/fact references were restored.

NonPartySceneReferenceRestorationRequired: false
DisposableNoSaveProcessIsolation: true
WhiteoutPublished: false

The reconstructed `observe-whiteout-disposable-weather-fixture` follows exactly
one native mansion → Oleg transition, with `AutoSaveMode.None`. It never returns
to the mansion, retains no prior-scene NPC/fact references, and uses the exact
existing visual carrier/controller/threshold/event/indoor contracts above. Each
site restores its three visual fields, two controller fields, saved weather
values/references, ordered native listeners and existing same-area buff references
inside its synchronous finally scope, before that scene is left or the process
exits. The owned weather probe is removed before transition and after Oleg.

Gate A: **WEATHER-FIXTURE-QUALIFIED**, 15/15 PASS. Native Rain/Light, repeated
Rain, Snow/Light, Clear and original-state restoration deliver five exact native
weather events per site. Mansion remains policy-inactive; the real Oleg map/part
is outdoors and policy-active under Rain/Snow. Party/character/inventory identities
and counts, money, GameTime and saved weather schedule remain unchanged before
transition and before exit. No player state or quest/kingdom state is deliberately
changed. Zero save-writing API calls, no raw save operation, automatic exit.

Run `20261006T0402483970988Z-50487eef976d485bacab33716fce964f`, PID 39268.
This is an engineering artifact embedded at exact base `5c59150a…` with its dirty
source fingerprint; it qualifies the revised weather gate, not Whiteout combat
or a clean-source final foundation. All 2,148 domain checks, repository/static/
icon gates, exact Release build, deterministic package and strict standalone
package validation passed; runtime preflight passed 492 checks. The exact leased
live snapshot restored byte-for-byte and the lease Completed.

The continuation handoff will record final clean-source attack qualification.
No attack adapter or Whiteout provider exists at this weather-only checkpoint.


## Final disposable-process foundation qualification - 2026-10-06

Current continuation outcome: **WEATHER-FIXTURE-QUALIFIED** and **QUALIFIED
UNPUBLISHED FOUNDATION**. The historical scene-reference blocker above remains
accurate under its original mission; the owner accepted native scene replacement
inside the one-way no-save process only. Two complete fresh-process runs at clean
commit `db3725d7eb665731c3b8c238217cd39f2d0a65b8` passed 65/65 each, followed by
working-save smoke 11/11 on the same exact artifact. Actual indoor Rain/Snow Light
kept the real provider inactive; outdoors both activated it. Each complete run
restored both weather scopes, removed all owned facts/actors/listeners/forces,
recorded eight mutations and ten exact native notifications, and auto-exited.
The shared acquired 254-file live snapshot was restored byte-for-byte and the
lease Completed. No scene-reference restoration after unloading is claimed; no
save-writing API was observed. See the [continuation handoff](../../CODEX-WHITEOUT-DISPOSABLE-PROCESS-CONTINUATION-HANDOFF-2026-10-05.md)
for exact artifact hashes, PIDs, evidence and publication-negative ledger.
WhiteoutPublished: false.
