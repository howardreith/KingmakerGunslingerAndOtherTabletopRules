# Reconciled Sprint 16 hidden batch, 2026-10-05

**Sprint 16: NOT QUALIFIED. Dire: six placements withheld. Sprint 17: not started.**

This is new evidence from clean reconciled `7c1690e616a75eb8dc66625fe527dd8988b94f7d`, not evidence borrowed from either sibling. The approved push wrapper published that linear descendant of `390f9a39`; the local `3ebefe7` safety reference remains intact.

## Exact prelaunch and artifact

- Clean focused Expanded Summoning: 216/216. Full unfiltered suite: 2018/2018.
- Repository/static/icon validation; runtime preflight 480; orchestration 68; closed persistence and crowd request tests: PASS.
- Clean exact-reference Release, deterministic 312-member package and strict standalone validation: PASS.
- Version remains 0.0.141. No publication or placement changes.
- Source fingerprint: `4f1628136895c314fd5eab35c80d384d3e7e46d34008c0a2b21e64ce84e6b91c`.
- DLL: `6decdac277ae3b569321edd14ae00bfd6ea5451b4bda81fea281e9895597c383`; MVID `f4540871-74d5-48df-96bd-16148ac179ca`.
- ZIP: `d815ae11094e7fdaafd8b5bce9d7da44a547f824285629039b43a050f5e13eab`.

## Six independent fresh Steam processes

| Scenario | Outcome | Assertions PASS/total | PID | Evidence directory / result SHA-256 | Working writes |
| --- | --- | ---: | ---: | --- | ---: |
| working-save-smoke | PASS | 11/11 | 16304 | `20261005T1648525112220Z-working-save-smoke` / `798A442629EF78843D5D2F102B62091BD58A9396004C1C1A340490C7EA6CB0B9` | 0 |
| disposable-expanded-summoning-crocodilians | FAIL | 156/205 | 2928 | `20261005T1650522756513Z-disposable-expanded-summoning-crocodilians` / `886ACD7F84F74F824A349A5AC1C97C25F1D2CA67C312105DA6A5F6480190227E` | 0 |
| working-save-expanded-summoning-creature-review | FAIL | 18/20 | 29148 | `20261005T1657563947613Z-working-save-expanded-summoning-creature-review` / `0110A632B5E14190F54296D381684C09899793BBB358789134079F48DFF1D75A` | 0 |
| working-save-expanded-summoning-prepare | PASS | 9/9 | 17252 | `20261005T1703248694434Z-working-save-expanded-summoning-prepare` / `5E6DEA1C03F659A581FFA0CBC42102B060661B5BF9579C3A30125C4380247CC8` | 1 |
| working-save-expanded-summoning-verify-cleanup | FAIL | 6/9 | 21640 | `20261005T1704494679856Z-working-save-expanded-summoning-verify-cleanup` / `81EC6E05283B09FFD32BBC8A8E3CD932323966FF6EA16972D756511A0753C911` | 0 |
| working-save-expanded-summoning-verify-absent | FAIL | 4/5 | 26808 | `20261005T1706054490986Z-working-save-expanded-summoning-verify-absent` / `2179A5DEEAB6BFBF4B3F3ADFAB1FA14DB38B455289737FF47FD62CE13885C12E` | 0 |

Every process launched through Steam App ID 640820 and exited automatically. No arbitrary UI input, process kill, baseline access or raw save operation occurred. No unexpected save-writing API was observed. Prepare made exactly one authorized native working-descriptor save (two stashed areas); cleanup correctly refused its write after failed invariants. The working fixture therefore still contains five test summons until a passing closed cleanup stage writes their absence.

## Actual live snapshot and restoration

Lease `runtime-20261005T164822Z-b57e6a6e4a9b4cd08f1884f189cc82dd` preceded snapshot and one deployment. Actual live Info was 0.0.140, 254 files, DLL `00EF67570D30E4FF9C54A2AD2027393660436D5724BCEE2BB99E4C5718E966FD`. No historical installation was forced.

