# Sprint 16 second reconciled hidden batch, 2026-10-05

**Disposition: FAIL — Dire remains withheld; Sprint 17 has not begun.**

Candidate: `634b6c18972747a4bff0ceb27f29af5189e5f586`. The six scenarios reused one frozen clean-head package and each used a fresh Steam App ID 640820 process. Previous and new source qualification are not gameplay qualification.

Clean-head prelaunch: 216 focused / 2018 unfiltered PASS; repository/static/icon validation; 480 runtime preflight and 68 orchestration checks; closed persistence and crowd request tests; clean exact-reference Release; deterministic 312-member strict standalone package. Prelaunch 17:29:54–17:31:55 UTC.

| Identity | Value |
| --- | --- |
| sourceStateSha256 | `3549ad80bb2728488c47b4789284b7488f64995b24616abcf79dcdede2e405b8` |
| dllSha256 | `f02d197c1afe763fc41dcc1f3591ae8c76e531bd6fbb93c24b6f8b10c9c26f98` |
| dllMvid | `e0cd36a5-64b5-4f35-8a71-4e0713c3cf2f` |
| packageSha256 | `567f863ea3d54085e505f523a77ef7cb1ee0bee870f213bfdf27aabe7a3fd61d` |
| version | `0.0.141` |

| Scenario | Result | PASS / total | Fresh PID | Evidence directory / result SHA-256 | Native working save writes |
| --- | --- | --- | --- | --- | --- |
| working-save-smoke | PASS | 11/11 | 24644 | `20261005T1733359533795Z-working-save-smoke` / `553511B2AD65EA5E04C63EF5B07C75EE95CFAB8626EF62A946FA0FECB21FA79D` | 0 |
| disposable-expanded-summoning-crocodilians | FAIL | 173/205 | 28408 | `20261005T1734535852027Z-disposable-expanded-summoning-crocodilians` / `846C67FB46BB040FA629827A5B735A3803F117A430203EB8BFFAFC164F8254B5` | 0 |
| working-save-expanded-summoning-creature-review | FAIL | 17/20 | 38016 | `20261005T1742339927155Z-working-save-expanded-summoning-creature-review` / `1C9D1637A3B80E75F936A8B0FB6612B59567B8E8385C04A19E31D4CE804E36A5` | 0 |
| working-save-expanded-summoning-prepare | PASS | 9/9 | 38832 | `20261005T1748033040686Z-working-save-expanded-summoning-prepare` / `930C5553C243A33749F341CD887D0F0E48EE0022FEF07B1F116CD5637A14BCDA` | 1 |
| working-save-expanded-summoning-verify-cleanup | FAIL | 6/9 | 9600 | `20261005T1749156934228Z-working-save-expanded-summoning-verify-cleanup` / `1C89FC742652FE43593D81FD641B3351F71A4256F27A654428184E8EF996EE50` | 0 |
| working-save-expanded-summoning-verify-absent | FAIL | 4/5 | 37792 | `20261005T1750235719413Z-working-save-expanded-summoning-verify-absent` / `601CA4CB5386DD20A0F7AD6AC5B9B424A79E5DA335C8F243DA7C733D310F407E` | 0 |

Batch: 2026-10-05T17:33:04.3537593Z through 2026-10-05T17:51:35.2121186Z. All processes exited automatically; no kill or UI input. No unexpected save-writing API was observed. Prepare made exactly one authorized native KMG_AUTOMATION_WORKING save, with two stashed areas. Verify-cleanup refused its write after failed checks. Five closed fixture units therefore remain in that disposable working save until successful guarded cleanup; final absence correctly failed. No raw save was read, copied, parsed or mutated.

## Exact restoration

Lease acquired before the actual installation observation: `runtime-20261005T173311Z-76a08ffb5bcd45a8995fc82ff7ff15a6`. Actual installation was **0.0.140 / 254 files**, DLL `00EF67570D30E4FF9C54A2AD2027393660436D5724BCEE2BB99E4C5718E966FD`. Snapshot: `C:/Dev/KingmakerGunslingerLab/runtime-backups/live-mod/20261005T1733246964445Z`. Before, snapshot, and restored tree SHA-256: `5C3E4AD499DC5116D2024D06260D11700087379937B96B7372C821628CB9BFBB`. Restoration VERIFIED / 254 files; lease released. Deployment: `C:/Dev/KingmakerGunslingerLab/runtime-evidence/deployments/20261005T1733310364340Z/deployment.json`. No historical baseline was forced.

