# Weapon findability qualification: 0.0.143

Source baseline: `4ba8d4aca087391144abf401f526189f59b26535` (0.0.142). Feature branch: `codex/weapon-findability-fixes`. Seven requested moves and two additional installed-scene corrections preserve canonical item, economy, effect, crafting and module identities. The other 20 placements remain unchanged.

## Measured gates

The complete 29-weapon registry passed installed blueprint/source identity (29/29), ordered native treasure (29/29), and active persistent scene presence (29/29). Normal entrance routes passed 28/29; native pickup 28/29; unloading/revisit 28/29; fresh-process disk reload 28/29. Each result below requires its own evidence. Missing evidence remains UNVERIFIED.

| Weapon | Blueprint | Native treasure | Scene | Normal route | Pickup | Revisit | Disk reload |
|---|---|---|---|---|---|---|---|
| Duelist's Rebuttal | PASS | PASS | PASS | PASS | PASS | PASS | PASS |
| The River King's Measure | PASS | PASS | PASS | PASS | PASS | PASS | PASS |
| Irovetti's Ovation | PASS | PASS | PASS | PASS | PASS | PASS | PASS |
| The Last Word | PASS | PASS | PASS | PASS | PASS | PASS | PASS |
| Watch at the World's End | PASS | PASS | PASS | PASS | PASS | PASS | PASS |
| Paper Lantern | PASS | PASS | PASS | PASS | PASS | PASS | PASS |
| Wayfarer's Oath | PASS | PASS | PASS | PASS | PASS | PASS | PASS |
| Border Sentinel | PASS | PASS | PASS | PASS | PASS | PASS | PASS |
| Quiet Current | PASS | PASS | PASS | PASS | PASS | PASS | PASS |
| Winter Reed | PASS | PASS | PASS | PASS | PASS | PASS | PASS |
| Cloud-Cleaver | PASS | PASS | PASS | PASS | PASS | PASS | PASS |
| Falling Petal | PASS | PASS | PASS | PASS | PASS | PASS | PASS |
| Drawn Horizon | PASS | PASS | PASS | PASS | PASS | PASS | PASS |
| Storm Over Stone | PASS | PASS | PASS | PASS | PASS | PASS | PASS |
| Foxfire Whisper | PASS | PASS | PASS | PASS | PASS | PASS | PASS |
| Thunder at the Gate | PASS | PASS | PASS | PASS | PASS | PASS | PASS |
| Mountain-Sunder | PASS | PASS | PASS | PASS | PASS | PASS | PASS |
| Empty Sleeve | PASS | PASS | PASS | PASS | PASS | PASS | PASS |
| Moonlit Crossing | PASS | PASS | PASS | PASS | PASS | PASS | PASS |
| Unfixed Form | PASS | PASS | PASS | PASS | PASS | PASS | PASS |
| Night Without Moon | PASS | PASS | PASS | PASS | PASS | PASS | PASS |
| Heaven's Measure | PASS | PASS | PASS | PASS | PASS | PASS | PASS |
| World-Tree Severer | PASS | PASS | PASS | PASS | PASS | PASS | PASS |
| Boughkeeper | PASS | PASS | PASS | PASS | PASS | PASS | PASS |
| Thornstep | PASS | PASS | PASS | PASS | PASS | PASS | PASS |
| Moonlit Fork | PASS | PASS | PASS | PASS | PASS | PASS | PASS |
| Viper's Reach | PASS | PASS | PASS | PASS | PASS | PASS | PASS |
| Briar-Crowned Spear | PASS | PASS | PASS | PASS | PASS | PASS | PASS |
| Spear of the First Branch | PASS | PASS | PASS | UNVERIFIED | UNVERIFIED | UNVERIFIED | UNVERIFIED |

The [curated evidence](../validation/weapon-findability-runtime-qualification.json) records exact item/loot identities, area, mechanics scene, persistent UUID, hierarchy and position, phase, visibility and interaction observations, native route steps, pickup/depleted-source witnesses, and sampled DLL/package identities. [The player inventory](../planning/PROJECT-MAGIC-ITEM-ACQUISITION-INVENTORY.md) correlates these objects with recognizable native treasure and map/room descriptions. Exact room names remain unverified where the scene/reference correlation does not identify them.

Native comparison uses Blueprints2.1.4.zip, SHA-256 `6ddf33970fb9ed3c127dd8f30b6416d7d23f607303ca61a31aab1972ed43df60`. [The full ordered reference](../validation/weapon-findability-native-reference.json) retains Littletown's two separate pearl rows. Installed **2.1.7b** is the physical qualification authority.

