# Sprint 4 closure checkpoint (S4 T1 to S4 T8)

Status: **Sprint 4 is accepted and formally closed for its reviewed development scope** (principal review,
2026-11-18). Acceptance is not merge or release. Nothing was merged, released, tagged, published, installed into the
human game or sent for review. Every pull request listed below stays an open draft with no reviewers. Every
limitation recorded below is unchanged by the acceptance.

## Accepted identities

| Item | Identity |
|---|---|
| Closure PR | #185, branch `codex/ver01-rapid-reload-transaction` |
| Accepted records head | `5c662d50b0824bf5eabb02892bfbc172db8a7374` |
| Qualified product/source commit | `16708bd5e237bd4f2f0fefc1d6cd0f2c9ba2e399` |
| Package SHA-256 | `e017a5f3d8ec5c9e2164f88d6b8c36a69d1e1556da59a2b13b6ae0d675a27634` |
| DLL SHA-256 | `cbca54912642c0ef7c5913489e3101ce8c58d424f52fc8262fb24e07547a3f26` |
| MVID | `90ce7e7d-9c5a-46ec-9c95-c0734d2faaec` |
| Harness SHA-256 | `c47e7f14d48edac88740b4c9bf1841e805c45a88043dc162789b4c86dbc1b089` |
| Final sessions | `20261118T014552Z-owner-0.33.0-true-grit-level-up` (6 of 6), `20261118T014708Z-owner-0.33.0-rapid-reload-transaction` (11 of 11), `20261118T014826Z-owner-0.33.0-deadeye-cost` (PASS) |

The three final sessions are execution on `16708bd5e237` only; the later records-only head `5c662d50b0` was not
rebuilt or rerun. The failed first True Grit run (candidate `fd4087c312bd`, session `20261118T013304Z`) remains a
fixture failure under its own identity.

| Story | Accepted head |
|---|---|
| S4 T1 / DEF-04 | `2fe1b8af2c3adc6e1ea58a34fd8f256b437083a6` |
| S4 T2 / REP-02 | `9b8a755ec80cdb19ebce787c433d87fef75c8f23` |
| S4 T3 / POL-01 | `ee65d09e9d3d48b38db8ee67c139829c3713b171` |
| S4 T4 / POL-02 | `c638de23857724dfaea39c2270467f0a79b12b78` |
| S4 T5 / RISK-01 | `e30b19e8fef03bd4c5e614e3e078ca9e0a467345` |
| S4 T6 / RISK-04 | `bb789b1891b5e76e21345b0dc6abd2ba8c70db7e` |
| S4 T7 / RISK-05 | `d53487a2a6ee8cf595069f55828d5d5c7409ff31` |
| S4 T8 / VER-01 and closure | `5c662d50b0824bf5eabb02892bfbc172db8a7374` |

