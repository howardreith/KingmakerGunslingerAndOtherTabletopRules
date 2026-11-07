# Summoned Creature Manual Playtest Repair — R2 status

**R2 DEVELOPMENT CHECKPOINT / STOP.** LIFE-02 (repeated Frightened saves) is
repaired and natively observed on DATA. LIFE-01 (Acadamae summon readiness) is
**not** natively established: the ordinary-round comparison failed at observer
setup before any creature acted, and the surprise round is explained from the
installed source only. No product change was made for LIFE-01. This is not a
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

- A stationary 4-second window per step (about 240 native ticks). Longer
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

This is an observer defect in the new named-creature Acadamae preparation
(the SM IV KMG child prepared at its own level through the detached
canonical/selected construction), not a creature result. Original FAIL
evidence is kept: `runtime-evidence/20261107T1631221655425Z…`,
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

## Candidate

| | |
| --- | --- |
| Purpose | `summon-playtest-r2` (development candidate; never a release) |
| Source | `c370ec234b7070679a528b259207f3ea4ddb9b39` |
| Source fingerprint | `75768669fa8b65ceb9f13e5ae0dc5bdc0dddf141a99f2187ace2b906d6ceeb78` |
| Package SHA-256 | `df7601d9eb60c3deba8788d6133ccdc42750c7da380d55a8be2f65864eb6596f` |
| DLL SHA-256 / MVID | `d473fba74a4ef821c54d841b2e8f061f3e5d744a09ad05c462bc213674143198` / `4e0042e6-5fa9-48e5-bca7-96f05876794d` |
| Version metadata | `0.0.179` |
| Selected tests | 58 of 2,851 registered, 0 failures |

Observers in these bytes: `summon-playtest-r2-fear` (PASS) and the
`readinessCreature` Acadamae path (defective, above). A corrected readiness
observer needs its own rebuild and run.

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
3. Optional: cast an Acadamae summon (Pteranodon or a mephit vs Lion/Hell
   Hound) in an ordinary round and note whether it acts in that round; and
   separately, open combat with the summon and note that it first acts in
   round 1. These are the open LIFE-01 observations.
