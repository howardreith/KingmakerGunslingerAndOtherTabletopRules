# Sprint 16 third reconciled hidden batch, 2026-10-05

**Disposition: FAIL — Dire remains withheld; Sprint 17 has not begun.**

Candidate `08df2611bfaf22f675ea13c31babf35f304de1d7` passed its own clean-head source/package/preflight gates. Six fresh Steam App ID 640820 processes reused one frozen installation. The candidate did not pass all required runtime gates.

Clean prelaunch: 216 focused / 2018 unfiltered PASS; repository/static/icon validation; 480 preflight; 68 orchestration; closed persistence/crowd request boundaries; clean exact-reference Release; deterministic strict 312-member standalone package. Prelaunch 18:13:29.4087355Z–18:15:42.9808306Z.

| Identity | Value |
| --- | --- |
| commit | `08df2611bfaf22f675ea13c31babf35f304de1d7` |
| sourceStateSha256 | `c4a67556d165ec81af1a6f8ea5ff6c2335cd2244d6298d33ea510f21ab85593d` |
| dllSha256 | `74626e9ad3f00777a81aa1a695d1403d410fadad9e4654ce53ed72e163beaea9` |
| dllMvid | `2f6acc19-ef36-4b73-bf77-45951ec1e75e` |
| packageSha256 | `300e91be429a5473be357f4026b9de9bab935f02f14925daa0b610be089653e7` |
| version | `0.0.141` |

| Scenario | Result | PASS / total | Fresh PID / start UTC | Evidence directory / result SHA-256 | Native working save writes |
| --- | --- | --- | --- | --- | --- |
| working-save-smoke | PASS | 11/11 | 11868 / 2026-10-05T18:17:17.0374965Z | `20261005T1817148029955Z-working-save-smoke` / `57B04E76DA3DBE9D9F0EDDBDCE69B056F41698782E34F2164A969521DB696829` | 0 |
| disposable-expanded-summoning-crocodilians | FAIL | 184/206 | 21372 / 2026-10-05T18:18:41.2137057Z | `20261005T1818392954492Z-disposable-expanded-summoning-crocodilians` / `AFDD78B0581E76150F3C0296C139F7D5E0179336A7F409F2E8BF5E83526F92B5` | 0 |
| working-save-expanded-summoning-creature-review | ERROR | no result | 37352 / 2026-10-05T18:25:43.5140728Z | `20261005T1825419057509Z-working-save-expanded-summoning-creature-review` / `none` | 0 |
| working-save-expanded-summoning-prepare | PASS | 9/9 | 35352 / 2026-10-05T18:25:59.0002404Z | `20261005T1825578973256Z-working-save-expanded-summoning-prepare` / `B7517C765419893EA78ED214A4E39B81F316B18FF30486CCA57D0E2928DAB272` | 1 |
| working-save-expanded-summoning-verify-cleanup | PASS | 9/9 | 38468 / 2026-10-05T18:27:11.9121684Z | `20261005T1827109442753Z-working-save-expanded-summoning-verify-cleanup` / `0F44E6FB7689251C9AEEE3A7F270DADE4802382BDED7FCB57EA54ECD2A5D5AB8` | 1 |
| working-save-expanded-summoning-verify-absent | PASS | 5/5 | 35104 / 2026-10-05T18:28:24.2002348Z | `20261005T1828231742230Z-working-save-expanded-summoning-verify-absent` / `1BCCF1249BB2C9B5A5ECA76C52B9D3A6BB8D84746CB90FB298D0074C23453FD2` | 0 |

Batch: 2026-10-05T18:16:40.4011815Z through 2026-10-05T18:29:31.9362396Z. Five completed scenarios requested and initiated automatic exit. The crowd process exited before any result/ready/menu/OnUpdate stage; the orchestration error is retained, with no unsupported crash diagnosis. No process was killed and no UI input was sent.

Prepare and cleanup each made exactly one authorized native KMG_AUTOMATION_WORKING write, each with two stashed areas. No unexpected writing API was observed. Fresh-process verify-absent proved no remaining KMG summons. No raw save archive was read, parsed, copied or mutated. Persistence 9/9 + 9/9 + 5/5 is now PASS on this artifact; it cannot qualify a later DLL.