These are explicitly authorized leveled disposable fixtures. Initial area loading uses the ordinary entrance; later movement, doors, skill checks, offered dialogue answers, fog, stairs and vines use native commands and conditions. Native loot-window slot transfer must acquire exactly one canonical weapon while preserving the original treasure. Native unloading/reconstruction and a new owned disk save followed by a separate process prove persistence. Native damage prepares nearby combat without changing immunity, immortality or interaction restrictions. This does not qualify combat balance or an organic campaign playthrough. Optional map discovery and organic quest/DLC reveal remain separately documented reference conditions.

## Source and sampled artifact identities

Repository validation, all **2,301 domain tests**, clean Release compilation, strict **293-member** package validation, the 2,660-entry blueprint manifest and production SoundBank validation passed for the sampled source build. The final source checkpoint passed **213 blueprint assertions**, **29 scene-presence assertions** and **11 working-save smoke assertions**. Game **2.1.7b**, Steam build **6757524**, native Assembly-CSharp MVID `07fa1e4d-8618-41b3-9b8d-faa17d3b26f7`. Mod informational version **0.0.143-weapon-findability-fixes**, assembly **0.0.143.0**.

Sampled build `2026-10-08T22:41:35.2621044Z`: embedded commit `4ba8d4aca087391144abf401f526189f59b26535`, source fingerprint `e159aeec7eb6138c48fd1816a4d432f856bf23aeca88f1474f79f1b4073656a0`, DLL SHA-256 `650554ce4441292b17e5bc0c1c20ee2b9d397515c8411cd30fe0c23187f40f21`, MVID `f4dbb96b-5503-47f3-9180-3f6684aa6db1`, ZIP SHA-256 `934e44163f8bc20b6d83e414047f27cecde21a7690562fd4dde68825c3720876`. These identify that sampled candidate, not every later archive. Documentation/commit packaging changes have separate final manifests. Exact identities for each physical and recovery run remain in the curated evidence; a green source suite does not promote any physical gate.

## House and the two scene corrections

Both +5 firearms passed ordinary first-floor walking, exactly-one pickup with native treasure preserved, revisit and disk reload before leaving the House. The measured Phase flag `db94ef898ad95944788d3da6b22a5e31` was **1**, with `HouseAtTheEdgeOfTime01_Mechanics / Loot_SideA` and native equipped lantern active. Mirror Memories and the Third Key objective remained **None** before and after pickup. The Last Word fixture naturally completed the LinziDead entrance-room cutscene; other companion outcomes were not individually exercised. Watch walked 65.85 metres; The Last Word walked 131.02 metres and opened an ordinary door. Literal `#1` names remain exact.

Heaven's Measure remains the second-floor Headband of Mental Perfection +6 cabinet in `HouseAtTheEdgeOfTime_2ndFloor02_Mechanics / Loot_SideB`, native Phase **2**. The measured route starts at the ordinary first-floor entrance in Phase 1, walks into G03SideA with the lantern off, observes its native Phase 1 to 2 change and FogWallSwitch01SideB teleport, activates the native lantern and uses the ordinary throne-room corridor and L2G stairs to the second floor. The corridor requires the native Horned Hunter key; the exact key was supplied only as a disposable story prerequisite, and organic Hunter progression was not exercised. Cabinet walking, native pickup, revisit and disk reload passed. Guide phase numbers are never substituted for native flags.

The cabinet is in the room behind Nyrissa's encounter room. After L2G stairs, the measured continuation crosses F01 with the lantern off (**2 to 1**), walks into native `GotAllKeys1`, observes the native `GotAllKeys` flag unlock, crosses F04 (**1 to 2**) and opens the Nyrissa-room door normally. That trigger requires the exact Wriggling Man, Knurly Witch and Third Nyrissa keys, supplied only as disposable story prerequisites. Neither the Third Key puzzle, organic key acquisition nor an organic Nyrissa encounter was exercised. The normal gate and phase behavior, cabinet pickup and persistence were measured; no gate flag or check outcome was forced.

Paper Lantern's retired `59cb0ac65b4093440ad341b9a2f372cf` and The River King's Measure's retired `b34367a637010f743815aed5875152bd` had **zero installed scene references**, including inactive objects. Their replacements have one exact persistent source each. [The correction record](../validation/weapon-findability-scene-corrections.json) records the exact comparison, UUID, native treasure and provenance. Both replacements passed native walking, pickup, revisit and reload. Paper Lantern's native movable-crate check used Approach interaction and actual Athletics result; it was not opened by invoking success actions. The palace replacement contains Wooden Spoon, Grinding Stone and 16 gold; the former unreferenced 632-gold table is historical. No similarly named table was silently substituted.

