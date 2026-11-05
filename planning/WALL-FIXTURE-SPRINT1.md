# Wall fixture Sprint 1 — complete

The shared Wall fixture prerequisite passed twice in fresh Kingmaker processes
on the same corrected immutable development candidate. Full Wall acceptance is
pending; this sprint deliberately stopped before Wall ability lookup/casting,
obstruction, breach, removal/expiry and Bone Devil tail acceptance.

## Implementation and established findings

Reviewed baseline `ee5812b64e5a914a3036588827d75a58d83fe552` and checkpoint
`893d534ff340c0ee327163b34d271b1cfb953862` were preserved. Changes are fixture,
admission and focused test code; production Wall/Recast/Grid behavior, models,
textures, generators and scale logic are unchanged. All twenty accepted export
pins match. The earlier pre-abort navigation/LOS-ID correction remains intact;
its post-cast branches were not exercised here.

- The Wall lane survey now uses the installed native walking graph mask,
  `ConstructTags`, walkability, connected graph/area constraints and that graph's
  linecast. Both accepted runs selected RTWP mask **1**, native tags **-1**,
  **RecastGraph index 0 / connected area 1**, excluding overlapping GridGraph 1.
  Unconstrained survey selection was a confirmed source inconsistency; it is
  not established as the cause of the historical nineteen-metre displacement.
- Exact phase `summon-wall-placement-preflight` shares full Wall fixture
  creation, three-summon setup, grouped anchor, native Default lease, brain /
  command / agent preparation, survey and prerequisite implementation. A shared
  policy predicate covers all former exact-Wall comparisons. Parser/profile,
  PowerShell phase admission and native ownership use that policy. Separate
  explicit exits prevent both Wall ability lookup and subsequent tail dispatch.
- The planned lane is immutable. The old baseline helper's substitution
  `start = mover.Position` was removed. Initial placement and return use the
  same native `Translocate` helper, independent native-ground query, immediate
  entity/view/agent agreement and three consecutive ordinary quiet frames at
  the intended location. No position/ground tolerance was increased. No direct
  transform/agent position writes or repeated teleport retries supply success.
- Per-placement JSON retains exact actor/scene, immutable intent, expected native
  ground, before/immediate entity/view/agent world state, command identities,
  path/velocity state, first ordinary frames, first divergence and final
  support/verdict, including early refusal/timeout. Installed 2.1.7b IL confirms
  synchronous XZ-preserving `Translocate` + native Y/FlyHeight grounding, with
  AgentASP on the view transform; no invented independent agent-position API.
- Baseline acceptance additionally requires the exact active supplied command,
  positive native velocity over multiple samples and actual travel. Existing
  native Success, endpoint, path-distance/deviation and zero-wall-claim criteria
  remain. Return is verified against the **original** plan, never the settled
  position substituted into a new plan.

Source/native-contract and routing reviews were read-only. Focused behavioral
coverage checks exact preflight/full classification, parser/profile admission,
malformed values, wrong scenario, extra parameters, idle-menu/exit/save guards,
and refusal to treat preflight as module-OFF or player-input scope.

## Examined failed repeat and fixture correction

Initial candidate `07720571667420e974a0acd677472b3f0044be90` passed its first
preflight (20 assertions), but its required fresh repeat failed (16 assertions
total). Both remain retained, not acceptance on the corrected candidate:

- `20261105T1639434416214Z-disposable-expanded-summoning-sprint32-native-review` — PASS.
- `20261105T1643279960923Z-disposable-expanded-summoning-sprint32-native-review` — FAIL.

The repeat's immediate entity/view/agent placement was exactly correct. A new
unsupplied `UnitMoveTo` appeared at frame 885; native movement began by frame
887 with the brain still disabled. Its events also record a naturally failed
**DC19 Will save** and exact Bone-source `FearFrightened`, native frightened
system condition and source memory on the witness during summon setup.

Actual `UnitFearController` IL processes frightened/panicked units independently
of brain activity and can create/run this native movement command. Disabling
the brain alone therefore does not isolate a clear-lane movement fixture from
its setup fear. The production five-foot aura intentionally affects allied
summoners too; this was not treated as a production defect.

Correction `8c9ccb761c4742ba1da9aa1e028064df883f806b` parks the source away from
the planned lane, uses the existing exact-blueprint/exact-source cleanup helper
to remove only its setup frightened buff from the disposable witness, preserves
natural source memory, and observes ordinary panic/command retirement. Quiet
frames require frightened/panicked state absent. No roll, save, stat, immunity,
fear flag, aura or movement result is injected or changed.

Neither final successful run had a setup fear effect to remove. Consequently
they prove the current shared fixture prerequisite twice, while the nonempty
removal branch was not separately reproduced natively in those runs. No further
runs were selected merely to fish for an obsolete failure or a particular roll.
The historical nineteen-metre displacement's exact causal chain remains unproven.

## Exact accepted development candidate

- Source: `8c9ccb761c4742ba1da9aa1e028064df883f806b`, version **0.0.177**.
- Purpose: `summon-outsider-review-corrections`; not release qualification.
- ZIP: `artifacts/development-candidates/summon-outsider-review-corrections/8c9ccb761c47/KingmakerGunslinger-0.0.177-development-summon-outsider-review-corrections-8c9ccb761c47.zip`.
- ZIP SHA-256: `ffa890ea2c6825b08deaad429d74c3514c4e9acd7802abbc4431db8a9f6bae34`.
- DLL SHA-256: `3d891e67c4727708db1f2e533a942b3859cd3b4f13e447df2de96945ab9d5f3a`.
- DLL MVID: `23cf32ff-25af-4829-a5cf-d5dc5a5f7ed1`.
- Source fingerprint: `7291471d197f153cc0cb0561313131680ce4faa5f86e4c0332cd7771318f021f`.
- Exact-reference compilation, 11 selected domain tests and strict packaging PASS.