## Exact restoration

Lease `runtime-20261005T181647Z-97e33a1a66194b549922c2017e57dfb6` preceded the actual live observation. Actual live installation: 0.0.140 / 254 files, DLL `00EF67570D30E4FF9C54A2AD2027393660436D5724BCEE2BB99E4C5718E966FD`. Snapshot `C:/Dev/KingmakerGunslingerLab/runtime-backups/live-mod/20261005T1817018793607Z`. Before, snapshot and restored tree SHA-256 `5C3E4AD499DC5116D2024D06260D11700087379937B96B7372C821628CB9BFBB`. Restoration VERIFIED / 254 files; lease released. Deployment `C:/Dev/KingmakerGunslingerLab/runtime-evidence/deployments/20261005T1817088787702Z/deployment.json`. Neither historical baseline was forced.

## Passed and failed surfaces

- All 35 live damage/maintain checks passed: printed bite/tail, live Strength/size, attribution, DR, held selection, recursive swallow graph and cleanup rules. The native Medium-versus-explicit baseline repair is supported by real rule evidence.
- All 80 final UI/lifecycle checks passed, including exact native sheet close, death/area reset, fallback and owned-resource cleanup. Speed 19/19 and icon binding 9/9 passed. The extra main assertion records safe removal of prior owned disposable summons before its exact census.
- Native combat matrix 33/55 passed. All tail-contact cells fail. Weighted bone/bind-pose world coordinates agree with the prior scale-correct BakeMesh path; this is real missing original-tail contact, not renderer-scale arithmetic. Some Dire bite contacts exceed 0.25m. Do not alter that tolerance, native reach, target/unit position or attack result.
- Fresh RTWP AI cells queue two interrupted Sprint attempts before their one successful cast. Exact appearance cleanup alone did not repair this. Inspect native setup/command timing with the full action list and retain the one-attempt/no-spam assertion.
- Manual cells remain NPC-controlled despite a disabled brain. One Dire rejected command stays unfinished while an unsolicited native attack occurs. The next request-local fixture must request native direct control through the exact owned RuleSummonUnit, rather than forcing controller state or adding a production control path.
- One Crocodile held cell hits but does not retain the required hold/rider. Record exact native maneuver rolls and permit bounded actual manual command retries for natural failure/escape; never force a hit, grapple, rider or animation contact.
- Crowd launch PID 37352 exited after request/build identity acceptance and runner creation, before OnUpdate, observer-ready, save interaction or a committed result. Error: `Kingmaker exited before committing a result after final rescan and bounded flush grace. stage=waiting-for-final-result; PID=37352; exitCode=`. Preserve this unavailable launch evidence and retry safely; no crowd visual PASS is claimed.

## Containing narrow repair

The containing source adds only a fixed crocodilian visual contact adapter following the qualified Wasp/Eagle/Stirge pattern: exact owner/weapon/original-mesh identities; the existing seven-joint tail chain; at most 0.25m of cosmetic root approach; native attack timing; per-view restoration/teardown. No generic limb, AI, movement, collision, animation or mechanical range rewrite. Original assets, icons, GUIDs, version and publication remain unchanged.

The manual fixture requests direct control only during its one exact synchronous summon, with subscription removal in finally. AI setup delays its decision clock through native appearance/combat-mode settlement, keeping all actions. New maneuver/control/pose diagnostics preserve failed-attempt information. Source gates and fresh full-batch qualification are required before any runtime claim.

`HumanReview: NOT_PERFORMED_NONBLOCKING`. All four accepted limitations/adaptation remain unchanged.

## Containing repair source gates

New 216 focused and 2018/2018 unfiltered tests PASS; repository/static/icon validation, 480 preflight, 68 orchestration, 11 persistence wiring + 3 round trips + 19 rejections, 5 crowd round trips + 7 rejections, clean exact-reference Release, deterministic strict 312-member standalone package PASS. Full pre-commit pipeline 2026-10-05T18:54:53.0776927Z–2026-10-05T18:57:19.1835664Z. Incremental compile had zero warnings/errors. This dirty pre-commit package is source evidence only. Commit/push, run complete gates again on the clean checkpoint, then freeze that exact artifact for the complete six-run batch.