## Failure classification and next narrow repairs

- Production dice: Dire printed bite/tail, strength variants, and size scaling now pass. Crocodile remains 1d6 instead of 1d8 because its native weapon type supplies Medium-relative dice; explicit KMG dice are already creature-baseline-relative. Use the exact native IsDamageDiceOverridden contract to choose the baseline, retaining live body size plus weapon-rule delta and any legitimate override.
- Production cleanup: lethal target release, native swallow lifecycle, load-time removal of native relationship parts/buffs, and rollback-owned resource cleanup pass. No further global grapple/animation rewrite is justified. Old source comments claiming native relationships never deserialize are corrected to match observed load reset.
- Fixture appearance: every persisted unit, including the unrelated wolf control, has CantAct/CantMove despite zero grapple parts, records and owned relationship buffs. Prepare froze the native appearance buff. Remove only that exact native fixture buff before arming persistence; retain both condition assertions and record all remaining buff sources.
- Fixture destruction: direct Dispose clears View before the native destruction controller can destroy it. The crowd leaked precisely two loaded original view meshes/four materials per creature. Keep the view for native destruction. Distinguish retired cached AllUnits references from live scene/view references, as the existing qualified persistence protocol does; still require zero live units, destroyed/detached original units, and zero summons on a fresh load after the cleanup save.
- Fixture UI: all cached character/section/group/selection/time/pause values restore, but the sheet keeps IsShow after tabs detach it. Invoke the exact opened sheet's native Show(false); retain both closed-state assertions.
- Fixture cooldown: RTWP returns Fail after executing availability; turn-based rejects with Interrupt before acting. Both must leave exactly one successful Sprint and the identical cooldown object/deadline; neither permits a second cast.
- Fixture AI timing: initial RTWP AI attempts interrupt during the summon appearance lock before one Sprint succeeds. Start the ready disposable combat actor using the existing exact appearance cleanup convention; retain no failed-Sprint spam and unchanged native brain/action requirements.
- Fixture presentation: a moving capture lands on a native fog fade (Crocodile dissolve 0.177). Wait within the existing bounded fade budget, with no material/visibility forcing and no relaxed intact-frame acceptance.
- Unresolved original contact: weighted bite/tail gaps still fail. The next guarded observer computes actual skin world positions from original vertices, weights, live bone matrices and bind poses, retaining 0.25 m and recording both previous BakeMesh conversions. This is a measurement correction/probe, not qualified contact and not permission to change range/geometry.
- Unresolved turn/cleanup: one Dire turn-based swallow cell never observes maintained damage. End only the fixture owner's completed idle native turn when a later maintain is needed; record exact native turn/buff/brain/control and census details. Main combat can also lose the five old persisted fixture units during its long observation; clear prior owned disposable KMG summons before taking its exact starting census, without a save write.

## Required next boundary

Run new focused, complete, validation, preflight, clean Release, deterministic and strict package gates. Commit/push a coherent source-qualified correction, then qualify one new clean-head artifact. No previous result qualifies new DLL bytes. Keep every failed attempt and all mechanical, visual, AI, mode, persistence, cleanup and publication assertions. External remote motion is still a stop boundary.

`HumanReview: NOT_PERFORMED_NONBLOCKING`. Accepted senses, reload reset, interior AC/HP limitation and swallow-else-Death-Roll adaptation remain unchanged.

## Next repair source gates

The containing correction passed new 216 focused and 2018/2018 unfiltered tests, repository/static/icon validation, 480 preflight, 68 orchestration, closed persistence/crowd request checks, clean exact-reference Release and deterministic strict package gates on 2026-10-05 18:06:34.9166726Z–18:09:19.3465386Z. This pre-commit source result does not qualify gameplay. No publication or assets changed. A new clean-head pipeline and guarded batch are required after checkpoint publication.
