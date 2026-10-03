# Whiteout (Undine) weather integration feasibility

**Mission:** Z weekend 2026-10-03, slice 6 — **research only**.
**Absolute restriction honored:** no Whiteout production code, trait
registration, icon, patch, concealment change or weather change was made.
This document records the engine contracts and an implementation
recommendation for owner review.

Rule under investigation (public reference: d20pfsrd
`traits/race-traits/whiteout-undine/`, now HTTP 410; Archives of Nethys
`TraitDisplay.aspx?ItemName=Whiteout`, Blood of the Elements): *the Undine
gains a 10% concealment miss chance in nonmagical fog, rain, snow, waterfall
spray or similar precipitation; if the precipitation already provides
concealment, the miss chances stack.*

All findings are read-only metadata inspection of the installed game
assemblies through the lab's existing IL dump
(`C:/Dev/KingmakerGunslingerLab/private/charvis-native-il/Assembly-CSharp.il`);
member names, types and behavior summaries only.

## Factual current-system map

### 1. Weather state

- **Gameplay-facing state:** `Kingmaker.Player.WeatherData` (field
  `Player.Weather`) with `CurrentWeather : InclemencyType`,
  `NextWeatherChange : TimeSpan` (the serialized schedule in the player save)
  and the computed property `ActualWeather : InclemencyType`.
- `Kingmaker.Controllers.InclemencyType` = `Clear(0) / Light(1) / Moderate(2)
  / Heavy(3) / Storm(4)`.
- **`ActualWeather` is a live projection of the visual system**, not a
  separate simulation: it reads `Kingmaker.Visual.WeatherSystem.
  WeatherSystemBehaviour.Instance` — `WeatherType` (`Normal / Rain / Snow`)
  plus the current `RainIntensity`/`SnowIntensity` — and buckets the
  intensity against the thresholds in `BlueprintRoot.WeatherSettings`
  (`WeatherRoot.RainIntensities` / `SnowIntensities`). If the behaviour is
  absent or `WeatherType == Normal`, it reports `Clear`. A
  `Kingmaker.Controllers.WeatherController` ticks transitions.
- **Change notification exists:** the PubSub event bus raises
  `IWeatherChangeHandler.OnWeatherChange()` (raised from the weather
  controller), and native consumers already use it
  (`UnitLogic.Parts.UnitPartPartyWeatherBuff`, `SoundState.OnWeatherChange`).
  No polling is required.

### 2. One system or several?

- **Rain/snow (precipitation):** one system — the visual weather system,
  projected to gameplay as above.
- **Fog:** there is **no fog weather state** (`WeatherType` has no Fog). Fog
  gameplay in Kingmaker comes exclusively from unit facts: spells such as
  Obscuring Mist apply concealment entries with
  `ConcealmentDescriptor.Fog`. The visual `FogSettings` on the weather
  behaviour is presentation only.
- **Waterfall spray:** no engine contract at all — zero case-insensitive
  matches for waterfall in the assembly metadata; any spray is per-scene
  particle VFX with no state.

### 3. Native weather gameplay effects (the two consumers)

- **Concealment fallback** (inside `UnitPartConcealment.Calculate`): when a
  target otherwise has **no** concealment from entries, and
  `Player.Weather.ActualWeather >= WeatherRoot.ConcealmentBeginsOn`, and the
  attack weapon is **ranged**, the target is treated as **`Concealment.Partial`**.
  It is a fallback only: it never stacks with, and never appears alongside,
  fact-based concealment.
- **Weather-conditional buffs:** `Kingmaker.UnitLogic.FactLogic.
  AddBuffInBadWeather { Buff, Weather : InclemencyType, WhenCalmer : bool }`,
  reconciled by `UnitPartPartyWeatherBuff` (applies/removes `m_LastBuff` on
  each weather change and party transitions). This is the native precedent
  for "while the weather is X, the unit has buff Y" and is fully save-stable.

### 4. Concealment and miss chance mechanics

- Concealment is a **three-tier enum**: `Concealment.None(0) / Partial(1) /
  Total(2)`. `RuleConcealmentCheck.OnTrigger` maps Partial → **20** and
  Total → **50**, rolls one d100, and the attack misses when
  `roll <= value`. **There is no 10% tier, and the 20/50 values are hardcoded
  in the rule.**
- Fact-granted concealment entries live in
  `Kingmaker.UnitLogic.Parts.UnitPartConcealment` as
  `ConcealmentEntry { Descriptor, Concealment, RangeType?, DistanceGreater?,
  OnlyForAttacks }` added by the native component
  `Kingmaker.UnitLogic.FactLogic.AddConcealment`. Multiple entries combine by
  **`UnitPartConcealment.Max`** — the engine never sums miss chances.
  `ConcealmentDescriptor` values include `InitiatorIsBlind`,
  `TargetIsInvisible`, `Blur`, `Fog`, `Displacement`; Blur/Displacement are
  suppressed by `UnitCondition.TrueSeeing` (0x12). Blindness forces Total;
  `UnitCondition.PartialConcealmentOnAttacks` (0x22) forces Partial.
- `RuleAttackRoll` consumes `RuleConcealmentCheck` and exposes
  `IgnoreConcealment` (used by effects such as the mod's Seeking — see
  `src/KingmakerGunslinger/Enchantments/SeekingConcealmentPatch.cs`, an
  already-qualified narrow Harmony patch on exactly this pipeline).

### 5. Indoor/outdoor