Briar-Crowned Spear retained Blakemoor's Hideout. The exterior native door, real Trickery 35 check, success-cue transfer to the hideout, interior walking, native chest lock, pickup, revisit and fresh-process reload passed. The optional Perception 45 discovery/Blutmond lead remains reference evidence; it was not organically played through. See [the player guide](../planning/PROJECT-MAGIC-ITEM-ACQUISITION-INVENTORY.md). No spear moved solely because of optional map discovery.

## Controlled recovery and merchant persistence

[Supported recovery instructions](WEAPON-FINDABILITY-RECOVERY.md) cover **28 historically relocated weapons**, selected one at a time. Native preparation passed **412 assertions** and wrote one new owned save; fresh-process verification passed **33 assertions** with zero writes. All 28 per-weapon ledger entries survived. Canonical ownership in inventory/equipment/stash/companion storage, loaded historical copies, missing progress/acknowledgement, one selected grant, repeated invocation and reload were exercised. An actual Craft Magic Items Winter Reed +3 clone refused from inventory, equipment and stash without inventory changes. Unknown historical ownership is explicitly acknowledged.

Fresh never-generated targets and an actual generated old-layout eastern bone pile were separate fixtures. With known item history, native Kingmaker reconciliation could add the new row without changing the original cloak; it did not respawn an extracted copy or refill unknown history. The missing-history inventory and cloak survived revisit and disk reload. **These definition moves do not automatically repair an old campaign.** The mod does not grant on load or refill containers. The ledger prevents repeated recovery grants in that saved state; it cannot prove that a copy was never sold, dropped or left in an unloaded area.

Roadwarden and Dead Reckoning remain merchant-only. The native Skeletal Salesman table supplied a RE_Trader fixture; native BeginTrading/AddForBuy/Deal debited measured prices, transferred one of each and depleted stock. Disk reload and reopened trade preserved ownership and depleted stock. Generic stock, optional Better Vendors progression and crafting restrictions remain unchanged. This qualifies native purchase/persistence, not organic merchant encounters.

Better Vendors **2.0.8** is installed, **Ready**, with expected hooks active. Its [acceptance workflow](BETTER-VENDORS-COMPATIBILITY.md) requires an authentic, nonfabricated kingdom-stage save; none is available. That optional acceptance is **NOT RUN**, separately from the passing base-mod native merchant fixture. The earlier 0.0.138 waiver does not qualify this mission.

## Protected saves and runtime isolation

Every real launch used Steam App **640820** and a guarded request. The actual campaign, `KMG_AUTOMATION_BASELINE`, load-only `KMG_AUTOMATION_WORKING` and all 95 preexisting saves were protected by deny-write handles and exact hash/metadata comparison. Temporary saves use unpredictable `KMG_WEAPONS_0143_*` names leased to one transaction; only owned descriptors can be written or removed. Live mod files/settings were backed up and restored per transaction. Raw artifacts, saves, packages, proprietary assemblies and machine-local configuration remain excluded from Git.

One user-authorized future fixture was retained: **KMG_WEAPONS_0143_20261008T1155313560103Z_791ccc275f0b4b96b2f1b424a2caca39** (Manual_310; SHA-256 `496720b2eabfde8bc92a65eeae4ed9d980399f027fd5953701b6602e1872681a`). It is a leveled House Phase-1 fixture after Watch pickup with a native lantern, not organic story progression. It is now part of the protected catalog.

## Remaining evidence

- **Spear of the First Branch**: the bounded native walk did not reach the next road segment after six interrupted-command retries. The two native vine interactions and earlier road segments passed. Route, pickup, revisit and disk reload remain **UNVERIFIED**. Exact partial observations are preserved in the curated evidence; no impossibility or physical PASS is inferred.

  Latest measured native step stopped at `[-38.2271919, -23.6851482, 36.40163]`, requested `[-42.6000023, -21.394001, 42.6000023]`, with navigation areas `{'actor': 2, 'requested': 3}` and goal distance `7.92405462` metres. Attempt: `20261008T2211359520947Z-d36f7bc6cfab453fa73d360c5eb14221`. This records the fixture's incomplete route; it does not identify the cause or establish that the game location is impossible.

  Direct native interaction and measured road waypoints both stopped short. The
  loaded scene census found the actual container; it did not establish the
  missing connection. No supported native entrance to that region was qualified
  from this disposable seed. Further qualification needs an established native
  route/progression prerequisite or an authentic final-chapter fixture; forcing
  a target-side teleport would not close this gate. The placement remains unchanged.
