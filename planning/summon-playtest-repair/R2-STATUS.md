# Summoned Creature Manual Playtest Repair — R2 status

**R2 FOLLOW-UP CHECKPOINT / STOP.** LIFE-02 (repeated Frightened saves) is
repaired and natively observed on DATA. LIFE-01 (Acadamae summon readiness) is
**partly** established: the original R2 cast failure is now explained (a
fixture issued the cast while the caster's native turn was still scrolling)
and fixed; with the corrected fixture, Acadamae Pteranodon and the Dog control
act in the round their cast resolved; Acadamae Lion attacked in its cast round
but its predicate failed on an observer assumption (see "R2 follow-up"). Fire
Mephit, Hell Hound, the nonaccelerated Pteranodon and the surprise round remain
unobserved. No production behavior was changed for LIFE-01. This is not a
release; nothing was installed permanently. R3 has not started.

Records: [MISSION](MISSION.md) section 5, [ISSUE-REGISTER](ISSUE-REGISTER.md)
(LIFE-01, LIFE-02), [R1-STATUS](R1-STATUS.md).

## Intake (DATA, 2026-11-07)

- Base: master `9d15ba18972894a93686cb08b2c9af9d3a5db7bf` (PR #111, R1, merged;
  same tree as its reviewed head `2f44dbf1ab`). Branch
  `codex/summon-playtest-r2-lifecycle`.
- Installed Kingmaker Gunslinger **v0.0.140**, DLL SHA-256
  `00ef67570d30e4ff9c54a2ad2027393660436d5724bcee2bb99e4c5718e966fd`.
- UMM enablement (installed `Params.xml`, not folder presence): all sixteen
  mods `Enabled="true"`, including CallOfTheWild, ProperFlanking2 and
  KingmakerGunslinger. All thirteen KMG feature modules on, including
  `expanded-summoning`, `acadamae-graduate` and
  `protection-from-alignment-control-immunity`.
- Reference bundle:
  `artifacts/summon-overhaul-baseline/reference-input/KingmakerGunslinger-private-build-references`
  (created 2026-11-05; `Assembly-CSharp`, `UniRx`, `UnityEngine.CoreModule`
  byte-identical to DATA's installed game), passed explicitly.
- `KMG_AUTOMATION_WORKING` present; `KMG_AUTOMATION_BASELINE` untouched.

## LIFE-02 — repeated Frightened saves: repaired

### Reproduced behavior and cause

The owner's E10-E12 show an allied, summoned Nalfeshnee (green ring) saving
repeatedly at DC 22 beside the summoned Ice Devil.

- The shared `BoneFearAura35` (Bone Devil 5 ft, Ice Devil 10 ft) is an
  `AbilityAreaEffectLogic`. Installed `AreaEffectEntityData` calls it on
  entry, every 5-foot move and **every tick** for every unit inside, with no
  relationship filter (`AffectEnemies` is not consulted by the area). The
  Sprint 35 contract keeps that native eligibility ("enemy-only rules changes
  and per-frame retries are not" allowed), so the exact source's memory buff on
  the victim is the only repeat gate (`ContextActionArchonMenace.HasSource`).
- The memory marker was added with `AddBuff(SourceMemory, effect, …)` where
  `effect` is the Frightened context. Native `MechanicsContext` inherits the
  parent's `SpellDescriptor`, so the marker carried
  `MindAffecting, Fear, Emotion, Frightened`.
- The Nalfeshnee's Unholy Aura (cloned native `UnholyAuraBuff`) carries
  `BuffDescriptorImmunity` MindAffecting with no fact check. Native
  `RuleApplyBuff` set `Immunity` and returned no buff, so no memory was kept
  and the next tick saved again. The Nalfeshnee's +21 Will made nearly every
  one of those saves a success, which is the reported stream.

### Change

`BoneFearAura35` now adds the marker with the native caster overload
`AddBuff(SourceMemory, source, duration)`: its context is built from the marker
blueprint alone (no descriptor) with the exact source as caster, which is what
`HasSource` compares. Admission and memory durations moved into the pure
`InfernalRules35.FearAuraAdmission` / `FearAuraMemorySeconds`. Unchanged:
native eligibility (allies remain eligible), the Frightened effect and its
DC/duration, the established 24-hour memory after a success and rolled-duration
memory after a failure, source-specific independence, Bone Devil behavior.

### Native observation (candidate `c370ec234b70`)

Phase `summon-playtest-r2-fear`, save-free Sprint32 static interior, run
`20261107T1628308465120Z-cf0345ff2b3c497bb62d8b5b26b11fd6`, 144 s incl.
restoration. **PASS 8/8.** Raw evidence local:
`runtime-evidence/20261107T1628308215123Z-disposable-expanded-summoning-sprint32-native-review/`
(`summon-playtest-r2-fear.json` SHA-256 `171d0715696f…`).

| Step | Observed |
| --- | --- |
| Identities | Ice Devil `KMG_Summoning_Unit_IceDevil` with live `KMG_Summoning_Infernal_IceDevil_FearArea` `33b3b474…` (10 ft); Frightened `…IceDevil_FearFrightened` `27f4315b…` (`MindAffecting, Fear, Emotion, Frightened`); memory `…IceDevil_FearImmunity` `e3514e49…` (no descriptor); Nalfeshnee with Unholy Aura `50012f40…` (`BuffDescriptorImmunity` MindAffecting, `CheckFact=false`); Ice Devil allied to Nalfeshnee and caster, enemy of the private-faction hostile |
| Ice Devil alone, 4 game s | Hostile (eligible enemy, Will lowered): 1 save, failed (roll 8 vs DC 22), Ice Devil Frightened applied for 18 s, memory kept |
| Pair, 4 game s | Allied Nalfeshnee: **1 save** (roll 20, passed), 24-hour memory kept and recognized by `HasSource`; hostile not re-evaluated |
| Exit and re-entry | Both left and re-entered the live area: 0 new saves (both memories still active) |
| Cause on these bytes | The pre-R2 construction offered to the Nalfeshnee: context `MindAffecting, Fear, Emotion, Frightened`, `RuleApplyBuff.Immunity=true`, not applied. R2 marker: descriptor `None`, kept |
| Bone Devil regression, 4 game s | Ice Devil area ended with its source. Bone Devil's own 5-ft area: hostile 1 save (failed, Bone Frightened 36 s), Nalfeshnee 1 save (passed, 24-hour memory) despite earlier Ice Devil exposure (distinct source) |

Domain tests: `summon-playtest-r2.fear-aura-stationary-once`,
`.fear-aura-source-specific`, `.fear-aura-memory-durations`.

### LIFE-02 limits

- A stationary window per step, confirmed from the receipt's own
  `windowGameSeconds`: 4.006 (Ice Devil alone), 4.005 (pair), 4.008 (Bone
  Devil). Longer
  combat cadence, moving units and turn-based mode were not observed.
- The established 24-hour successful-save memory is a pre-existing Sprint 35/40
  contract, kept unchanged. SOURCES R-06 notes the PF1 Fear universal rule does
  not itself supply a blanket 24-hour immunity; that existing choice is an open
  owner question, not an R2 change.
- Natively, a mind-affecting-immune creature that fails the save is still
  "Frightened"-refused by its immunity; it is re-evaluated once its rolled
  failure memory ends (seconds-to-rounds cadence, not per tick).
- The fixture hostile is a private-faction human; Ice/Bone Devil saves against
  other creature types were not separately observed.

## LIFE-01 — Acadamae summon readiness: not established

### Source findings

- Ordinary round: the existing `SummonSameTurnActivationPolicy/Runtime`
  removes only Owlcat's stale full-round appearance lock and 6-second grace for
  a correlated Standard/Swift invocation and holds the caster's turn until the
  summon is natively joined, ordered and initiative-prepared. Pteranodon, every
  mephit, Lion and Hell Hound are all KMG Summon Monster IV children on that
  same path; no creature-specific spawn, appearance or enrollment branch was
  found in source. A creature difference would have to come from the creature
  itself (size, reach, flight, brain), which needs native evidence.
- Surprise round (installed `TurnBased.Controllers.CombatController`): when a
  surprise round exists (round 0), any unit that rolls initiative without the
  `Surprising` flag is marked `Surprised`, and `ChooseNextUnit` skips it for
  that round. At combat start, existing summons inherit their caster's
  `Surprising` flag; a summon that **arrives during** round 0 is added later,
  rolls initiative, and is marked `Surprised`, so its first turn is round 1.
  That matches "sometimes waits" when a summon opens combat. Whether this is a
  defect (the combat-start inheritance suggests Owlcat intends summons to share
  their caster's status) or a legitimate surprise restriction is unresolved,
  and it was not natively observed. No change was made.

### Native attempt (failed at observer setup)

The existing `summon-same-turn-acadamae` / `-native-control` cases gained a
closed `readinessCreature` parameter (pteranodon, fire-mephit, lion,
hell-hound) and per-command execution/result records. One guarded lease on
the disposable `KMG_AUTOMATION_WORKING` save (loaded, never written), run
`runtime-20261107T163049Z-c27d89ecbf31485da6553fbc1b00b307`:

| Case | Result |
| --- | --- |
| Acadamae Pteranodon / Fire Mephit / Lion / Hell Hound | **FAIL ×4**, identical: `failureStage=acadamae-native-command-resolution`, "stage 3 did not complete". The Standard Acadamae command was accepted (Acadamae on, `CanTarget`/`CanStart` true, slot available) but no cast resolved; the caster's turn was forced through rounds 1-2. In-run fixture cleanup exact (130/130 units, party restored) |
| Native-control Pteranodon | Not run (runner stopped) |

Corrected certainty (R2 follow-up): these receipts establish only a pre-spawn
cast failure and no creature-readiness result. They did not establish the
detached canonical/selected construction as the cause; the follow-up found the
cause in the fixture's turn timing (below), which also failed the SM I Dog
control. Original FAIL evidence is kept unchanged: `runtime-evidence/20261107T1631221655425Z…`,
`…T1635418444025Z…`, `…T1639568197585Z…`, `…T1644115022001Z-summon-same-turn-acadamae/`.

After the Hell Hound case flushed its result, Kingmaker hung (not responding,
0 CPU) and the harness retained the lease ("do not restore, kill it or steal
this lease"). With the owner's explicit authorization the hung process (PID
2948) alone was terminated. `Complete-KingmakerRuntimeLease.ps1` refused the
runner's custom-purpose outer lease (generic leases only), so the lease was
recovered with the repository's own functions: exact lock owner verified,
original owner process gone, live mod restored through `Restore-Live-Mod.ps1`
under that lease, settings and saves verified, then `Exit-KmgRuntimeLease`
(record: `runtime-evidence/summon-playtest-r2/20261107-readiness-recovery.json`).

## R2 follow-up — LIFE-01 repair checkpoint

Authorized as one bounded additional checkpoint (15-minute target, 30-minute
additional ceiling). Base: PR #112 branch at `00ad9db6fa` (still open).

### Intake

DATA unchanged from R2 intake: Kingmaker Gunslinger v0.0.140, DLL
`00ef67570d30e4ff9c54a2ad2027393660436d5724bcee2bb99e4c5718e966fd`, live tree
`5C3E4AD4…CB9BFBB` (254 files), sixteen UMM mods enabled in `Params.xml`, 95
protected save files, same reference bundle, passed explicitly.

### Cause of the unresolved R2 cast (established)

Observer additions: native `IUnitCommandStart/Act/EndHandler` events for the
exact cast and summon commands, stage timestamps, and periodic snapshots of
the exact cast command.

First candidate `2715e59b007a` (observer only), Dog Acadamae control, run
`20261107T1750289500185Z-4ef0628666ba4afaa453872328cd563d` — **FAIL** (kept),
receipt `runtime-result.json` SHA-256 `25250142b533…`:

- The caster owned the turn at 4.18 s with native status `Scrolling`; the
  fixture issued the cast at 4.21 s.
- The exact cast command never started (`started=False`, `timeSinceStart=0`)
  and ended `Interrupt` within 2 s as the turn moved to `Preparing`.
- Installed `TurnController.Start` sets `Scrolling` while the camera pans,
  then `Prepare()` calls `InterruptCommands()`. A command queued during the
  scroll is interrupted by native turn preparation.
- Stage 3 waited on `IsStarted` and so reported the interrupted command as a
  240-second timeout. The same signature (pre-spawn, stage 3, caster turn
  forced onward) matches all four R2 failures.

The working-save fixture moved to the cast as soon as the caster owned the
turn; the save-free fixture already waited for `Preparing`/`Acting`. This is a
fixture defect, not a product defect: production Acadamae casting and the
activation runtime were not changed. Fixture fix (`9c13837dd4`): both fixtures
wait for the native `Preparing`/`Acting` state; stage 3 fails fast when the
command ends before it starts.

### Execution-timing observer correction

The R2 observer stored the round at `UnitCommands.Run` and combined it with an
eventual `IsActed`, so it could accept a command queued in the cast round that
acted later. The observer now stamps each exact command at its native start,
act and end events with the round current at that event, and the pure
`SummonActionTimingPolicy` picks each summon's first legal action. Focused
tests reject a command queued in the cast round but acting in the next round,
and a queued/started/interrupted command that never acts, and accept a missed
attack and the first act of a multi-attack command.

### Native observation (candidate `9c13837dd4ce`)

One guarded runner on the disposable `KMG_AUTOMATION_WORKING` save (loaded,
never written), stopping at the first non-PASS:

| Case | Run | Result |
| --- | --- | --- |
| Acadamae SM I Dog (control) | `20261107T1803470764064Z-f5ba561b…` | **PASS 11/11**. Cast Standard, acted round 1, one Acadamae save (roll 20, DC 16), slot spent. Dog: first legal action `UnitAttack` Standard acted round 1 = cast round (miss, result Success); next round normal; 12-second duration expired at the boundary |
| Acadamae SM IV Pteranodon | `20261107T1805183558592Z-38015fb8…` | **PASS 12/12**. Cast Standard, acted round 2; Pteranodon `UnitAttack` (BiteHuge2d6, hit) acted round 2 = cast round; one attack per later round; duration 120 s exact; save DC 19 roll 20 |
| Acadamae SM IV Lion | `20261107T1806562190222Z-876705b4…` | **FAIL 11/12** (kept). Every enrollment, opportunity, duration and Acadamae check passed. The cast-round action predicate failed: the Lion's first act boundary fired in round 2 (cast round) with command `Result=None`, which the observer's policy rejected. The same receipt records the Lion's bite (`BiteDragonLarge1d8`, hit) in round 2 on its own turn, then two claws per later round |

Receipts: `runtime-result.json` SHA-256 `8be6ebc91181…` (Dog), `ea1336b3e544…`
(Pteranodon), `bfd05c918582…` (Lion).

Classification of the Lion FAIL: observer defect, not a gameplay failure.
Installed `UnitAttack.OnAction` returns `None` after each attack of a
full/iterative attack except the last, and `Success` only when the sequence
completes. The policy required `Success` at the act boundary. The policy now
counts any act boundary that is not `Fail` or `Interrupt` (focused test added),
but that correction is **source only and has not executed natively**. The Lion
FAIL stands for candidate `9c13837dd4ce`; its cast-round bite is a direct
attack-rule observation, not a reinterpreted PASS.

### LIFE-01 status after the follow-up

- Ordinary round, Acadamae: Pteranodon (one of the two reported slow cases)
  acted in its cast round under matched conditions, as did the Dog control. No
  creature-specific delay was reproduced for Pteranodon. Lion attacked in its
  cast round (direct attack evidence; predicate FAIL as above).
- Not observed: Acadamae Fire Mephit, Acadamae Hell Hound, the nonaccelerated
  Pteranodon control, and the surprise round. The allowance did not fit
  another rebuild plus cases. The surprise-round source finding remains an
  unobserved lead.
- No production defect was demonstrated; no production change.

### Follow-up restoration

Both runners restored automatically under their own lease: live tree
`5C3E4AD499DC5116D2024D06260D11700087379937B96B7372C821628CB9BFBB` before and
after, v0.0.140 DLL `00ef6757…66fd`, settings restored exactly, 95 protected
saves byte/metadata identical, no hang, no recovery needed. Records:
`runtime-evidence/summon-playtest-r2/20261107T1749568956490Z-r2b-*` and
`…T1803141819688Z-r2b-*`.

### Follow-up validation spend (additional 15-minute target, 30-minute ceiling)

| Invocation | Seconds |
| --- | --- |
| Focused gate, refused at compilation | 10.7 |
| Focused gate (27/27) | 16.0 |
| Candidate build `2715e59b007a` | 379 |
| Runner 1 (Dog FAIL; incl. deploy/restore) | 291 |
| Focused gate (27/27) after fixture fix | 14.9 |
| Candidate build `9c13837dd4ce` | 393 |
| Runner 2 (Dog, Pteranodon, Lion; incl. deploy/restore) | 315 |
| Focused gate (23/23) after policy correction | 16.1 |
| **Follow-up increment** | **≈ 1,436 s (≈ 23.9 min)** |
| **Cumulative R2** (1,706 + 1,436) | **≈ 3,142 s (≈ 52.4 min)**, plus ≈ 8 s recovery after the R2 hang |

The increment passed its 15-minute target and stayed under its 30-minute
ceiling; no further launch fit after the Lion case.

## Candidate (original R2)

| | |
| --- | --- |
| Purpose | `summon-playtest-r2` (development candidate; never a release) |
| Source | `c370ec234b7070679a528b259207f3ea4ddb9b39` |
| Source fingerprint | `75768669fa8b65ceb9f13e5ae0dc5bdc0dddf141a99f2187ace2b906d6ceeb78` |
| Package SHA-256 | `df7601d9eb60c3deba8788d6133ccdc42750c7da380d55a8be2f65864eb6596f` |
| DLL SHA-256 / MVID | `d473fba74a4ef821c54d841b2e8f061f3e5d744a09ad05c462bc213674143198` / `4e0042e6-5fa9-48e5-bca7-96f05876794d` |
| Version metadata | `0.0.179` |
| Selected tests | 58 of 2,851 registered, 0 failures |

Observers in these bytes: `summon-playtest-r2-fear` (PASS; that execution
claim belongs to these bytes only) and the original `readinessCreature`
Acadamae path (fixture timing defect, above). Follow-up candidates:

| Source | Package SHA-256 | DLL SHA-256 / MVID | Observed |
| --- | --- | --- | --- |
| `2715e59b007aaf9d4f01b607e9913bbaaeccd089` | `c70245d7f12df280831abd3c5fc722718bd1613afcbe14bbd7105ac70943acb3` | `9fd3c97c153be089107f43865bc15772cd73c6150e69e260b7461b249f45ba03` / `345d9c18-8c71-41b1-b955-3287f4d6bdea` | Dog FAIL (cause found) |
| `9c13837dd4ceb19848799127dfbf92de10441d08` | `a4352bf96d7cc9925b2f43986cb3c973fba6c6a2cb5391fe615e71894763fe2b` | `6500e1ba93ca466da8150fdb1c270281ae905a2e90114ba148d9fcb401949f19` / `f734ae9f-f712-401a-81d4-bf7055ed6217` | Dog PASS, Pteranodon PASS, Lion FAIL |

The later policy correction (full-attack `None` at act) is in source only.

## Focused tests

Filter: `summon-playtest-r2;expanded-summoning.sprint32-native-admission;summon-same-turn;expanded-summoning.sprint35;expanded-summoning.sprint40;expanded-summoning.sprint9-flight-combat-fixture;expanded-summoning.sprint10-wasp-native-flight-combat;expanded-summoning.sprint10-stirge-native-attack-visual`
— 58 of 2,851, 0 failures (three new R2 tests, the R2 phase admission, the
18 existing same-turn tests, the Sprint 35/40 infernal tests, and the Sprint
9/10 pins on the activation parameter lists, which stay unchanged because the
new creatures use a separate `readinessCreature` key).

## Restoration

- Fear run: live tree `5C3E4AD499DC5116D2024D06260D11700087379937B96B7372C821628CB9BFBB`
  (254 files) restored, v0.0.140, settings restored, 95 protected saves
  byte/metadata identical.
- Readiness run (after the authorized recovery): live tree restored to the
  same hash from backup `runtime-backups/live-mod/20261107T1630491522018Z`,
  v0.0.140 DLL `00ef6757…66fd`, settings (changed by the hung game) restored
  exactly, 95 protected saves identical, lease released.
- `KMG_AUTOMATION_BASELINE` untouched; no save written.

## Validation spend (R2's own 15-minute target, 30-minute ceiling)

| Invocation | Seconds |
| --- | --- |
| Focused gate, refused at compilation | 14.9 |
| Focused gate (58/58) | 21.3 |
| Candidate build `c370ec234b70` | 423 |
| Fear run incl. restoration | 144 |
| Readiness runner (4 failed cases + hang) | 1,103 |
| **Total** | **≈ 1,706 s (≈ 28.4 min)** |
| Safe restoration/recovery after the hang (excluded from the target by MISSION §11.3) | ≈ 8 |

R2 exceeded its 15-minute target and stopped just inside the 30-minute
ceiling. The ceiling was not extended. No further R2 validation remains in this
allowance; LIFE-01 needs a separately authorized narrow repair.

## Owner retest checklist

1. Summon an Ice Devil and a Nalfeshnee side by side (turn-based): the
   Nalfeshnee should make one Frightened save, then no stream of saves.
2. An enemy walking into an Ice Devil's or Bone Devil's aura still saves and
   can be Frightened; stepping out and back in does not re-roll while it is
   remembered.
3. Acadamae Pteranodon in an ordinary turn-based round should act (attack or
   move) in the same round it appears. Optional: compare a mephit and Hell
   Hound, which were not observed; and separately, open combat with the summon
   and note whether it first acts in
   round 1. These are the open LIFE-01 observations.