Exact snapshot: `C:/Dev/KingmakerGunslingerLab/runtime-backups/live-mod/20261005T1648375757849Z`. Before/backup/after tree fingerprint: `5C3E4AD499DC5116D2024D06260D11700087379937B96B7372C821628CB9BFBB`. Restoration VERIFIED, same 254 files, lease released. Deployment manifest: `C:/Dev/KingmakerGunslingerLab/runtime-evidence/deployments/20261005T1648454591598Z/deployment.json`.

## Failed invariants and narrow repairs for the next candidate

The main fixture reached all 18 RTWP/turn-based command/AI/maintain cells and its final UI, lifecycle, route and fallback pack: 156/205 assertions passed, 49 failed. All 19 Sprint timing/modifier checks, all nine icon bindings, printed skills/HP and targeted root census passed. Successful build and these partial results do not qualify the creature.

- Production dice: exact native NPC weapon entities retain donor size, producing Crocodile 1d6 and Dire 2d6/2d8 rather than their printed 1d8 and 3d6/4d8. Compose live creature body size with the native weapon-rule delta; preserve legitimate overrides and multiplier handling. Focused arithmetic cases cover baseline, enlargement/reduction, native shifts and bounds.
- Production invalid-target maintain: the fixture settles lethal damage via the native per-unit life controller, then a holder tick can deal again to the dead target. Reject dead/destroyed prey and destroyed/unconscious owners before the crocodilian check; release the exact link. Retain the one-damage-event assertion and add full event attribution.
- Production Dire prey death: native swallower handles source death only. Add exact Dire-only native unit-death/destroy notification cleanup for its own swallowed prey. Worm/flytrap paths and component data stay unchanged.
- Production load reset: the area safeguard swept only party members and only missing holders. Native serialized parts/buffs survived on the five nonparty summons. At loading complete, reset exact KMG-owned hold/swallow relations across loaded units, remove only the owning buffs/parts, and clear session records. No grapple reconstruction, persistence subsystem or unrelated effect removal.
- Production rollback material: the native material controller creates a driven clone during attachment; the post-swap fault restores the donor but dropped that clone reference. Capture and destroy exact crocodilian-owned clones before reverting; keep native/cache materials.
- Fixture command result: native UnitCommand.Tick always sets IsActed after OnAction, including Fail. Require ResultType.Fail, exactly one successful Sprint rule, the same cooldown object/deadline and no repeat cast; IsActed=false was not the native rejection contract.
- Fixture cadence: an 18-second wall cap ended manual/AI cells before their later stationary full attack. Allow a bounded 60-second native-turn observation without driving AI or altering speed/timing. Contact tolerance remains 0.25m.
- Fixture cleanup: use the established owned-summon disposal path and drain native destruction. Keep exact detachment and fresh-load absence checks.
- Unresolved evidence: original tail contacts, borderline Dire jaw contact, native character-sheet restoration substate, fallback visibility, and crowd captures during native fog-of-war dissolve. Add narrow structured diagnostics/wake-and-settle observation; no contact threshold, visibility or restoration assertion is waived.

The ignored local batch helper captured native FAIL status accurately but labeled script return as Clean because the generic launcher exits nonzero without throwing. Correct its local exit-code record for future batches; scenario results, failed counts, artifact identities and restoration evidence above are unaffected.

## Retry boundary

Run fresh focused/unfiltered/static/icon/clean Release/deterministic/strict and current request/preflight gates on the repaired tree. Commit/push the source-qualified candidate with runtime status explicitly pending, then qualify a fresh clean exact-head artifact. Stop if the remote advances externally. Dire stays withheld until every required gate passes. Sprints 17 and Phase 2C remain gated/unauthorized respectively.

Repair checkpoint source gates (2026-10-05T17:25:30.7002580Z through
17:27:40.3348053Z): 216 focused and 2018/2018 unfiltered PASS;
repository/static/icon PASS; 480 preflight and 68 orchestration PASS;
closed persistence/crowd request tests PASS; clean Release and deterministic
312-member strict standalone package PASS. The preliminary class-name filter
matched no registered test label; rerun with `expanded-summoning.crocodilian`
passed 10/10, then the correct full `expanded-summoning` filter passed 216/216.
This checkpoint is source-qualified only. A fresh clean committed build and
all applicable runtime gates remain mandatory before claiming qualification.
