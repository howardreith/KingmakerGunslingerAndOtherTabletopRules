# AI recovery after a firearm misfire (POL-01 / S4 T3)

Status: **implemented on a review branch under the owner's decision B; native lanes pass on the current candidate.
Awaiting review. Not merged, not released.** Branch `codex/pol01-sprint4-policy`, stacked on draft PR #179 at its
accepted head `9b8a755ec8`, draft PR #180. Sprint 4 stays open.

## 1. The issue, as the epic states it

From `Gunslinger_Remediation_Epic_2026-11-15.docx`, Sprint 4 ("Resolve firearm rules and progression behavior: repair
demonstrated firearm regressions and settle the remaining adaptation questions without inventing balance rules"),
story S4 T3, "Settle AI recovery after a firearm misfire", inventory record POL-01 / GS-AUD-0013 ("Repaired AI
companion remains suppressed until a player attack order"):

> The order ledger clears suppression only for a deliberate player attack order. Repair, Quick Clear and rest do not
> grant a new attack order under the current contract.

The remedy asks for a demonstration (automatic AI against a manual-order control), a choice between explicit player
rearm and fresh AI decisions after a successful recovery, and the chosen transition in `FirearmAttackOrderLedger` and
its recovery call sites, "scoped to the exact actor and firearm and preserving stale-command epochs", preserving
deliberate stop, hold position, invalid-target and combat-end behaviour. Completion outcome: "Behavior decision then
targeted repair". The original finding is `planning/release-audit-0.0.182/FINDINGS.md`, GS-AUD-0013.

## 2. The owner decision and the approved contract

The owner selected **option B** on 2026-11-17: a successful recovery of the exact firearm to Normal permits fresh
ordinary AI decisions; a completed successful rest counts as recovery; the interrupted pre-break sequence stays
cancelled for ever. Reasoning recorded with the decision: tabletop Quick Clear removes the broken condition with no
separate rearm action, the Gunsmithing feat restores a broken firearm during a rest, so a hidden extra attack click
after a successful recovery is neither a tabletop rule nor good CRPG behaviour. The v0.0.128 wording ("Recovery alone
grants no new order") stays historical; the next release must say it is superseded.

Approved contract:

1. A committed misfire immediately cancels the current attack sequence.
2. Every command, order, reload continuation, callback, epoch-bound token or other work captured before the break stays
   stale and is refused for ever.
3. A successful Quick Clear, Repair Firearm or completed-rest maintenance releases only the misfire-generated
   suppression of the exact physical firearm, after that firearm has committed a transition to Normal.
4. Recovery itself creates or submits no attack order, chooses or keeps no target, resumes no old command, spends no
   ammunition, spends no grit beyond Quick Clear's own cost, consumes no extra action and fires no shot.
5. A later attack is a separately constructed post-recovery AI decision or a new player order.
6. A failed, rejected, cancelled or rolled-back recovery releases nothing.
7. Repairing another firearm releases nothing.
8. Reloading a firearm that stays Broken releases nothing.
9. A cancelled, interrupted, failed, encounter or skip-time rest releases nothing.
10. A firearm that stays Broken or Wrecked releases nothing.
11. Deliberate stop, hold position, invalid-target, combat-end and ordinary native AI restrictions stay effective.
12. The epoch is kept. No save data or schema changes.

## 3. What changed in gameplay

Before: after a misfire break, the firearm's own wielder could fire it again only on a new player attack order (or
after a save and reload); Quick Clear, Repair Firearm and a completed rest repaired the item but left that lock, so a
companion's own AI never fired the repaired gun.

Now: once the exact firearm's recovery to Normal has committed, its lock is gone. The native AI may decide a new attack
with it in its ordinary way; nothing is ordered for it. Everything else in the misfire contract is unchanged.

## 4. Design

**Ledger transition** (`Firing/FirearmAttackOrderLedger.cs`). `Recover(weapon, committedCondition)`: when the committed
condition is Normal, for every actor recorded against that exact item, clear `Suppressed` and `RequiresOrder`. It does
not touch the epoch, the actor's current order or its submission counter; it creates, accepts, cancels and submits
nothing. Any other condition releases nothing. It is idempotent: after release the item's record is removed, so a
second call finds nothing. A later `Break` records the actor again and suppresses as before.

**Provenance and cleanup.** The ledger is keyed by actor, then item, so the rest path, which repairs items wherever
they are, needs the item's lock holders. `Break` records the actor in a `ConditionalWeakTable` keyed by the physical
item, whose value is a list of `WeakReference`s to actors: the table does not keep the item alive and the list does not
keep actors alive. It is never serialized. Dead references are pruned whenever the list is read; the item's record is
removed on release and on an order's normal `Complete`. The holder at the time of the rest does not matter: the lock is
released for the actor that broke the gun, whether the gun is still wielded by it, in shared inventory or carried by
another participant.

**Call sites, after each rollback boundary** (`BrokenSequenceSuppressionRuntime.OnCommittedRecovery(weapon)` reads the
item's committed condition itself and fails closed, logging and keeping the lock, if anything throws):

- Quick Clear (`Deeds/QuickClearRuntime.cs`): after its `try`/`catch`, so after the state replacement, the combat-log
  line and diagnostics can no longer roll the item back.
- Repair Firearm (`Recovery/RepairTestMusketRuntime.cs`): when `FirearmRepairTransactionService` returned a verified
  `Repaired` result; the transaction rolls back and throws on any failure, so a failed or rolled-back repair never
  reaches the release.
- Completed rest (`Gunsmithing/CompletedRestMaintenancePatch.cs`): per item, after that item's `Transition` committed
  and its retained state was verified; a failure on one item stops before its release and does not affect the others.
  The rest boundary itself admits only a successful, non-encounter, non-skip-time completion.

The interruption policy's comment and the ledger's summary now state the release; the policy still takes no firearm
condition as input.

## 5. Evidence: three separate statuses

### 5a. Retained historical evidence (v0.0.156), under its own identities

`disposable-firearm-break-interruption` run `20261016T0411002291236Z`: PASS, 491 of 491; product 0.0.156
(`0.0.156-firearm-rtwp-kill-continuation`), commit `162087000707d3721179ef0d916aca1e5a5f3d1b`, module version id
`5027a1b0-71cb-4ed2-9981-13eaec3ce5b7`, loaded module SHA-256 `a5c4d6587af63102a4e1e1c96fdeca2d332913ad159f55e334aa2fca39b7770a`,
game 2.1.7b. It was a guarded run against the live installation (deployment `20261016T0411001838055Z`), not the
loader-compat copy. It shows the **superseded** contract: after a break the native AI's retries are refused; Quick Clear
alone (`RuntimeTestRunner.FirearmManualInput.cs`, lanes `rtwp-`/`tb-quick-clear-*`) and out-of-combat Repair Firearm
alone (`RuntimeTestRunner.FirearmAdditionalInput.cs`, variant `repair`, check `repair-native-repair-preserves-item-kit`)
leave the gun suppressed; a completed rest leaves it suppressed (`RuntimeTestRunner.FirearmRestCompatibility.cs`,
`native-rest-restores-natural-wrecked`); a new click fires (`*-quick-clear-new-attack-fires`,
`repair-new-order-fires-exact-target`).

Git blob identities of every production and observer file behind those conclusions, at the run's commit and at the
decision checkpoint `108c2e4b77` (identical in every case; the files changed by this cut differ from both since
`8a2b0dd6e4`):

| File | `162087000707` | `108c2e4b77` |
|---|---|---|
| `Firing/FirearmAttackOrderLedger.cs` | `774e8597eb39` | same |
| `Firing/BrokenSequenceSuppressionRuntime.cs` | `aebb353baebc` | same |
| `Firing/BrokenSequenceInterruptionPolicy.cs` | `2f92004b2091` | same |
| `Firing/NativeFirearmAttackOrder.cs` | `6becbc0d749d` | same |
| `Firing/NativeFirearmAttackOrderPatch.cs` | `64a159bb9f2e` | same |
| `Firing/EmptyFirearmAttackCommandPatch.cs` | `a31eddc090ae` | same |
| `Misfires/FirearmMisfireRuntime.cs` | `754c1a546b19` | same |
| `Deeds/QuickClearRuntime.cs` | `bd0b8da13fdb` | same |
| `Recovery/RepairTestMusketRuntime.cs` | `c272a58640ed` | same |
| `Gunsmithing/CompletedRestMaintenancePatch.cs` | `6a43b8aae4a3` | same |
| `RuntimeTesting/RuntimeTestRunner.FirearmMaintenance.cs` | `94df5973a4e0` | same |
| `RuntimeTesting/RuntimeTestRunner.FirearmManualInput.cs` | `fa8ec1e54d0a` | same |
| `RuntimeTesting/RuntimeTestRunner.FirearmAdditionalInput.cs` | `d38114358e7e` | same |
| `RuntimeTesting/RuntimeTestRunner.FirearmReviewKillChain.cs` | `292fbc7f95be` | same |
| `RuntimeTesting/RuntimeTestRunner.FirearmInputNegatives.cs` | `a92580f4c8c4` | same |

### 5b. Current source reading (at `5fcc3e9d3e`)

Sections 3 and 4 describe the source. Four checks of `disposable-firearm-break-interruption` that encoded the
superseded rule now expect the release: `*-quick-clear-alone-never-revives-old-order` (`FirearmManualInput`, whose idle
after Quick Clear now runs without the AI brain because the lane is the manual-order control),
`*-quick-clear-normal-nonzero-epoch` (`FirearmReviewKillChain`, the same idle change), `repair-native-repair-preserves-item-kit`
(`FirearmAdditionalInput`) and `native-rest-restores-natural-wrecked` (`FirearmRestCompatibility`). **These four were not
executed on current bytes** (see 5c).

### 5c. Execution on the POL-01 candidate

**Final candidate:** source and product commit `5fcc3e9d3ed945730a9b1a8079efba016cfc55b6`, package
`aec986ff8430debc68c50af959b008b886d2753fdf8807edc8f93b129ffb4065`, DLL
`9a531808e800f9f4975972dc7c23ef4c1d33883444adf8db6eef7f68bc35b7cf`, module version id
`d358e9c9-40ed-4519-90a5-1ea50422f56e`; loader-compat harness `593a26d438f8cd68e5c0e9c6b5b7a387c583fb0e0c723a81af82857da5db1077`
(unchanged since REP-02); game 2.1.7b in the `game-copies/loader-compat` copy, UMM 0.33.0, Harmony 2.3.6 and 1.2, the
candidate and the harness the only active mods, `locale=enGB`, no save loaded (the save-free fixture at the main menu,
with its save-mutation guard).

**Session `20261117T211826Z-owner-0.33.0-firearm-misfire-recovery`** (`scripts/compatibility/loader-compat/sessions/firearm-misfire-recovery.json`),
scenario `disposable-firearm-misfire-recovery` (`RuntimeTesting/RuntimeTestRunner.FirearmMisfireRecovery.cs`):
**PASS, 42 of 42 assertions**, 77 s launch. Staging and restoration receipt: the session reports the 95 owner saves
unchanged, the owner installation unchanged and the registry restored. Evidence files `runtime-result.json` (SHA-256
prefix `b3e2aecf872d`) and `firearm-native-input.json` (`65b437d73204`), kept on the lab machine.

Every lane starts from a player order whose shot misfires (forced natural 1): the pre-break order and command are
cancelled, the epoch goes from 0 to 1, the gun is Broken and empty, and through the AI's retry window there is no
further shot. A native attack command constructed before the break is kept to be run after the recovery.

| Lane | Recovery and its own effects | Stale work after recovery | What followed |
|---|---|---|---|
| RTWP, Quick Clear (standard) | Normal; epoch 1; 1 lock released; no current order; no submission; 0 shots, powder, balls, grit; action cost 5.5 | the pre-break command refused (binding epoch 0 against 1), the pre-break order neither current nor resumable, a captured attack's epoch refused | the native AI constructed a reload 0.25 game s later (`BlueprintAiAttack`, epoch 1, no order), then an attack on the enemy; one shot; 1 powder and 1 ball; standard cooldown 5.5; the AI's reload cost 2.5 |
| RTWP, deliberate stop after it | | | the AI's next pending reload was stopped through the native `TryCancelCommands`: finished, never acted, no open command, no shot in the next second |
| RTWP, out-of-combat Repair Firearm | the same, party out of combat (no action cost charged out of combat) | the same | after both units rejoined combat natively, the AI's reload 0.25 game s later, then its attack; one shot; 1 powder, 1 ball |
| RTWP, completed rest, gun in shared inventory | native rest succeeded and completed once, maintenance ran once; then as above | the same | after the gun went back to its wielder and combat rejoined, the AI's reload 2.0 game s later, then its attack; one shot; 1 powder, 1 ball |
| RTWP, completed rest, gun carried by another participant | the same; the new holder was never locked | the same | the same |
| RTWP, negative controls | Repair Firearm clicked in combat: not submitted, 0 recovery calls, still locked; manual reload: loaded and still Broken, 0 released, still locked, no shot; Quick Clear of another Broken firearm in the same hand: that gun Normal, 0 released, the original still locked | | then Quick Clear of the original gun: Normal, 1 released |
| RTWP, manual-order control | | | before any recovery a new player order fired the Broken gun (a new order, a new command; 1 powder) |
| Turn-based, Quick Clear | the same recovery effects | the same | the actor gained no AI command after recovery; on its next turn a new player order fired once, the gun Normal; no autonomous turn-based resumption is claimed |

**Failures kept on record**:

- Candidate `8a2b0dd6e441` (package `58344128a60e55c4a84f92cd10b30aab0b19acbdfe6d9a4a0f9fb698ff39aaca`, DLL
  `bb6a5d532e32b0bbda6bde819ba862156ab478d33ea6449e82269f4204b1d3ba`), session `20261117T205541Z`: launch 1 FAIL at
  `pol01-rtwp-quick-clear-recovery-commits-and-releases-only`, every recorded effect as required, an observer defect
  (a null order id stored as a string-typed JSON null was compared with `JTokenType.Null`). Launch 2,
  `disposable-firearm-break-interruption`, FAIL at `cr04-loaded-one-command-four-native-attacks` (one of four planned
  attacks missed), before any recovery lane.
- Matched baseline, the accepted REP-02 candidate `d72746b81c5d` (package `0a43cd8f3e3f…`), session `20261117T205923Z`:
  `disposable-firearm-break-interruption` FAIL at `cr04-final-kill-one-command-four-native-attacks`, the same kill-chain
  family, the failing case varying with the process's native rolls. So that scenario fails in this game copy without
  any POL-01 code; it passed on the live installation in October. It stops before its recovery assertions and was
  dropped from the POL-01 session.
- Candidate `efcc4735c0f2` (package `0443ca613a173fff74e5cfa29a87168012a84295d49583d98268f1a5245cc754`, DLL
  `af7a260c3bc27450ed51df0ed99c3cb263eaada7d5bb233cc089d0e8fa95aeb8`), session `20261117T210848Z`: FAIL at
  `pol01-rtwp-quick-clear-fresh-ai-decision-fires` after observing exactly the selected behaviour; an observer defect
  (the AI's first command appeared in the same Unity frame as the post-recovery snapshot, so a frame comparison could
  not order them; the final check uses the snapshot and the game clock).

## 6. Validation

| Check | Result | Time |
|---|---|---|
| `scripts/Invoke-KmgGate.ps1 -Filter 'firearm-recovery;firearm-orders;broken-sequence'` (and wider filters with rest-maintenance, field-repair, quick-clear, scenario catalog) | production and tests compiled; 38 of 38, 64 of 64 | 22 s, 23 s |
| Neighbouring domain tests (`rest-maintenance;field-repair;quick-clear;sprint29;firearm-maintenance;firearm-recovery;firearm-orders;broken-sequence`) | 66 of 66 | under 1 s |
| `scripts/Test-RuntimeScenarioPreflight.ps1` | passed | 22 s |
| `git diff --check` | clean | |
| Three `Build-Local.ps1 -DevelopmentPurpose firearm-misfire-recovery` candidates | each passed development-candidate validation (63 focused tests) | 439 s, 415 s, 406 s |
| Native sessions: `20261117T205541Z`, baseline `20261117T205923Z`, `20261117T210848Z`, `20261117T211826Z` | FAIL, FAIL, FAIL, PASS (see 5c) | 182 s, 102 s, 75 s, 90 s |

Domain tests (`tests/KingmakerGunslinger.DomainTests/FirearmMisfireRecoveryTests.cs`, twelve, through the production
ledger, `FirearmStateMachine`, `QuickClearService`, `FirearmRepairTransactionService` with in-memory ports,
`CompletedRestMaintenancePolicy` and `BrokenSequenceInterruptionPolicy`): break suppresses and advances the epoch;
recovery keeps it, creates no order and submits nothing; pre-break orders, continuations and captured attacks stay
refused; a new automatic construction is allowed; exact firearm scope (another gun of the same actor, another actor's
gun, an unrelated item); a rejected repair (no kit), a rolled-back repair (kit count moved) and Wrecked guns release
nothing; a reload of a Broken gun releases nothing; incomplete rests (cancelled, encounter, skip-time, no gunsmith, no
kit, item failure) release nothing; a completed rest releases the gun wherever it is and restores a Wrecked one; a
second misfire suppresses again; player orders and stops work before and after recovery; release is idempotent and a
collected actor is not retained. The tests model each runtime's glue (release after the committed condition); the glue
itself is shown by the native lanes.

Total validation time was about 30 minutes, at the ceiling, because two observer defects each needed a rebuild.

## 7. Limits

- **The four updated `disposable-firearm-break-interruption` checks were not executed on current bytes**: that scenario
  fails earlier in this game copy on the pre-POL-01 candidate too. Running it needs an environment where its
  kill-chain lanes pass (the live-installation route it used in October), or a narrower phase of it.
- **Hold position** (`UnitEntityData.HoldState`, which only forbids AI movement) was not exercised; only the native stop
  was. Invalid-target and combat-end behaviour are native and unchanged by this cut, not separately observed here.
- **Turn-based**: the party member made no autonomous decision in the exercised turn-based state; resumption there is
  by the player's order, as before.
- **Fixture, not a campaign**: a save-free fixture at the main menu, one actor and one enemy, the native AI brain and
  rest boundary driven by the fixture; not a loaded save with a real companion. Natural rolls were forced for the
  misfire (1) and the AI's shot (10).
- **Commands constructed while suppressed** (between the break and the recovery), such as an AI reload: by source
  reading they carry the post-break epoch and no order, so after recovery they are admitted as ordinary work; a reload
  neither fires nor targets by itself. Not observed natively.
- **One run of the final bytes.** The lanes ran once; a repeat was not needed (no intermittency or ambiguity seen).
- The next release must state that v0.0.128's "Recovery alone grants no new order" is superseded.
