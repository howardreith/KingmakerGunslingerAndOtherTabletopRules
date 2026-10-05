# Sprint 16 fourth reconciled hidden batch, 2026-10-05

**Disposition: FAIL — Dire withheld, Sprint 17 not started.**

Clean candidate `3fca5aba1cdaaf6215fd5712409fc208855c79cf` passed 216 focused / 2018 unfiltered tests, repository/static/icon validation, 480 preflight, 68 orchestration, closed persistence/crowd request boundaries, clean exact-reference Release and deterministic strict 312-member standalone packaging. Exact clean prelaunch 19:00:07.6100632Z–19:02:00.7812426Z. Six fresh Steam App ID 640820 processes reused the frozen artifact. Previous results do not qualify this candidate.

| Identity | Value |
| --- | --- |
| sourceStateSha256 | `6f8c164da5c28496f95cec626513db3ad18f70557fc18851b0684422d3926dd5` |
| dllSha256 | `5ff8b9b0c6df08281e6012f37dc5b47b4f6b4af52bec43c2e2ebda7d893f5303` |
| dllMvid | `0cfdbe42-9fe4-443c-b605-7a6f901800c7` |
| packageSha256 | `a6cf8e1c9ddc39abc3cb00f50eaa79ceb5bac7a57e97aa7583807668e0f5a888` |
| version | `0.0.141` |

| Scenario | Result | PASS / total | Fresh PID / start UTC | Evidence directory / result SHA-256 | Native working writes |
| --- | --- | --- | --- | --- | --- |
| working-save-smoke | PASS | 11/11 | 38880 / 2026-10-05T19:02:52.1084576Z | `20261005T1902505326192Z-working-save-smoke` / `687DDA3736A44669829430A6B4F746760B4DA72DBF4A0E47EEE31D58ED07CB53` | 0 |
| disposable-expanded-summoning-crocodilians | FAIL | 124/153 | 11692 / 2026-10-05T19:04:07.7578029Z | `20261005T1904066883829Z-disposable-expanded-summoning-crocodilians` / `A9DD0157F22F00849C146E95552F6F2A7555070471F69184EAEFB0F5B45CFDB1` | 0 |
| working-save-expanded-summoning-creature-review | FAIL | 18/20 | 37416 / 2026-10-05T19:05:48.8294898Z | `20261005T1905473856746Z-working-save-expanded-summoning-creature-review` / `2EC109DEC4041E1FE7BE7DDB7A6EE01D4BBFD28AFAC43392E47DFC033EFC9163` | 0 |
| working-save-expanded-summoning-prepare | FAIL | 6/9 | 13132 / 2026-10-05T19:11:15.6400994Z | `20261005T1911145718581Z-working-save-expanded-summoning-prepare` / `8A5247564120F7D1A9BC7363BF76595754764EC10D0330428108D5A780B290BC` | 0 |
| working-save-expanded-summoning-verify-cleanup | FAIL | 5/9 | 18428 / 2026-10-05T19:12:30.0081246Z | `20261005T1912287651200Z-working-save-expanded-summoning-verify-cleanup` / `C9F94159D66873ADEE7B472186C6938526AB404601E7923F21AAC13D445583A7` | 0 |
| working-save-expanded-summoning-verify-absent | PASS | 5/5 | 23180 / 2026-10-05T19:13:42.8882725Z | `20261005T1913418639071Z-working-save-expanded-summoning-verify-absent` / `12FF79EF3830B5E9C37C85A4565A17911C8F9962FF39195831E97F1E02DA4B3F` | 0 |

Batch 2026-10-05T19:02:17.9337399Z–2026-10-05T19:14:52.4283842Z. All processes exited automatically, no kill/UI input. Prepare refused its write on failed visual checks; verify-cleanup correctly rejected missing prepared identities and refused its write. Fresh absence 5/5 proves the working save remains clean. No unexpected writing API; no raw save access.

## First failed invariants and narrow cause

- Production visual adapter: its exact ownership lookup searched the unit's direct components. `ExpandedSummoningSpecialBuilder.ConfigureGrabber` actually attaches `SummonCrocodilianWeaponStats` to the owned CombatTraits fact in `unit.AddFacts`. The guard therefore rejected every original attachment and correctly kept the native donor. The visual adapter's effect itself was never qualified. Repair only that exact facts-graph lookup; cache exact owner/weapon references and add positive runtime carrier assertions.
- Cascade: missing original attachments failed lifecycle resource and private publication-view assertions. Native CMD size calculation also lost the existing original-form correction, so the two live-profile checks failed even though bite/tail damage stayed correct. Keep the profile, resource, private-root and original-view assertions intact; restore proper attachment rather than changing their expected values.
- Manual fixture: the native summon-control check failed before any combat cell. The native `UnitEntityData.IsDirectlyControllable` predicate additionally restricts capital units to the main character or its pet. The disposable caster is neither. Request-local repair retains the native controllable summon flag and exact summoner link; only in a capital, set this disposable actor's Master reference to MainCharacter, restoring it before cleanup. Do not alter the player, party, area classification, native turn flags or production control behavior. Record matched rule count, native part flag, and capital master disposition; refuse any mismatch.
- Crowd completed 18/20 this time; the earlier pre-OnUpdate exit did not recur. Two original visual contracts failed with the donor fallback. No claim that the actual contact pose or intact-frame correction passed.
- Closed persistence correctly did not save an invalid visual candidate. Absence remains green; the historical third batch's passed persistence is retained but cannot qualify this new DLL.

## Actual installation restoration

Lease `runtime-20261005T190225Z-a3f589bd7ef1432e834ed4c3e8985768` preceded the actual live snapshot. Actual baseline 0.0.140 / 254 files; DLL `00EF67570D30E4FF9C54A2AD2027393660436D5724BCEE2BB99E4C5718E966FD`. Snapshot `C:/Dev/KingmakerGunslingerLab/runtime-backups/live-mod/20261005T1902387026669Z`. Before/snapshot/restored tree `5C3E4AD499DC5116D2024D06260D11700087379937B96B7372C821628CB9BFBB`; VERIFIED / 254 files, lease released. Deployment `C:/Dev/KingmakerGunslingerLab/runtime-evidence/deployments/20261005T1902450705308Z/deployment.json`. No historical baseline or permanent deployment was forced.

## Containing source repair and next boundary

The containing narrow facts-graph and disposable capital-control repair compiles with zero warnings/errors and passes focused 216 tests. All complete source/package/preflight gates are required, followed by a coherent pushed checkpoint and new clean-head gates. Then rerun all six guarded scenarios on one exact artifact. Dire remains withheld; no Sprint 17, art/icon/GUID/version change or assertion weakening.

`HumanReview: NOT_PERFORMED_NONBLOCKING`. The four accepted limitations/adaptation remain unchanged.

## Containing repair source gates

New 216 focused / 2018 unfiltered tests, repository/static/icon validation, 480 preflight, 68 orchestration, closed persistence/crowd request boundaries, clean exact-reference Release, deterministic strict 312-member standalone package PASS. Complete pre-commit pipeline 2026-10-05T19:19:05.6260885Z–2026-10-05T19:21:17.0735670Z. Zero-warning/error incremental compile. No runtime qualification is claimed; new clean-head gates and the full six-scenario batch remain mandatory.
