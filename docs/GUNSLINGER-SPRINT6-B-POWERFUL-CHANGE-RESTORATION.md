# Sprint 6 checkpoint B: Powerful Change restoration with an unresolved caster (RISK-02)

Status: **defect demonstrated on the unchanged baseline, repaired, and natively qualified for the stated scope; returned
for manual review** (2026-11-22). Nothing is merged or released.

- Branch `codex/s6-risk02-powerful-change-restoration` is stacked on A2's acceptance-records head `e0a41fd237`
  (`codex/s6-risk03-brown-fur-persistence`, draft PR #200).
- A1 and A2 are accepted. RISK-03 is development remediation accepted for both owners, with the retained engine
  limitations in section 10 of the A2 record.
- REP-05 and REP-06 stay BLOCKED on their original inputs.
- Master has moved to v0.0.194 (`06abcc775e`) since the stack was based. The stack is not rebased.

## 1. Player-visible outcome

**Before.** Suppose a Brown-Fur Arcanist companion cast a Powerful Change spell on another character and was then
dismissed while the effect still lasted. The engine keeps the effect: in the tested case the main character kept
Bull's Strength with 598 s left, at +6. After the next save and load it was **+4**: the Powerful Change increase was
lost although the spell was still running. Removing that buff later also left its stale persisted record behind.

**Now.** The effect keeps its exact increase (+6) through the load, through repeated recalculation and through a
further save and load. Removing the buff forgets its record.

## 2. Trace and cause

- `MechanicsContext.MaybeCaster` resolves the saved caster `UnitReference` through `Game.Instance.State.Units[id]`, the
  loaded units. If that returns nothing, it falls back to the owner only when the caster and owner are the same unit.
  Decompiled 2.1.7b.
- `Player` lifecycles (decompiled):
  - `RemoveCompanion`: the unit goes to `RemoteCompanions` with `IsInGame = false` and stays in the cross-scene state.
    It stays resolvable.
  - `DetachPartyMember`: the unit is also kept.
  - `DismissCompanion` (custom companions, `CanRemoveFromParty`): calls `value.Destroy()`. The unit is gone, while
    buffs it cast on other units stay until they end.
- On load, a buff's `AddStatBonus` re-adds its modifier. `BrownFurModifierAdjustmentRuntime.TryRestorePersisted` then
  matched the owned record, but it returned early when `MaybeCaster` was null. `Forget`, on buff removal, had the same
  precondition.
- **Repair (`4e11e6c25c`).**
  - `TryRestorePersisted` and `Forget` key on the caster id the engine saved in the buff's own mechanics context
    (`MechanicsContext.m_CasterReference`, read-only). That id survives the caster's destruction, and it equals
    `MaybeCaster`'s id whenever the caster resolves.
  - It is taken from the buff's saved context, never from a persisted Powerful Change record, so a record cannot
    validate itself.
  - Restoration still requires an exact match on buff, spell, caster, score, original value, descriptor, carrier and
    lifetime (`EndTimeTicks`), against one owned active record.
  - New casts (`TryAdjust`, `Remember`, `PrepareOrdinaryRecast`) are unchanged and still need the live caster.
  - A2's store and policies are untouched: retained history stays inert, gameplay writes stay strict, and ambiguity
    grants nothing.

## 3. Native scenarios and dispositions

The fixture is disposable: `KMCS_AFTER_NORMAL_012_MODONLY.zks` (`0be64b3f…a249`), beside Call of the Wild only, with
UMM 0.33.0 (owner profile).

- **Casters.** Three removable companions created through the engine's persistent-party path:
  `Game.AddUnitToPersistentSate` with the default player blueprint, then `CanRemoveFromParty = true` and
  `Player.AddCompanion`. Each is a level-five CotW Arcanist with Powerful Change and Share Transmutation.
- **Targets.** The main character and a fourth companion. Both are party members, so their buffs load through the
  party/cross-scene route (`party.json`), not area state.
- **Casts.** All are real committed casts through `UnitUseAbility`.

| Case | Reachable? | Engine disposition | Result before (`20261122T184839Z`) | Result after (`20261122T190544Z`) |
|---|---|---|---|---|
| Present caster P: Bull's Strength armed on the companion target | yes | effect kept | +6 restored | +6 restored |
| Swapped-out caster R (`Player.RemoveCompanion`): Cat's Grace armed on the main character | yes | caster remote, still resolvable; effect kept | +6 restored (caster resolved) | +6 restored |
| Dismissed caster D (`Player.DismissCompanion`, unit destroyed): Bull's Strength armed on the main character | yes | effect kept (598 s left at load, +6 in session) | **+4: increase lost** (caster unresolved, `TryRestorePersisted` returned false) | **+6 restored** (saved caster id) |
| Caster in another area | party casters: not reachable as unresolved | party characters (active, remote, detached) live in the cross-scene state, so an area transition does not unload them | not executed | not executed |
| Unrelated effect: P's unarmed Cat's Grace on the companion | control | no record | +4 | +4 |

The "caster in another area" disposition rests on the decompiled `Player`/`Game` code and on the remote case above
(a caster out of the party and not in game, still resolvable). No area transition was executed. A non-party unit as a
Brown-Fur caster does not arise in normal play, because Brown-Fur is a player-built archetype.

## 4. Validation

### 4.1 Focused

`scripts/Invoke-KmgGate.ps1 -Filter 'brown-fur'`, with production compilation: **100 of 3,177 PASS**. The repair is in
engine-typed code (`MechanicsContext`, `Buff`) that the domain tests cannot load. Its evidence is native.

### 4.2 Candidate (built once)

- Command: `Build-Local.ps1 -DevelopmentPurpose risk02-powerful-change-restoration -DevelopmentTestFilter brown-fur
  -ReferenceBundleDir <summon-overhaul-baseline bundle> -LoaderReferenceDir <repo>/artifacts/references/loader`, at
  detached `4e11e6c25c`.
- 645 s. Strict standalone UMM package validation passed. 100 of 3,177 selected tests passed.
- Package `KingmakerGunslinger-0.0.193-development-risk02-powerful-change-restoration-4e11e6c25c81.zip`, SHA-256
  `2608e9f593494dfbdd801e39f20c599b7e88e658c0dd875fb3ab9a46c431374b`.
- DLL `d813f62c059946f2d8355c1ff134bc154cf41a0f21a55547734d70a319b02cc4`.
- The baseline is the accepted A2 candidate `caad210696` (DLL `b651c69b…87b8`), whose restoration path is unchanged.

### 4.3 Native sessions

All three use session `risk02-bf-restore`. In each, the transaction completed, the registry readback was verified
(`25f28ffd…7514`), the owner saves and installation were unchanged, and there was no host conflict. Each ran in a
no-Kingmaker window agreed with the peer session.

| Session | Product DLL | Harness DLL (commit) | Result |
|---|---|---|---|
| `20261122T183136Z` | baseline `b651c69b…` | `94abe0cd…` (`8465c53063`) | **FAIL**, fixture: the engine's `CustomCompanion` blueprint has no race until chargen, so the save failed to load (`PreloadClothes` NullReferenceException) |
| `20261122T184839Z` | baseline `b651c69b…` | `3c1ad44f1ca72838b422680a54e87456fbac3e58f92f0c01185f63565ebd87f0` (`66d5535a6e`) | **FAIL**, the baseline reproduction: dismissed caster's effect +4 after load; its record not forgotten on removal (2 records, expected 1) |
| `20261122T190544Z-owner-0.33.0-risk02-bf-restore` | repaired `d813f62c…` | same `3c1ad44f…` | **PASS** 3 of 3 (create 153 s, reload-mutate 150 s, final 127 s) |

**Accepted session.**

- Lease `runtime-20261122T190544Z-2eb34cdd7ef74937bf1d7e7d998cea0c`. Transaction `5d98f5d89d6a4f218ab8997ba9822713`.
- `Manual_1_bfr1.zks` `88d9ed06fdd892abfa68500cde9a717e0464f7640543eb356292e788dc8dc52e`.
- `Manual_2_bfr2.zks` `3c52155cbcf1a6c384d9fe12282d1c8a42df01e707d57b57490ac76acdf970b1`.
- `session-summary.json` `721d49c9c1fb4d7d03f04c0e65425ba28c4f56577d2be203b66ff6806d6ac95c`.
- The baseline and repaired sessions ran the same harness bytes, session and fixture. Only the product DLL differed.

**Asserted by the PASS.**

- Every real cast left exactly one modifier of the expected value from its spell (+6 armed, +4 unarmed).
- **After the lifecycles, in session.**
  - The remote caster is a remote companion, not in the party.
  - The dismissed caster is destroyed and unresolvable.
  - Both effects remain on the main character at +6, each with its saved caster id.
  - Record counts: main 2, companion 1.
- **Fresh load (reload).**
  - Exactly one completed native `PostLoad` on both targets' persistence parts.
  - main STR +6 from D, main DEX +6 from R, companion STR +6 from P, companion DEX +4. Each value is checked with its
    owning buff's saved caster id.
- **Repeated callbacks.** Three `Fact.Recalculate()` calls on every buff of both targets left every modifier's count
  and value unchanged, so no increase accumulates.
- **Removal.** Removing D's Bull's Strength on the main character removed the modifier, and the main character keeps
  exactly 1 record (R's).
- **Ordinary recast.** P's unarmed Bull's Strength recast on the companion produced +4, and the companion keeps 0
  records.
- **Final fresh load.** The mutated state is unchanged: main STR none, main DEX +6, companion STR +4, companion DEX +4.
  Record counts: main 1, companion 0.

**Observed, not separately gated.**

- `TryRestorePersisted` calls at load: for D's buff, `savedCasterId` present, `casterResolved` false, restored true.
  R's and P's casters resolved and were restored. P's unarmed Cat's Grace was not restored, because it has no record.
- Lifetime: at load the effects had about 598 s left of 600 s (saved expiry kept), and the recast buff had 600 s.
- The only exception in each launch is the native `BugReportCanvas.OnEnable` NullReferenceException.

**Inferred from source.**

- Lifetime is protected because restoration requires the record's `EndTimeTicks` to equal the buff's saved end time
  (unchanged A2 policy).
- The "caster in another area" disposition (section 3).

**Unexecuted.**

- Timer-driven expiry (removal was an explicit `Buff.Remove`).
- An actual area transition.
- Other spells and carriers.
- Story removals of companions.
- Downgrade.
- NEW-10 (Favored Class / Brown-Fur compatibility) stays separate; nothing in B touches it.

## 5. Commits

| Commit | Kind |
|---|---|
| `e0a41fd237` | A2 acceptance records (base of this stack) |
| `8465c53063`, `66d5535a6e` | harness and session |
| `4e11e6c25c` | product repair (**product-build commit**) |
| (this commit) | records |

## 6. Disposition

- **RISK-02:** the defect is demonstrated (dismissed companion caster, increase lost after load). It is repaired at its
  owning boundary, the restoration and forget keys, and natively qualified for the stated scope. Awaiting manual
  review.
- **Proposed next checkpoint after review:** C (VER-02).