- `Kingmaker.Blueprints.Area.BlueprintAreaPart.IsIndoor` exists (a per-area
  part flag; `LocalMapArea.IsIndoor(Vector3)` resolves a point through it).
- The native weather **concealment fallback does not consult it**; indoor
  protection is only implicit — indoor scenes presumably lack an active
  weather behaviour, making `ActualWeather` read `Clear`. **This is inferred
  from the getter's null/Normal handling, not runtime-verified**; a guarded
  runtime observation would be required before relying on it.

### 6. What this means for Whiteout specifically

| Tabletop clause | Native representability |
|---|---|
| 10% miss chance | **Not representable** as a concealment tier (hardcoded 20/50). |
| In nonmagical rain/snow | Representable and event-driven (`ActualWeather >=` threshold; Rain/Snow distinguishable). |
| Nonmagical fog | No fog weather exists; all Fog-descriptor concealment in game is magical. Clause needs an owner decision. |
| Waterfall spray | No engine contract; unrepresentable. |
| Miss chances stack with weather concealment | **Opposite of native**: entries combine by Max, and the native weather concealment is a no-other-concealment fallback. Summing requires a separate roll outside `UnitPartConcealment`. |
| Attacks against the Undine (melee and ranged) | Native weather concealment is ranged-only; melee coverage needs a custom path. |

## Feasibility conclusion

**FEASIBLE-WITH-DOCUMENTED-ADAPTATION.** Everything needed to *gate* the
effect exists natively and safely (event-driven weather state, the
`AddBuffInBadWeather`/`UnitPartPartyWeatherBuff` reconcile pattern, exact
attack-pipeline patch precedent in this repository). The *effect itself*
cannot be expressed with native concealment components and requires one
narrow documented adaptation.

## Minimal proposed implementation architecture (not implemented)

1. **Trait fact** registered save-stable and offered through the Favored
   Class `racial_traits` publication gated by `PrerequisiteRace` on
   `KMG.ElementalRaces.Undine.Race` (same offering mechanism as the
   Fiery Glare / Stoic Dignity findings in
   `CHARACTER-RACE-TRAITS-FEASIBILITY.md`).
2. **Weather-gated hidden buff** using the native reconcile pattern: a small
   component implementing `IWeatherChangeHandler` (or a fact mirroring
   `AddBuffInBadWeather`'s contract) that applies the hidden buff exactly
   while `ActualWeather >= Light` (threshold is an owner decision) and
   removes it on calm/weather change, party transitions and area unload.
   Event-driven — no polling, no stale buff (the native part's
   remove-on-change semantics are the stale-buff answer).
3. **Separate 10% roll** via one narrow Harmony patch on the attack pipeline
   adjacent to the existing qualified `SeekingConcealmentPatch` (same file
   family, same exact-attack guards): when the attack's target holds the
   weather buff and the attack is not already missed by concealment, roll
   one independent d100 ≤ 10 → miss. A separate roll is the only way to make
   the tabletop "stacking" clause true (Max-combination would swallow it).
   Interaction rule to match tabletop: an attack that ignores concealment
   miss chances (e.g. Seeking) also ignores this roll, and Total
   concealment's auto-miss ordering must be preserved.

## Rejected alternative (and why)

**Pure-component `AddConcealment` on the weather-gated buff** (no patch):
rejected as inaccurate on four axes at once — the smallest grantable tier is
Partial = 20% (double the trait's benefit), the Fog descriptor would
mislabel weather as magical fog, Max-combination means the benefit
disappears exactly when other concealment exists (inverting the stacking
clause), and no component path covers melee attacks. It would be the
cheapest implementation and the least faithful.

## Test matrix (for a future implementation slice)

- Domain: weather-threshold policy table (Clear/Light/Moderate/Heavy/Storm ×
  buff present/absent); gate excludes `WeatherType.Normal`; the 10% roll
  predicate (independent d100, miss on ≤ 10); stacking coexistence with a
  Partial Fog entry (both rolls occur); Seeking/IgnoreConcealment bypass;
  no effect for non-trait units; deterministic repeated initialization.
- Guarded runtime (deterministic, disposable fixtures only): observation
  that `IWeatherChangeHandler` fires on controlled weather change; the
  inferred "indoor scenes report Clear" behavior; exact attack-roll evidence
  for the separate miss roll. Any scenario must be readable without UI
  automation and must not write saves.

## Unresolved player-facing design decisions for Howie

1. **Fidelity vs patch surface:** exact 10% + true stacking (one narrow
   patch, as recommended) vs 20% Partial non-stacking pure components (zero
   patch, roughly double benefit that vanishes under other concealment).
2. **"Nonmagical fog":** no fog weather exists; recommend excluding
   Fog-descriptor (magical) effects entirely, but the owner may prefer
   treating magical fog as precipitation-like for feel.
3. **Melee coverage:** tabletop protects against all attacks; the native
   weather convention is ranged-only. Recommend all attacks (separate-roll
   architecture makes this free).
4. **Activation threshold:** any precipitation (Light+) or Moderate+?
5. **Indoor behavior:** rely on the inferred Clear-indoors projection, or
   additionally gate on `BlueprintAreaPart.IsIndoor` for robustness (needs
   the runtime observation above first).

## Explicit no-change confirmation

No production source, blueprint, localization entry, setting, patch,
concealment rule, weather system behavior, save or asset was changed by this
slice. `git diff` for this slice contains this document only.