## Two successful native preflights

Evidence directories are under `C:/Dev/KingmakerGunslingerLab/runtime-evidence/`.
Both use suffix `-disposable-expanded-summoning-sprint32-native-review`.

| Evidence directory timestamp | Run ID | Fresh PID | Result |
| --- | --- | --- | --- |
| `20261105T1709110604156Z` | `20261105T1709110895096Z-a737a372625340ad9568c989dd7bb986` | 15232 | PASS / 21 |
| `20261105T1713072453582Z` | `20261105T1713072991257Z-3509ef822dba47b5890c29ac32dd869f` | 23704 | PASS / 21 |

Result SHA-256, in row order:
`e68e119ceb521d85a027798f38b4295f03f36ec754edd59bb6b5bab2575f070b`,
`4c6efe2d5372b9dc7f93cd7ab0bc37fb77e3d95eae4d84b96974221f96a62ab8`.

In both runs the immutable graph start was
`[9.747345,-5.21520329,16.0368938]`; native supported world root was
`[9.747345,-5.562784,16.0368938]`. Entity, view and agent matched that supported
root immediately and after three quiet frames, both initially and on return.
The Y difference is the documented native ground contract, not a changed plan.
No first divergence was observed.

Planned destination: `[17.747345,-5.22498751,16.0349369]`.
Planned casting point: `[13.747344,-5.65821028,19.9921036]`.

| Measurement | First accepted run | Fresh repeat |
| --- | --- | --- |
| Native command started / acted / finished | true / true / true | true / true / true |
| Terminal result | Success | Success |
| Positive-velocity exact-command samples | 81 | 235 |
| Exact-command observed travel | 7.9008193m | 7.948757m |
| Total actual path travel | 8.149094m | 8.158641m |
| Peak native velocity | 3.048m/s | 3.048m/s |
| Maximum horizontal route deviation | 0.00000572m | 0.00000954m |
| Endpoint | `[17.7153587,-5.284958,16.0349445]` | `[17.7093163,-5.28366327,16.0349464]` |
| Return immediate / three-frame agreement | PASS / PASS | PASS / PASS |
| Wall sections / navigation claims | 0 / 0 | 0 / 0 |

The first process exited/restored before the second launched; both verified the
same ZIP, DLL, MVID and source fingerprint. All four Sprint 1 transactions
restored the current installation **v0.0.140** exactly (live-tree SHA-256
`5c3e4ad499dc5116d2024d06260d11700087379937b96b7372c821628cb9bfbb`),
native settings and protected save bytes/length/mtime. No save load or mutation.

## Validation ledger and commands

Owner explicitly superseded the former 85-minute ceiling for this narrow sprint.
Retained prior validation **84.857689 minutes**; additional **32.461544 minutes**;
cumulative **117.319233 minutes**. Earlier separate ledgers remain preserved.
All preflights, including the failed repeat, count. Authoring/source inspection
is separate from validation, consistent with the retained policy ledger.

| Check | Seconds | Result |
| --- | --- | --- |
| Initial focused compilation / 11 tests | 27.090 | PASS |
| PowerShell exact admission / OFF refusal | 0.747 | PASS |
| Candidate 077205716674 build | 542.856 | PASS |
| Initial preflight | 190.450 | PASS / 20 |
| Initial candidate fresh repeat | 171.451 | FAIL / 16 total |
| Fear-isolation focused compilation / 11 tests | 27.629 | PASS |
| Corrected candidate 8c9ccb761c47 build | 564.948 | PASS |
| Corrected preflight | 173.239 | PASS / 21 |
| Corrected fresh repeat | 189.282 | PASS / 21 |
| Final evidence/identity/handoff verification reserve | 60 | Included |

One immutable candidate was built per source identity; no unchanged rebuild.
After the failed repeat, its evidence was examined before the targeted fixture
repair. The final source was reused unchanged for its fresh-process repeat.

Commands used the existing workflows:

```powershell
scripts/Invoke-KmgGate.ps1 -Filter 'expanded-summoning.sprint32-native-admission;expanded-summoning.sprint35;expanded-summoning.overhaul'
scripts/Test-SummonOverhaulOffAdmission.ps1
scripts/Build-Local.ps1 -DevelopmentPurpose summon-outsider-review-corrections -DevelopmentTestFilter 'expanded-summoning.sprint32-native-admission;expanded-summoning.sprint35;expanded-summoning.overhaul' -ReferenceBundleDir artifacts/summon-overhaul-baseline/reference-input/KingmakerGunslinger-private-build-references
```

Guarded `artifacts/wall-fixture-sprint1/run-native.ps1` admits only the explicit
placement preflight and uses the exact candidate/version/purpose, fresh
processes, native leases and transactional intake/restoration. Its underlying
scenario is `disposable-expanded-summoning-sprint32-native-review`, parameter
`reviewPhase=summon-wall-placement-preflight`, no SaveName. Logs, exact timing
files, retained ledger, raw-result summary and `verified-prerequisite.json`
are in that artifact root. Machine-local evidence/packages are not committed.

## Next sprint

Stop Sprint 1 here. Full Wall acceptance remains pending: legal casting,
navigation/obstruction and attack-eligibility diagnosis, physical damage,
destruction, exact grid/claim restoration, controlled original-deadline expiry,
post-removal movement and Bone Devil tail contact. Keep PR #96 draft and
unmerged. Accepted creature appearance/motion and original candidate identities
remain credited; no cosmetic overhaul or compatibility campaign was reopened.