Closure branch: `codex/ver01-rapid-reload-transaction`, stacked on `codex/risk05-proficiency-provenance` (draft PR #184).

## Final Sprint 4 candidate

One development candidate was built from the closure branch and used unchanged for every Sprint 4 native session
below.

| Item | Identity |
|---|---|
| Source commit | `16708bd5e237bd4f2f0fefc1d6cd0f2c9ba2e399` (`codex/ver01-rapid-reload-transaction`) |
| Package | `KingmakerGunslinger-0.0.191-development-sprint4-closure-16708bd5e237.zip`, SHA-256 `e017a5f3d8ec5c9e2164f88d6b8c36a69d1e1556da59a2b13b6ae0d675a27634` |
| DLL | SHA-256 `cbca54912642c0ef7c5913489e3101ce8c58d424f52fc8262fb24e07547a3f26`, MVID `90ce7e7d-9c5a-46ec-9c95-c0734d2faaec` (the same in every launch) |
| Build | `scripts/Build-Local.ps1 -DevelopmentPurpose sprint4-closure`, 2026-11-18 01:38:36 to 01:45:36 UTC (7 min), development validation PASS against baseline v0.0.191, 113 of 113 selected tests |
| Harness | `KmgLoaderCompatHarness.dll`, SHA-256 `c47e7f14d48edac88740b4c9bf1841e805c45a88043dc162789b4c86dbc1b089` (source unchanged since `c638de2385`) |
| Loader | `owner-0.33.0` (UMM 0.33.0), the loader-compat game copy, game 2.1.7b, non-elevated session |

| Session | Story | Result | Time |
|---|---|---|---|
| `20261118T014552Z-owner-0.33.0-true-grit-level-up` | S4 T6 | PASS, 6 of 6 assertions | 63 s in game, 77 s total |
| `20261118T014708Z-owner-0.33.0-rapid-reload-transaction` | S4 T8 | PASS, 11 of 11 assertions | 65 s in game, 78 s total |
| `20261118T014826Z-owner-0.33.0-deadeye-cost` | S4 T4 | PASS, every probe expectation | 85 s in game, 99 s total |

Each session reported the 95 owner saves unchanged, the owner installation unchanged and the registry restored.

### Preserved earlier runs (not the final evidence)

**First closure candidate.** Built at `fd4087c312bd` (package `551a5b36…1171`, DLL `a822fad2…1325`, MVID
`287d2056-ac4d-467f-9fe3-993bf447c035`), with three sessions:

- `20261118T013027Z-owner-0.33.0-rapid-reload-transaction` passed, 11 of 11.
- `20261118T013600Z-owner-0.33.0-deadeye-cost` passed.
- `20261118T013304Z-owner-0.33.0-true-grit-level-up` **failed** one assertion (`true-grit-respec`). This was a
  fixture defect: the observer added `chosenDeedCosts` to the after-commit snapshot that it later compared with the
  cancelled respec, so equal characters could not compare equal. Its other five assertions passed, including the
  distinctness steps.

The observer was repaired on the S4 T6 branch (`bb789b1891`), cherry-picked to the closure branch (`16708bd5e2`), and
the candidate was rebuilt. All three sessions were then run again on that one candidate, so the final evidence above
comes from a single unchanged build.

**Earlier build attempts.** The first closure build attempt refused a purpose name that was not lower-case and
hyphenated. The second stopped at `Test-IconPolishRound2Assets.ps1`: the S4 T5 containment changed
`ProductionFirearmCatalog.cs`, a file that check pins. The pin was refreshed with a note on the S4 T5 branch
(`e30b19e8fe`), as DEF-04 had done for `GunTrainingBlueprints.cs`, and cherry-picked (`fd4087c312`).
`scripts/validate-repository.ps1` then passed on its own (490 s) before the rebuild.

**Stack note.** The S4 T6 and S4 T7 branch heads predate the S4 T5 pin refresh, and S4 T7 predates the S4 T6 fixture
repair, because history was not rewritten and no branch was merged. The closure branch carries both fixes through
`cherry-pick -x`.

## Closure table

| Story | Inventory | Branch / PR | Exact head | Classification | Focused tests | Native evidence and identities | Residual limits | Merged | Released |
|---|---|---|---|---|---|---|---|---|---|
| S4 T1 | DEF-04, GS-AUD-0032 | `codex/def04-gun-training-progression`, #178 | `2fe1b8af2c` | Text-only policy statement; accepted in review at `2fe1b8af2c` | 22 of 22; complete suite 3,071 of 3,071 | Candidate `0c0f4f7b7708`, session `20261117T033517Z`, 12 of 12 real level-ups | Respec analysed from code only; no historical save; no level-up screens | no | no |
| S4 T2 | REP-02, GS-AUD-0038 | `codex/rep02-pistolero-damage-consumption`, #179 | `9b8a755ec8` | Diagnostic repair, gameplay unchanged; accepted in review at `9b8a755ec8` | Focused per record | Candidate `d72746b81c5d`, package `0a43cd8f3e3f…`, DLL `99a19f16210d…`, harness `593a26d438f8…`, session `20261117T183252Z`, 4 of 4 | No native recovery offset; immunity, insufficient grit, misfire, scatter and duplicates from the component fixture only | no | no |
| S4 T3 | POL-01, GS-AUD-0013 | `codex/pol01-sprint4-policy`, #180 | `ee65d09e9d` | Owner decision B implemented; accepted in review at `ee65d09e9d` | `FirearmMisfireRecoveryTests` | Candidate `5fcc3e9d3ed9`, package `aec986ff…4065`, DLL `9a531808…b7cf`, MVID `d358e9c9-…f56e`, session `20261117T211826Z`, 42 of 42 | Hold position not executed; the four break-interruption assertions not executed; deliberate stop is cancellation before acting only | no | no |
| S4 T4 | POL-02, GS-AUD-0011 | `codex/pol02-deadeye-cost`, #181 | `c638de2385` | Policy reconciliation (flat-one-grit CRPG adaptation retained); accepted with Sprint 4 | 49 of 49 | Final candidate, `deadeye-cost` session: refused without grit; one debit; touch-AC hit beyond the first increment; miss consumed; first-increment shot consumed; cancelled order kept, no refund; expiry after 6.01 s, no refund | True Grit's reduced cost not exercised natively; text does not mention True Grit's reduction | no | no |
| S4 T5 | RISK-01 | `codex/risk01-advanced-revolver`, #182 | `e30b19e8fe` | Containment as unsupported content; accepted with Sprint 4 | 89 of 89; icon catalog validator | Registration on the final candidate (the mod loads and the class registers in every session); no dedicated session (no legitimate save or acquisition route) | A revolver already equipped by external means stays equipped (reloads still fail safely); CMI qualification not run | no | no |
| S4 T6 | RISK-04 | `codex/risk04-true-grit-distinct`, #183 | `bb789b1891` | Demonstrated native invariant; test-only pair helper removed; accepted with Sprint 4 | 11 of 11, 10 of 10; preflight 1,205 | Final candidate, `true-grit-level-up`, 6 of 6: duplicate neither offered nor accepted in nine steps; commit Deadeye + Quick Clear at rank 1, each costing 0; cancel, injected failure and cancelled respec unchanged; committed respec restarts at Gunslinger 1 with no True Grit | Disposable character advanced without earlier selections; the level-up screen not observed | no | no |
| S4 T7 | RISK-05 | `codex/risk05-proficiency-provenance`, #184 | `d53487a2a6` | Boundary record; no supported independent provider exists; accepted with Sprint 4 | 55 of 55 | None required; the S4 T8 matrix rows with request-local independent sources are fixture evidence only | A provider surviving a Gunslinger's reconciler is a future requirement, not verified compatibility | no | no |
| S4 T8 | VER-01 | `codex/ver01-rapid-reload-transaction`, #185 | qualified product `16708bd5e237bd4f2f0fefc1d6cd0f2c9ba2e399`; accepted records `5c662d50b0824bf5eabb02892bfbc172db8a7374` | Dedicated scenario executed; no product or fixture defect found; this closure record; accepted with Sprint 4 | Repository validation, then 113 of 113 in the build | Final candidate, `rapid-reload-transaction`, 11 of 11 (details below) | The scenario's own limits (below) | no | no |

## S4 T8 / VER-01: the dedicated Rapid Reload transaction scenario

**Location and authorization.** The scenario is `disposable-rapid-reload-proficiency-gate`
(`RuntimeTesting/RuntimeTestRunner.RapidReloadProficiencyGate.cs`). It had never been executed, because the runtime
orchestrator refuses elevation and the authoring sessions were elevated. This session was not elevated: a token check
reported `IsInRole(Administrator) = False`. The scenario ran through the supported loader-compat product launch, with
no change to any admission check.

**What was exercised** (session `20261118T014708Z-owner-0.33.0-rapid-reload-transaction`, 11 of 11):

- **Selection flow:** 19 real `LevelUpController` cases, none reporting a cleanup error.
  - Refusals and the empty child selection.
  - Scoped child refusals.
  - Acquisitions through ordinary and Fighter combat-feat slots.
  - The legacy-wrapper owner.
  - A fresh level-1 Gunslinger.
  - A Gunslinger 1 taking a Fighter bonus combat feat.
  - Request-local independent sources.
- **Pending class and archetype changes** in one live transaction. With a Rapid Reload (Pistol) choice held, the
  pending Musket Master change:
  - left two-handed proficiency only (full 0, two-handed 1);
  - granted the automatic Rapid Reload (Musket) at rank 1;
  - kept the parent held once;
  - cleared the now-invalid Pistol child (not selected, rank 0, not selectable);
  - left only Blunderbuss eligible (`[false, false, true]`).

  Removing the archetype restored full proficiency, removed the automatic grant and made every child eligible again.
  All of this happened inside one controller instance.
- **Musket Master and duplicates:** the owned Musket child cannot consume another feat, Pistol stays out of scope, and
  Blunderbuss is acquired by exactly one feat slot, after the `LevelUpState.IsComplete` gate.
- **Class-identity negative control.**
- **Visit-ownership cleanup:** a natively rejected archetype after the controller exists; the helper cancels it, and
  the setup error keeps its message and stack.
- **Native prerequisite matrix:** nine fixtures.
- **Catalog publication and combat-feat classification.**
- **Party and unit isolation, and the loaded version.**

**Result:** no product or fixture defect was observed, so nothing was repaired.

**Limits:**

- The scenario uses disposable characters, not a save.
- The level-up screen and the controller-driven UI path (`CharacterBuildController.Next`) were not observed; the
  scenario uses the controller calls they make.
- The cleanup-failure injection is a controlled throwing cancellation, not an engine fault.

## Commands and durations (S4 T4 to S4 T8)

| Command | Result | Time |
|---|---|---|
| `Invoke-KmgGate.ps1 -Filter 'deadeye;steady-aim;true-grit;archetype'` (S4 T4) | 49 of 49 | 21 s |
| `Invoke-KmgGate.ps1 -Filter 'firearm-capacity;advanced;craft-magic-items;firearm-descriptions;presentation;sprint31;sprint33;production-firearm'` (S4 T5) | first 87 of 89 (two test-fixture failures, fixed), then 89 of 89 | 26 s, 4 s |
| `Invoke-KmgGate.ps1 -Filter 'true-grit;sprint59;runtime-scenario;scenario-catalog;runtime-automation'` (S4 T6) | 11 of 11 | 27 s |
| `Test-RuntimeScenarioPreflight.ps1` (S4 T6) | 1,205 passed | 55 s |
| `Invoke-KmgGate.ps1 -Filter 'true-grit;sprint59'` (S4 T6, twice) | 10 of 10 | 26 s each |
| `Invoke-KmgGate.ps1 -Filter 'firearm-proficiency;rapid-reload;archetype;pistolero;musket-master'` (S4 T7) | first compile failed (missing `using`), then 55 of 55 | 16 s, 24 s |
| `tools/validate_icon_catalog.py` (S4 T5, S4 T7) | PASS | under 5 s |
| `scripts/validate-repository.ps1 -DevelopmentPurpose sprint4-closure` | PASS | 490 s |
| `Build-Local.ps1` (attempts: purpose name refused; pin stop; `fd4087c312bd`; `16708bd5e237`) | refused; failed; PASS; PASS | 1 s, 25 s, 7 min 15 s, 7 min |
| Loader-compat sessions on `fd4087c312bd` | rapid-reload PASS; true-grit FAIL (fixture); deadeye PASS | 100 s, 79 s, 105 s |
| Loader-compat sessions on `16708bd5e237` | three PASS | 77 s, 78 s, 99 s |

No Sprint or Tranche gate was run. POL-01 and REP-02 were not rebuilt or rerun.

## Epic exit condition, as it stands for review

- **The chosen firearm rules have behavioural evidence:**
  - Deadeye's flat cost and armed-state contract (native);
  - misfire recovery (S4 T3, native);
  - Up Close and Deadly damage and consumption (S4 T2, native);
  - Gun Training's four opportunities (S4 T1, native level-ups).
- **Progression is coherent:**
  - True Grit's two deeds stay distinct through real level-up, back navigation, cancellation and respec;
  - Rapid Reload's pending class and archetype changes clear the invalid child;
  - proficiency scope rules are explicit.
- **Latent unsupported content is contained:** the legacy Advanced Revolver is registered, loadable, unequippable and
  refused as a pattern at registration. The proficiency provenance boundary is recorded as a requirement for future
  providers.

Every Sprint 4 story was worked; Sprint 4 is accepted and closed for its reviewed development scope (2026-11-18).
Not merged, not released.
