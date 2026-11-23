# Sprint 6 checkpoint C2a (VER-02 F1): a permitted save keeps an ongoing summon hold

Status: **C2a R1 accepted by manual review (2026-11-22) for its demonstrated development scope**, at head `4729014dba`
(product `044355c2a9`, package `47ec71d0…06fe`, DLL `3746ff5a…a6b2`). No further C2a repair or native cycle is requested.
Section 9 records the acceptance and the last classification corrections. **VER-02 is not closed.** Nothing is merged or
released.

- The first C2a review accepted:
  - the ordinary G1 route;
  - the baseline save-time release;
  - immediate save preservation on candidate `21f46b2009` for G1 and constructed G2;
  - the complete written relationships and their intentional reset.
- C2a stayed pending on two points: the lifetime of the new unload state, and active-carrier removal. Section 8 (R1)
  answers both with product change `044355c2a9`, and **section 8 governs** where it differs from sections 2–6.

- Branch `codex/s6-ver02-c2a-save-suspension` is stacked on C2's acceptance-records head `fbb773db04`
  (`codex/s6-ver02-c2-multilink-cleanup`, draft PR #205), then #204, #203, #200 and #198. The stack is not rebased;
  master is at v0.0.194.
- C1 and C2 are accepted for their tested subsets (C record, section 8; C2 record, section 9). A1, A2 and B stay
  accepted. RISK-03 keeps its retained limitations. REP-05 and REP-06 stay BLOCKED on their original inputs.
- Intended behavior (owner, 2026-11-22): a permitted save should preserve an ongoing hold in the running session, and a
  later load keeps ACTIVE_SUMMON_GRAPPLES_RESET_SAFELY_ON_RELOAD (release safely, never reconstruct).

## 1. Ordinary reachability (unchanged candidate `4e11e6c25c`)

**Source investigation** (decompiled 2.1.7b):

- **Save admission.** `SaveManager.IsSaveAllowed` refuses while `Player.IsInCombat`, during a game over, a dialog, a
  cutscene, a global-map encounter, or a dual-companion switch. `Game.SaveGame` itself refuses when saving is not
  allowed.
- **Combat state.** `Player.IsInCombat` is true while any controllable character's group is in combat.
  `UnitCombatLeaveController` takes a group out of combat 3 s after no conscious enemy in combat remains engaged with it.
- **Save lifecycle.** `SaveManager.SaveRoutine` calls `LoadedAreaState.TurnOff()` (then `PreSave`) and
  `CrossSceneState.TurnOff()` before serializing, and `TurnOn()` afterwards. `UnitDescriptor.TurnOff` sets
  `IsTurnedOn = false` before it turns the unit's facts off.
- **The route.** A summon's own AI grabs an enemy, then a party caster Dominates the held enemy. Native `ChangeFaction`
  (`ToCaster`) moves it to the caster's faction and group. No enemy remains, so the party leaves combat while the hold
  continues, and saving becomes permitted.

**Native reproduction** (`20261122T214832Z`, launch 1, harness `46ad4fd9…0aee`):

1. The main character summoned the crocodile through the product's execution ability. Its brain was left as summoned
   (active).
2. A native hostile, `CR7_BanditFighterRangedLevel8` (`02459ebf…`, faction Bandits, Medium), was spawned
   **unmodified**. Its eligibility for Dominate Person (`AbilityData.CanTarget`) and for the grab (product size rule)
   was read from the engine and the product.
3. The party joined combat through the engine's controllers. The crocodile's **own AI** bite (a `UnitAttack` command,
   brain active) established the hold. Native admission at that point: in combat, **save refused**.
4. The main character cast **native Dominate Person** on the held enemy through `UnitUseAbility` (Success). The enemy
   became faction Player, in the main character's group. The party left combat. Native admission: not in combat,
   **save allowed**, with the hold active (initiator and target parts, hold and grappled buffs, Entangled and CantMove,
   one raw record).
5. `Game.SaveGame` completed (paused, game clock unchanged). One `SummonHoldComponent.OnTurnOff` ran during the save.
   In the continuing session afterwards:
   - the target's part, grappled buff and conditions were gone, and the raw record was empty;
   - the holder kept its native initiator part, the hold buff, and CantAct/CantMove;
   - the target took no damage.

   **Release reproduced.**

**Disclosed conveniences:**

- crocodile BAB 100;
- hostile HP 100000 and brain inactive (so it neither kills the main character nor flees; the engine still perceives
  it as an enemy, and the party joined combat);
- Dominate Person granted to the main character at caster level 20 and DC 100, cast through the ordinary command;
- units woken.

No faction was altered by the harness. The faction change is the native spell's own. No combat state, admission check,
record or buff was forced.

**G2 reproduction under the same save probe** (`20261122T215826Z`, harness `493e63e2…5746`): the C2 constructed
Flytrap fixture. Every one of three native saves released both links (one multi-hold `OnTurnOff` per save; held states
and records gone; the hold buff kept). This is a constructed fixture; G2's ordinary reachability is inferred from the
same save mechanism, not separately driven.

## 2. Repair (product commit `21f46b2009`)

**Owners.** The single-link `SummonHoldComponent` and the multi-link `SummonMultiHoldComponent`. Each released its
hold from `OnTurnOff`.

**Change.**

- **`OnTurnOff`** releases only when `ExpandedSummoningSpecialProfiles.ShouldReleaseHoldOnTurnOff(ownerTurnedOn,
  areaUnloading)` holds:
  - The owner is still turned on: a genuine end (buff removal, the death purge, expiry, suppression).
  - Or an area unload is in progress: its established release is kept.
  - The save case is the owner turned off with no area unload. It **keeps** the hold.
- **`OnFactDeactivate`** (new) always releases. This covers genuine removal and disposal, including destruction, where
  the owner is turned off before its facts are deactivated. Release stays idempotent; on genuine removal it runs once
  through `OnTurnOff` and again as a no-op.
- **`SummonGrappleAreaSafeguard`** (the existing global subscriber) now implements `ISceneHandler`:
  - `OnAreaBeginUnloading` sets `SummonHoldSuspension.AreaUnloading`. The engine raises it in
    `Game.HandleAreaBeginUnloading`, before `UnloadAreaCoroutine` turns the area off and stashes it.
  - `OnAreaDidLoad` and `OnAreaLoadingComplete` clear it.
- **Unchanged:** release paths (`Release`, `ReleaseLink`), maintain, limb records, the held side
  (`SummonHeldComponent`), the load reset (`ResetLoadedGrapples`), the leave sweep, and G3/G4 owners.
- No new save schema, persisted field or restoration feature was added, and holds are not reconstructed on load.
  Existing fields (the native grapple parts, the hold and held-state buffs, and the limb-record list) can now contain a
  complete relationship in a written save, because a save no longer releases it. There is no save interception and no
  broad turn-off suppression.

**Lifecycle rationale.**

- **Save:** units are turned off, written and turned back on. Nothing is deactivated, so ownership, limbs, records,
  buffs and round counters pass through untouched.
- **Area unload:** the flag is set first, so the release happens at the turn-off step, before `StashAreaState`
  serializes. This preserves the previous unload semantics (inferred from source; actual transitions are G5, blocked).
- **Pre-exit autosave** (`AutoSaveMode.BeforeExit`): it runs before `UnloadAreaCoroutine`, so it now preserves, and the
  unload then releases (inferred, unexecuted).
- **Flag failure mode:** if an unload never reaches `OnAreaDidLoad`/`OnAreaLoadingComplete`, the flag stays set, and a
  save then releases as before. (Corrected in section 8: this global latch could remain set and affect later saves; it
  was replaced in R1. The earlier "no stuck state" wording was unsupported.) (Source reasoning; not
  failure-injected). The flag is area-unload scoped, not save scoped.
- **Leads reviewed:** `LillendFascination32` gates on `Owner.IsTurnedOn` plus `OnFactDeactivate`, which also suspends
  across area unloads; that was rejected here to keep area semantics. `ElementalBloodRuntime` has an empty
  `OnTurnOff`.

## 3. Focused validation and candidate

- **Gate:** `scripts/Invoke-KmgGate.ps1 -Filter 'grapple;c2a-hold;expanded-summoning.sprint4'`. Production compilation
  14.3 s; 10 of 3178 selected domain tests passed, 0 failures (17.0 s in total). New case:
  `expanded-summoning.c2a-hold-save-suspension` (the four policy combinations).
- **Candidate:** `scripts/Build-Local.ps1 -DevelopmentPurpose ver02-c2a-save-suspension -DevelopmentTestFilter
  'grapple;c2a-hold;expanded-summoning.sprint4' -ReferenceBundleDir <private bundle> -LoaderReferenceDir <loader refs>`,
  run on a detached checkout of `21f46b2009`. Wall time 22:00:52Z–22:08:29Z (about 457 s); 10 of 3178 selected tests.
  - Package `KingmakerGunslinger-0.0.193-development-ver02-c2a-save-suspension-21f46b200936.zip`, SHA-256
    `f4a3b1fca851ccb3440ace4f18d9f5ddc4517388c8c6f0414eccdcf012acb846`.
  - DLL `c717efac6e209932eabae57dc5f250fd96234548d25ef92d688857b6bced1ab2`.
  - Built once and reused for every candidate session.

## 4. Native results (candidate `21f46b2009`)

**G1, ordinary route** (`20261122T221344Z`, PASS 2 of 2, harness `addb5dc3…906a`, with read-only `MaintainLink` and
release traces).

- The same sequence as section 1 reached admission: refused at the hold, allowed after Dominate, hold active.
- **Three native saves** (asserted after each): hold intact (initiator and target parts, hold and grappled buffs, one
  raw record), no target damage during the save, game clock unchanged. One suspension `OnTurnOff` per save, with no
  release.
- **Next round** (captured trace): `MaintainLink` → `death-roll … heldKept=True`, damage dealt. The hold was still
  intact after the round (asserted).
- **Genuine terminal:** the holder's death. The native controller removed the target part
  (`UnitGrappleController.TickOnUnit` → `UnitPartGrappleTarget.OnRemove`); target part, grappled buff, CantMove and
  Entangled all gone (asserted).
- **Written save** (`Manual_1_ver02c2ag1.zks`, SHA-256 `e441c7c9…73a6`, inspected read-only after the session):
  - holder: `UnitPartGrappleInitiator` (target id, hold buff `5af4e99c…`), populated
    `_links ["60f537dc…|1|-1|BiteDragonLarge1d8|0|0"]`, CantMove and CantAct counters;
  - target: `UnitPartGrappleTarget` (initiator id, grappled buff `e49fc099…`), Prone and CantMove counters.

  A **complete single-link relationship** was serialized.
- **Fresh-process load** (asserted): at load and after 13.0 s of game time, no part, buff, grapple condition or record
  on either unit, and target damage delta 0. The native CanMove/CanAct flags are recorded only, because the native
  Dominate state lowers them (see section 6).

**G2, constructed Flytrap fixture** (`20261122T221729Z`, PASS 2 of 2, harness `addb5dc3…906a`).

- **Three native saves** (asserted after each): both links kept (exact holder context and saved caster id, one held
  state each, one raw record per limb, occupied limbs, Entangled and CantMove), raw records unchanged, target damage
  and hold/held round counts unchanged, no duplicated held state.
- The unrelated buff was kept.
- **A holder round followed** (asserted witnessed). Both links were still held.
- **Sibling terminal:** one target's death ended its link while the other stayed exact (asserted).
- **Accelerated expiry:** the remaining link was released and the holder destroyed (asserted).
- **Written save** (`Manual_1_ver02c2ag2.zks`, SHA-256 `337388a9…590d`):
  - holder: multi-hold buff `a6ff9b2a…`, populated `_links` with both records;
  - both targets: held-state buff `c6a4dd2e…` (`MultiHeld`).

  A **complete two-link relationship** was serialized.
- **Fresh-process load** (asserted): clean at load and after at least 12 s of game time (gate). Captured: 39.9 s,
  4393 buff-collection ticks per unit, zero hold/held round callbacks, damage delta 0.

**Regression of the changed terminal paths on the candidate.**

| Session | Harness | Result | Note |
|---|---|---|---|
| `20261122T222129Z` C1 terminals | `33530ca8…f4bd72` (the accepted C1 harness) | **PASS** 2 of 2 (81 s, 81 s) | Holder death, target death, second terminal, expiry, concurrent pair, reload |
| `20261122T222433Z` C2 multi-link terminals | `3b9d4498…bc41` (the accepted C2 harness) | Launch 1 **FAIL** (143 s); launch 2 PASS (113 s) | **Retained.** After the sibling case's target death, the remaining sibling's link ended during the following round: the hold buff dropped with that last link, and the move control then walked it 2.93 m. That harness recorded no maintain outcomes, so the cause is **not established**. It is consistent with an ordinary maintain failure: the product's `SummonManeuverChecks` makes a natural 1 always fail, about 5% per link per round, and the release takes the `ReleaseLink(…, dropHoldWhenLast: true)` path. The concurrent pair's control in the same run stayed at 0.0 m |
| `20261122T223033Z` C2 multi-link terminals | `160d5758…d715` (C2a harness, read-only `MaintainLink`/`ReleaseLink` trace added; assertions unchanged) | **PASS** 2 of 2 (143 s, 115 s) | 20 traced maintains, all `maintained`; 12 `ReleaseLink` calls at the terminals and controls. The save kept both links live in the continuing session (captured, and one round later) |

## 5. Retained failures and sessions

| Session | Product | Harness | Result | Classification |
|---|---|---|---|---|
| `20261122T214832Z` G1 reach | baseline `2608e9f5…` | `46ad4fd9…0aee` | Launch 1 FAIL (95 s); launch 2 FAIL (84 s) | Launch 1: **expected reproduction** of F1. Launch 2: **observer over-assertion**. The reset check required native CanMove/CanAct, which the native Dominate state lowers with no grapple condition present. The grapple-owned state was clean, and the check was narrowed afterwards. Save `895be348…51b2` held holder residue only |
| `20261122T215826Z` G2 save | baseline | `493e63e2…5746` | Launch 1 FAIL (80 s); launch 2 blocked (no written save) | **Expected reproduction** (three saves, each released both links) |
| `20261122T220852Z` G1 reach | candidate `f4a3b1fc…` | `493e63e2…5746` | Launch 1 FAIL (106 s); launch 2 PASS (88 s) | All three saves **kept** the hold (asserted). The hold then ended in the next round with no maintain damage; cause not instrumented, consistent with an ordinary maintain natural 1 or a break-free. The holder-death check also over-asserted CanMove (Dominate) |
| `20261122T221344Z` G1 reach | candidate | `addb5dc3…906a` | **PASS** (105 s, 86 s) | Trace added |
| `20261122T221729Z` G2 save | candidate | `addb5dc3…906a` | **PASS** (101 s, 113 s) | — |
| `20261122T222129Z` C1 | candidate | `33530ca8…f4bd72` | **PASS** (81 s, 81 s) | Regression |
| `20261122T222433Z` C2 | candidate | `3b9d4498…bc41` | Launch 1 FAIL; launch 2 PASS | Retained (section 4) |
| `20261122T223033Z` C2 | candidate | `160d5758…d715` | **PASS** (143 s, 115 s) | Regression with trace |

- Environment: loader profile `owner-0.33.0`, product alone; fixture `KMCS_AFTER_NORMAL_012_MODONLY.zks`
  (`0be64b3f…a249`), RTWP.
- Runtime window: the Mounted Combat peer session had ended; no peer session and no Kingmaker process were present when
  checked before the first launch and before the build.
- Every transaction completed, with game settings applied and owner files verified.
- Evidence is under `C:\Dev\KingmakerGunslingerLab\runtime-evidence\loader-compat\` and is not committed.

## 6. Limitations and open items

- **Ordinary reachability** was driven for G1 only (crocodile, Dominate Person). G2's preservation was qualified in the
  constructed Flytrap fixture; its ordinary reachability is inferred from the same mechanism.
- Other routes to a permitted save during a hold were not surveyed beyond this one: other charm/faction effects, or
  combat ending through loss of perception.
- **Area-unload ordering, pre-exit autosave behavior and the flag's failure mode are source inferences.** An actual
  area transition (G5) remains blocked on a fixture with an unscripted exit.
- **Native CanMove/CanAct are not grapple-owned in the G1 route.** The dominated target's flags were false with no
  grapple condition, so the G1 assertions use the grapple-owned parts, buffs and conditions.
- **Intermittent next-round releases** (`220852Z`, `222433Z`) are retained with their cause not established. The
  instrumented reruns showed every traced maintain succeeding. A test that needs a hold to survive several rounds is
  exposed to the product's natural-1 maintain rule; this is a fixture limitation, not counted as qualification.
- **Unchanged and still tracked separately:** F2/F3 accepted residue; next-round release timing for G2 (RTWP only);
  the Xill, Giant Scorpion and Giant Crab configurations (shared handler only); G3 swallow/engulf; G4 stirge; G1 variant
  owners (Impale, live constrict and riders, swallow promotion); G5.

**VER-02: open.** C2a repairs and qualifies F1 for the implicated G1 and G2 hold owners within the scope above.

## 8. R1: unload-state lifetime, active-carrier removal and native cadence (2026-11-22)

**Starting point.**

- Reviewed head `f897d14dd5`, base `fbb773db04`.
- Tested product `21f46b2009` (package `f4a3b1fc…b846`, DLL `c717efac…1ab2`).

**Verified engine contract** (decompiled 2.1.7b):

- Unload entries all raise `ISceneHandler.OnAreaBeginUnloading` through `Game.HandleAreaBeginUnloading`:
  - `SceneLoader.UnloadAreaCoroutine`;
  - the same-area `Game.LoadArea` branch;
  - `Game.ReloadAreaMechanic`.
- Each continues to `Game.OnAreaLoaded`, which raises `OnAreaDidLoad`; `ReloadAreaMechanic` raises it in its
  completion callback.
- `LoadingProcess` runs every step in a try/catch. On any exception it clears the queue and throws
  `LoadGameException`, and `Runner.ReportException` calls `Game.ResetToMainMenu` (the smoke-test mode only stops).
- So an interrupted unload or load does not resume savable play. Play resumes only through a fresh load, which
  deserializes new facts.
- Fact components start listening to events on `Fact.TurnOn` and stop on `TurnOff`.
- These are source facts. No interrupted lifecycle was injected natively.

**Repair** (product `044355c2a9`):

- The process-global `SummonHoldSuspension.AreaUnloading` latch, and the area safeguard's scene hooks, are removed.
- Each hold buff's own component (`SummonHoldComponent`, `SummonMultiHoldComponent`) now implements `ISceneHandler`:
  - it sets its own non-serialized `m_AreaUnloading` in `OnAreaBeginUnloading`;
  - it clears it in its own `OnTurnOn` (which runs after every save's `TurnOn`) and in `OnAreaDidLoad`.
- `OnTurnOff` uses that instance's state.
- **Why it cannot improperly affect later saves:**
  - the state belongs to one buff instance and is never serialized;
  - a new relationship gets a new instance, which starts false;
  - the old instances are disposed with the unloaded area or the old game.
  - Even in a hypothetical path that set it without clearing it, it would affect at most that one hold's next
    suspension, after which the save's `TurnOn` clears it. By the contract above, no such path resumes savable play.
- The comments are corrected as well:
  - the link store and load-reset comments describe the intentional reset, not a serializer inability;
  - the multi-hold class comment lists its four consumers and the save exception.
- **Focused gate:** `-Filter 'grapple;c2a-hold;expanded-summoning.sprint4'`. Production compilation 16.4 s; 10 of
  3178 tests passed, 0 failures.
- **Candidate** (built once from a detached `044355c2a9`, 23:01:58Z onward):
  - package `KingmakerGunslinger-0.0.193-development-ver02-c2a-r1-unload-state-044355c2a98b.zip`, SHA-256
    `47ec71d05b64c263ba5bad9e04c331944f7d95b33cc43f01a2f7c6022ff806fe`;
  - DLL `3746ff5ac7cbadd2b37258a9a886ce2488ae9dd3fef286f60e4de36d0e9ba6b2`.

**Harness** `d9201475…b543` (`artifacts/loader-compat-harness-ver02-c2a-r1`):

- `HoldLifecycleProbes.cs` is a process-wide read-only observer, installed by the launch condition
  `observeHoldLifecycle` and kept across a same-process load. It records, in order:
  - `Game.HandleAreaBeginUnloading` and `Game.OnAreaLoaded`;
  - each hold buff's `OnAreaBeginUnloading`, `OnAreaDidLoad`, `OnTurnOn`, `OnTurnOff` and `OnFactDeactivate`, with the
    owner's `IsTurnedOn` and the instance's own `m_AreaUnloading` at that moment;
  - `MaintainLink` results, `Release`/`ReleaseLink`, and `ResetLoadedGrapples`.
- The R1 probe `kmgHoldLifecycle` has three modes: `removal`, `cadence` and `afterLoad`.
- The C2a save probes now also assert: the exact G1 records and ownership before and after each save, an unchanged
  clock during paused saves, and the unrelated-effect control.

### 8.1 Lifecycle session `20261122T231045Z-owner-0.33.0-ver02-c2a-r1-lifecycle` (PASS, 105 s, R1 candidate)

**Active-carrier removal** (asserted). The cleanup path and timing are from the observer.

- **G1** (crocodile, holder alive). `BuffCollection.RemoveFact` on the holder's actual hold-buff instance ran, in order:
  1. `OnTurnOff` (owner on, unload state false);
  2. `SummonHoldComponent.Release(target)`, which dropped the record and the target part;
  3. `OnFactDeactivate`, a no-op.

  Within one game second the native controller removed the orphaned initiator part, so CantAct/CantMove and the
  records were gone. The target's grapple part, grappled buff and grapple conditions were gone. The holder stayed
  alive, the target's unrelated Bull's Strength was kept, and the independent control pair was intact.
- **G2** (Flytrap, two links). `RemoveFact` on the multi-hold buff ran `OnTurnOff` (owner on), then
  `ReleaseLink(target 0)` and `ReleaseLink(target 1)`, then `OnFactDeactivate`. Both links ended in the same frame:
  held states and records gone, no live answer. The holder stayed alive; the unrelated effect and the control pair were
  kept.
- **Disposal attribution**, from the G1 accelerated expiry: `OnTurnOff` with the owner on, then `Release`, then
  `OnFactDeactivate`.
- **Observed in every genuine case:** the release came from `OnTurnOff` with the owner on. `OnFactDeactivate` was
  delivered afterwards and released nothing further. A release from `OnFactDeactivate` alone (owner off at disposal)
  was **not** observed.

**Native cadence** (asserted; native `Buff.RoundNumber`, `NextTickTime` and `TickTime` read by reflection, distinct
from the observer's callback counts).

- Before the saves, both aged holds were at `RoundNumber` 1:
  - G1 hold buff next tick `432458660000`;
  - G2 multi-hold buff next tick `432459300000`;
  - tick time 6 s for both.
- **Two paused native saves.** For each:
  - unchanged game clock;
  - unchanged buff instance, `RoundNumber`, next tick, tick time, and caster and saved-caster ids;
  - exact raw records, limb occupancy, held-buff state and target damage unchanged;
  - the unrelated effect kept;
  - observed callbacks: `OnTurnOff` (owner off, unload state false) then `OnTurnOn`, with no release.
- **Next scheduled round:** `RoundNumber` 2, and each next tick moved to exactly the pre-save schedule plus 6 s
  (`432518660000`, `432519300000`). Neither restarted nor duplicated. Traced maintains: G1 death-roll (hold kept);
  G2 maintained ×2. Both holds intact.

**Same-process unload and load.** The harness `saveGame` (label `ver02c2ar1`) was followed by `loadLastSave`. The
observer's order:

| Seq | Event |
|---|---|
| 37–40 | The harness save: each hold `OnTurnOff` (owner off, unload state false; kept), then `OnTurnOn` |
| 41 | `Game.HandleAreaBeginUnloading` |
| 42–43 | `OnAreaBeginUnloading` on both live hold instances (their unload state becomes true) |
| 44–45 | G1 old instance: `OnTurnOff` (owner on, unload state true; the holder's grapple parts were already gone), then `OnFactDeactivate`. No product release was needed |
| 46–49 | G2 old instance: `OnTurnOff` (owner on, unload state true), then `ReleaseLink` ×2, then `OnFactDeactivate` |
| 50–51 | The loaded instances: `OnTurnOn` (unload state false) |
| 52 | `Game.OnAreaLoaded` |
| 53–54 | `OnAreaDidLoad` on the loaded instances. The loaded G1 relationship was complete: initiator target set, and the target part named the holder |
| 56 | `ResetLoadedGrapples`, then `Release`/`ReleaseLink`, then hold `OnTurnOff` (owner on), then `OnFactDeactivate` for both. **Intentional reset** |

- **Scope of what was exercised.** Both holders are summons kept in cross-scene state. The old instances were turned
  off during disposal with the owner still turned on, so the release decision used the genuine-end branch. The
  owner-off-plus-unload-state branch (an area-state holder turned off by `UnloadAreaCoroutine`) did **not** occur on
  this route; it remains source-inferred.
- **After the load.** The gates and captures are classified in section 9.
  - Both loaded holders were found (**observed**; the harness skips a missing holder) and had no hold buff, initiator
    part or record. No unit carried held state. These residual checks are **asserted**.
  - Fresh G1 and G2 holds had their own unload state false (**asserted**). Native admission allowed saving
    (**captured**, not asserted).
  - A native save completed with the clock unchanged (**asserted**), and **kept** both relationships with exact records
    unchanged (**asserted**). At least two hold turn-offs occurred, none with unload state true (**asserted**). Owner off
    at those turn-offs is **captured**, not asserted.

  Stale unload state did not control the later save.
- **Written save** `Manual_1_ver02c2ar1.zks` (SHA-256 `a3847464…fc70`): complete G1 relationship (initiator part,
  hold buff, populated record) and G2 records for both links.

### 8.2 Affected save checks on the R1 candidate (harness `d9201475…b543`)

- **`20261122T231327Z` G1 ordinary route, PASS 2 of 2 (110 s, 87 s).**
  - Three saves; each asserted an unchanged clock, exact records and ownership, the hold kept, no damage, and the
    unrelated control kept. One suspension turn-off per save.
  - Next rounds: death-roll ×3, hold kept.
  - The holder's death released the target through the native controller.
  - The written save (`8bc4e7d6…e8e2`) held a complete relationship, and the fresh-process reload reset it.
- **`20261122T231658Z` G2 constructed, PASS 2 of 2 (105 s, 114 s).**
  - Three saves with an asserted unchanged clock, the unrelated effect kept, and exact links.
  - Round, sibling terminal and expiry as before.
  - The written save (`408ddb7e…8582`) held both records, and the reload reset them.

### 8.3 Current status, assertions versus observations

**Asserted** (fail the run):

- R1 lifecycle: removal outcomes; cadence `RoundNumber`, next-tick and identity equality.
- After the load:
  - `Game.HandleAreaBeginUnloading` and `Game.OnAreaLoaded` observed;
  - each named hold observed `OnAreaBeginUnloading` before arrival, and was turned off or deactivated during the unload;
  - no named hold was suspended at unload without its unload state;
  - residual-state checks on the loaded holders that were found, and on all units;
  - fresh-hold unload state false;
  - post-load save completion, clock and preservation.
- R1 save checks: as listed in 8.2.

**Captured observations:**

- callback sequences and counts, the owner and unload state at each callback, maintain results, and buff lists;
- the detailed arrival/reset sequence in the 8.1 table, including the `ResetLoadedGrapples` callback and its position
  (the reset itself is asserted only through the residual-state checks);
- the post-load `IsSaveAllowed()` value;
- owner-off at the post-load save's turn-offs;
- the presence of both loaded holders.

**Source inferences:**

- the interrupted-lifecycle contract;
- the area-state holder's owner-off unload branch;
- pre-exit autosave ordering;
- G2's ordinary reachability.

**Earlier sections keep their original scope:**

- `221344Z`/`221729Z` (candidate `21f46b2009`) asserted the save-time items listed there.
- The "round counts unchanged" in the G2 save case were **observer callback counts**, not native `Buff.RoundNumber`.
- Native round state was first asserted in R1.
- The untraced candidate releases `220852Z` and `222433Z` remain **FAIL, cause not established**. A natural-1 maintain
  is a plausible mechanism, not their established cause.

**Unchanged and still tracked separately:**

- VER-02 open;
- G5 real party-leave hook and pre-exit autosave (blocked);
- G3 and G4;
- G1 variant owners;
- the Xill, Giant Scorpion and Giant Crab configurations;
- F2/F3 accepted residue.

## 9. Review decision (2026-11-22)

**C2a R1 is accepted** for its demonstrated development scope, at head `4729014dba` (product `044355c2a9`, package
`47ec71d05b64c263ba5bad9e04c331944f7d95b33cc43f01a2f7c6022ff806fe`, DLL
`3746ff5ac7cbadd2b37258a9a886ce2488ae9dd3fef286f60e4de36d0e9ba6b2`).

- **Classification corrections** (this records-only commit; no rebuild or rerun):
  - The loaded holders' presence was observed; the harness skips a missing holder and checks residual state on those it
    finds.
  - The post-load `IsSaveAllowed()` value and owner-off at the post-load save were captured, not asserted.
  - The detailed arrival/reset sequence, including `ResetLoadedGrapples`, was captured, not fully required.
  - The observed values and the sequence table are unchanged.
- The obsolete comment above `SummonGrappleLinkRecord.Encode()` is corrected. Comment only; the qualified product
  remains `044355c2a9`.
- **Limits retained:**
  - The owner-off plus unload-state release branch was not exercised.
  - A release attributable solely to `OnFactDeactivate` was not observed.
  - The interruption contract and the pre-exit autosave ordering remain source inferences.
  - G5 remains open and fixture-blocked.
  - The two untraced candidate failures (`220852Z`, `222433Z`) remain FAIL with unknown cause.
