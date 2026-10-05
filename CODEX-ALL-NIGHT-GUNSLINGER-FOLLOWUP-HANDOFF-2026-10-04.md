# All-night Gunslinger followup handoff - 2026-10-04

Status: COMPLETE within the finite mission, including its permitted safe-foundation disposition. The finite mission includes a deliberately
unpublished Whiteout policy/observation foundation; native Whiteout binding is
blocked by the observed weather prerequisite below. No release is authorized.

## Repository state and provenance

- Repository: `howardreith/KingmakerGunslingerAndOtherTabletopRules`.
- Exact fixed starting SHA: `5482db429bd3c4009a031aa733a3091edfa5fe5e`.
- Branch: `codex/gunslinger-content-followup-2026-10-04`.
- Isolated worktree: `C:/Dev/KingmakerGunslingerLab/worktrees/gunslinger-content-followup-2026-10-04`.
- Final implementation/source HEAD: `d4afa40947741c2f8c7a15024375ad4618550a9b`.
  The final handoff/documentation commit is its direct successor; resolve its
  exact self-referential HEAD with `git rev-parse HEAD` and the approved wrapper's
  remote equality check. The final chat reports that exact documentation SHA.
- Final pushed implementation HEAD: `d4afa40947741c2f8c7a15024375ad4618550a9b`.
  Every implementation checkpoint is remote-verified. The documentation
  closeout uses the same exact wrapper; its containing commit is the final tip.
- Master began and remains `2ce70e4e7e9d3c97ca1008ab05a341e758f5cf5a`;
  no main ref exists. Phase 2A remains the exact fixed starting SHA.
- Phase 2B moved remotely from `cdec0180b948dea0c41f4d6b09d97b37f5c478ad`
  to `a762ece553ab9b539a53a782102a29e5c70d44eb` during this mission.
  Its committed objects were read for the forecast only. Its branch/worktree
  was not checked out, changed, deployed, pushed or integrated by this mission.
- The original main worktree remains on the weekend branch at
  `1b7bfa06ae897e2359d8ba6a0ec5522bc1183a57`; no foreign worktree was edited.
- No merge, rebase, history rewrite, force push, master/main movement, tag,
  release, PR, version bump or save mutation occurred. Info/assembly/package
  version remains **0.0.141**, informational version remains Phase 2A.

Required refs were fetched without pull/rebase; exact base existence, origin,
Phase 2A ref equality and master ancestry were checked before worktree creation.
Later read-only Phase 2B fetches used `--no-tags --no-write-fetch-head` and exact
SHAs, without updating its branch. No worktree was deleted or pruned.

## Slice matrix

| Workstream | Disposition | Coherent checkpoint | Principal changed files and evidence |
|---|---|---|---|
| A - firearm descriptions | PASS | `3cbeb4c6766eb7a500a9fbbc58d0d1e28768d5aa` | `FirearmEnchantmentItemText.cs`, penetration presentation, production/magic blueprints, progression/midgame catalogs, description tests, `RuntimeTestRunner.FirearmDescriptions.cs`; four new domain cases, resolved-string runtime 8/8 |
| B - race-trait contracts | PASS | `f1155cb158188a1264f8b53ac41a64f514fe099a` | `docs/research/CHARACTER-RACE-TRAITS-FEASIBILITY.md`; Fiery ADAPTED, Stoic same-source prerequisite and full acceptance matrix, Earthsense omission; no gameplay publication |
| C - vendor audit/model | PASS | `f1155cb158188a1264f8b53ac41a64f514fe099a`, `4b3b6ae450f071ea07a746764d339f0612b3a563` | audit Markdown/CSV; Model C corrected, Model D selected, 65-row corrected historical ledger; later closeout aligns acquisition/placement/manual docs and README |
| D - Model D implementation/migration | PASS | `4b3b6ae450f071ea07a746764d339f0612b3a563` | capital/Oleg/Bokken and Eastern/spear campaign publishers, `VendorCatalogPublication.cs`, bootstrap wiring, `ModelDVendorTests.cs`, runtime vendor observer and legacy guards; 26 new tests, runtime 26/26 |
| E - Whiteout foundation | PARTIAL-SAFE-FOUNDATION | `db2ab71ee3450c12c8b7bc1de527209ed893cd0e`, `d4afa40947741c2f8c7a15024375ad4618550a9b` | `WhiteoutPolicy.cs`, 35 policy/observation cases, weather/metadata runner partials, request/scenario guards; live scene observation 6/6 and metadata 2/2 |
| E - native state/attack binding | BLOCKED-WITH-EVIDENCE | same checkpoints | Indoor/active-precipitation qualification remains ambiguous; mission 11.4 requires policy plus observation. No marker, buff, native adapter or patch is registered |
| F - integrated qualification/closeout | PASS | implementation HEAD plus documentation successor | current gate, focused 78 tests, full 2,040, preflight 480, strict package, five fresh-process scenarios, exact restore and safety audits below |

Project/test compile registration, `Program.cs`, runtime catalogs,
`RuntimeAutomation.Common.ps1`, `Test-RuntimeScenarioPreflight.ps1`,
`validate_expanded_summoning_phase2a141.py` and the active validation record were
reconciled with the fixed baseline. Older count records remain historical.
`git show --stat <checkpoint>` gives each exact file set.

## Port ledger

| Weekend commit | Ported/reconciled | Superseded or omitted |
|---|---|---|
| `dea494233` | Declarative mundane descriptions; lead-ball versus Scatter Shot wording; real penetration policy; one Reliable/Seeking description source; four focused cases and current registration | Blind cherry-pick, stale test totals and release assumptions omitted. New runtime reads actual registered localization getters |
| `bc83e0044` | Engine precedents, faithful conditional late Favored Class `racial_traits` offering, research content | Fiery is ADAPTED success-only native take-10, not EXACT; Stoic requires exact incoming same-effect/source lineage and grants when correlation cannot prove existing effect; Earthsense acknowledges Blindsense/Blindsight but remains OMITTED-NO-FAITHFUL-ENGINE-CARRIER |
| `ee88a6361` | Native weather/event/concealment findings and rewritten frozen Whiteout contract | Indoor-clear inference is not runtime proof; no fake Partial, additive 30%, fog/VFX activation, visible publication or unsupported completion claim |
| `7e7cee271` | Audit and CSV, corrected into a 65-row historical before-ledger; exact owner Model D | Model C concentration/resupply claims rejected; wrong 4-versus-6 spear count, invalid item symbols, omitted cold-iron variants/World-Tree Severer and transposed cells repaired |

No weekend handoff, interim mission ledger, tracked `.zcodeignore` ignore change,
0.0.140 release assertion, hard-coded 1,922 total or weekend runtime evidence was
ported as qualification. `.gitignore` is unchanged; no local `.zcodeignore`
exclusion was needed. No race-trait implementation or icon was fabricated.

## Firearm result and limits

Pistol and other mundane descriptions use ordinary declarative English.
Blunderbuss distinguishes lead-ball attacks from Scatter Shot without suggesting
that Scatter Shot rolls an attack. Penetration distances still come from the
real policy. Reliable retains minimum 0 and natural-1 behavior; Seeking says it
ignores concealment miss chance while preserving sight, target eligibility and
other defenses. Names, localization keys, GUIDs, prices, enchantments and item
composition are unchanged. Acquisition changes belong only to Model D.

The guarded observer reads the actual `BlueprintItemWeapon.Description` getters
for Pistol, Blunderbuss and The Last Word. It verifies exact final strings,
spacing/punctuation and both complete property clauses exactly once. Source
scans are additional guards, not rendered-tooltip proof.

**HumanTooltipReview: NOT_PERFORMED.** Layout, wrapping and appearance require
the owner review listed below. All touched icon consumers retain their existing
approved assignments; no icon asset or player-visible identity was added.

## Vendor before/after ledger

All quantities below are per fixed table row unless the Better Vendors row
explicitly describes a native rank stock operation.

| Surface | Fixed Phase 2A before | Model D after |
|---|---|---|
| Oleg Gunslinger | none (old supply path cleanup only) | Pistol 1, Musket 1, Blunderbuss 1, Black Powder Charge 50, Lead Ball 50 |
| Capital Gunslinger | mundane three and +1 three, 1 each; powder/ball/cartridge 200 each; kit 1 | Pistol +1 1, Musket +1 1, Blunderbuss +1 1; powder/ball/cartridge 200 each; Gunsmith's Kit 1 |
| Bokken | powder/ball/cartridge 100 each; kit 1 | powder/ball/cartridge 100 each; kit 0; firearms 0 |
| Eastern generics | capital 12, Oleg 6, Dire Narlmarches 12, Pitax 12; all quantity 1 | capital 0; Oleg 6, Dire 12, Pitax 12 unchanged, all quantity 1 |
| Spear generics | capital 6, Oleg 4, Dire Narlmarches 6, Pitax 6; all quantity 1 | capital 0; Oleg 4, Dire 6, Pitax 6 unchanged, all quantity 1 |
| BTSL, each campaign/standalone pair | Honest Guy mundane/+1 firearms 1 each, Eastern 12 and spears 6 at 1 each; Xelliren powder/ball/cartridge 200 each and kit 1, no Eastern/spear rows | identical rows, quantities, DLC/profile behavior and cleanup |
| Better Vendors | 50 catalog entries: 30 firearm and 20 Eastern/spear; Military I/III/V adds tier +1/+2/+3 at 5 copies per matching item per stock operation; VII/IX adds +4/+5 at 2 each | identical catalog, schedule, quantities and ledger; no other rank is supplemented |
| Skeletal Salesman | Roadwarden and Dead Reckoning, 1 each on existing generated C3/C4 paths | identical |
| Named fixed loot | five firearm, 18 Eastern and six spear placements, one each; Cord fixed loot also unchanged | identical item/target/container/quest identities and counts |

Oleg gets no +1, cartridge, kit, named firearm or Better Vendors variant from
this publication. Capital loses all mundane firearms and mod-owned generic
Eastern/spear rows. Modeled capital concentration falls from 28 to 7 mod rows;
this is not a claim that every merchant carries only a handful of total rows.
All prices, classifications, enchantments, initial equipment, crafting costs,
recipes, named progression and non-capital publication paths remain unchanged.

The dedicated `OlegFirearmVendorBlueprints` publisher uses the Gunslinger gate
without Eastern, spear, Favored Class, Better Vendors or BTSL prerequisites.
Exact GUID/type/name resolution and reference identity replace no native stock.
`NormalizeOwned` builds detached rows before assigning the table, removes only
exact retired/incorrect owned rows, appends the expected shape once, preserves
native/foreign references and order, detects conflicts, checks round trips and
supports guarded exact rollback. Repeated initialization preserves the already
correct array; module-off normalization removes the corresponding owned rows.
Capital includes generic cleanup identities even if those modules are disabled.

Migration is ordinary initialization of blueprint/generated vendor tables,
including a table seeded with the old shape. No inventory, stash, equipment,
container item instance or raw save is traversed. Already purchased instances
remain. Already materialized merchant inventory is native persisted state;
this mission neither rewrites it nor promises an immediate refill.
**Stock can deplete. Native table regeneration is not guaranteed. No renewable
or infinite merchant supply is created.** Crafting remains separate.

The runtime observer reads Oleg/capital/Bokken/Dire/Pitax by exact identities,
checks retained regional rows, then exercises repeated real publication and
previous-shape normalization on detached native fixed-row components. Native
and unknown foreign controls survive, including exact rollback. No merchant UI,
input, save load or item-instance mutation is needed.

## Whiteout ledger and exact remaining prerequisite

Publication is **NONE**. Marker GUID: none. Buff GUID: none. Patch identity:
none. No FC trait/alternate trait/feat, automatic Undine grant, localization,
icon, setting or acquisition path exists. The foundation is inert pure code,
instance-owned state/replay models and explicitly guarded observers.

The frozen pure policy requires marker + Rain/Snow + ActualWeather Light through
Storm, with an explicitly supplied indoor-exclusion decision. It excludes clear,
Normal, fog-only spell effects, waterfall spray/VFX and unmarked units. Melee
and ranged share the same stage policy. Only ordinary concealment success can
reach the independent d100; 1-10 misses and 11-100 continues. Ordinary failure
short-circuits without another roll or miss mutation. Exhaustive 100 by 100
outcomes prove 20% then 10% gives **28%**, not 30%. Seeking and IgnoreConcealment
bypass it. Per-rule replay and per-owner reconciliation models are idempotent;
weather/area/marker removal and disabled/absent contracts leave no active state.
These domain proofs are not a native combat qualification.

Observed native scene round trip:

| Scene | Exact area | Indoor | Visual | Current / Actual | Proposed active |
|---|---|---|---|---|---|
| Mansion | `2849fdde28fe50f4d935bf2cf3405051` | true | Normal | Clear / Clear | false |
| Oleg | `ead426a6c23d39548a670ee515d77df4` | false | Rain, intensity 0 | Clear / Clear | false |
| Mansion return | same origin | true | Normal | Clear / Clear | false |

`BlueprintAreaPart.IsIndoor` and `LocalMapArea.IsIndoor(anchor.Position)` agreed.
The observer recorded two native unload/load pairs and zero weather-change
notifications, preserving that observation rather than synthesizing events.
Origin area, party references/positions, inventory identities/counts, money,
saved weather schedule and camera were restored; save-write evidence was zero.

The catalog then read 607 area parts and found eight precipitation overrides,
five marked indoor: `FinalDungeon` (`a39dc445480d9b9419aaa3f8df2d1ed1`),
`FinalDungeon2` (`f85f5e240e68b14438992554f5234d57`), `FinalDungeon3`
(`0bc7e9d236228564ba1f7f3da61d8e91`), `CultistsVillage`
(`201f4b2d57bc1314dae8350a62a3e189`) and `HouseAtTheEdgeOfTime_FB`
(`8c3d1882fd2e493409d5d3832553b55e`). The other candidates are the Varnhold
cutscene, capital owlbear attack and ShumblingmoundLairFW. Native scene code
has an indoor OverrideWeather exception. Configured metadata is **not** proof
of loaded-scene active weather, so the clear mansion cannot justify a universal
indoor-clear rule. No indoor gate was chosen.

**Blocker:** a qualified reversible native precipitation fixture and a decisive
active-weather indoor/outdoor observation are absent. The safe working-save
round trip remains Clear; the other authored destinations are scripted/late or
unqualified. Existing runtime code has no qualified weather setter/injector.
Forcing shared weather, advancing campaign time, or loading an unqualified
scripted destination would replace the missing prerequisite with an unproven
fixture. Mission 11.4 explicitly requires stopping at pure policy plus observation
when indoor evidence is ambiguous. The broader native binding is therefore
blocked; this is not proof that a future narrow fixture/subsystem is impossible.

The exact private native method
`RuleAttackRoll.TryOvercomeTargetConcealmentAndMissChance(): bool` exists with
zero parameters and 149 IL bytes on native MVID
`07fa1e4d-8618-41b3-9b8d-faa17d3b26f7`. Its existence and ordering were inspected
beside the qualified Seeking patch; a missing patch point is **not** the blocker.
No patch was installed and no global RNG/native concealment tier changed.
No two-process active-weather combat PASS is claimed or required for this
policy-only result. Future binding still requires that full two-process gate.

## Validation commands and outcomes

The fixed baseline had **1,975** actual registered domain tests. Four firearm,
26 vendor and 35 Whiteout cases yield **2,040**. Only active count pins were
updated; no historic checkpoint was rewritten as a current total.

Commands ran from this worktree with `powershell.exe -NoProfile -ExecutionPolicy
Bypass -File` for PowerShell entry points:

```powershell
scripts/Invoke-KmgGate.ps1 -Level Focused -Filter 'firearm-descriptions;midgame;model-d;paper-cartridge;unified-repair;whiteout-policy'
scripts/Test-RuntimeScenarioPreflight.ps1
scripts/Invoke-KmgGate.ps1 -Level Sprint
```

- Final focused selection: **78/78 PASS**, 2,040 registered.
- Request/WhatIf preflight: **480 PASS**, plus teleportation metadata check;
  includes exact save, exit, parameter and no-deployment negative guards.
- Sprint gate: **PASS: 2,040/2,040 tests, repository validation, clean Release,
  deterministic package and strict standalone validation**. The current gate delegates exactly once to
  `Build-Local.ps1`: repository wrapper, complete unfiltered domain suite,
  clean exact-reference Release build, output validation, deterministic package
  creation and strict standalone UMM package validation. It launches no game.
- Changed PowerShell files parsed with
  `[System.Management.Automation.Language.Parser]::ParseFile`; changed Python
  validators parsed with `ast.parse` (no pycache): PASS.
- The active gate's deterministic packager/asset/file-count checks pass. The
  standard and local-runtime ZIPs are byte-identical (SHA256 comparison). No
  release tag, version change or release publication was performed.

Final integrated runtime commands used the approved owned-lease deployment:

```powershell
scripts/Deploy-Local.ps1 -PackagePath <qualified-package> -RuntimeLease <owned-lease> -PassThru -Confirm:$false
scripts/Invoke-KingmakerRuntimeTest.ps1 -Scenario <scenario> -ExpectedVersion 0.0.141 -TimeoutSeconds 900 -ExitAfterCompletion:$true -AllowDirtyGit -ReuseInstalledArtifact -PackagePath <qualified-package> -DeploymentManifestPath <verified-receipt> -RuntimeLease <owned-lease> -Confirm:$false
scripts/Restore-Live-Mod.ps1 -BackupDirectory <that-receipt.backupDirectory> -RuntimeLease <owned-lease> -Confirm:$false
```

`<scenario>` is each row below. Only working-save smoke and weather observation
also supply `-SaveName KMG_AUTOMATION_WORKING`; only weather observation supplies
`-CompletionTimeoutSeconds 600`. These commands ran through Steam App ID 640820
with the normal user token, no direct executable launch and no UI automation.
The batch never kills a process and waits for each correlated automatic exit.

| Scenario | Evidence directory | Assertions | Fresh process / exit |
|---|---|---|---|
| `observe-firearm-descriptions` | `20261005T0607124215190Z-observe-firearm-descriptions` | PASS 8/8 | 16300 / automatic |
| `observe-model-d-vendors` | `20261005T0608280224235Z-observe-model-d-vendors` | PASS 26/26 | 16952 / automatic |
| `observe-whiteout-weather-catalog` | `20261005T0609391900181Z-observe-whiteout-weather-catalog` | PASS 2/2 | 15696 / automatic |
| `working-save-smoke` | `20261005T0610486099427Z-working-save-smoke` | PASS 11/11 | 23560 / automatic |
| `observe-whiteout-weather` | `20261005T0612214416560Z-observe-whiteout-weather` | PASS 6/6 | 21500 / automatic |

All five runs used DLL SHA256 `e0895e76e2d756431a6041f5f0202016eabbf2d67ac028b2abcc67ae9043416a`,
MVID `7ae9fd3c-d00d-4d24-8841-2c7fc0828435`, package SHA256
`2475d4ec04d2a3208267934e1a4d946c8ac54b5853b2abe8d952802e1476eaa8` and source-state fingerprint
`5cb1c7af359d9d9d018bef085a699ecf4a514181bad488938125f614a1b9e2eb`. The standard and local-runtime packages both match
that ZIP hash. All five process IDs and start times are independently recorded.
The two save-backed runs observed zero save writes. Live deployment backup
`20261005T0607037733245Z` was restored and verified exactly after the
last automatic exit; the owned lease was released. The final batch reports
`status=PASS`, `restored=true`, finishing at `2026-10-05T06:14:12.5968740Z`.

The final runtime candidate embeds implementation HEAD `d4afa40947741c2f8c7a15024375ad4618550a9b`.
`-AllowDirtyGit` is explicit because six reconciled documentation files were
pending; the exact source fingerprint binds the entire candidate. No production,
test or script source changed after that implementation commit. The final commit
adds documentation/evidence only and is not misrepresented as the embedded SHA.
Raw logs, receipts, packages and private inspection output remain untracked in
authorized evidence/artifact locations.

## Checkpoint evidence and meaningful failure history

| Checkpoint | Full gate count | Guarded evidence directory | Result |
|---|---:|---|---|
| fixed-base research | 1,975 | no runtime for documentation only | PASS |
| firearm | 1,979 | `20261005T0454491229974Z-observe-firearm-descriptions` | PASS 8/8 |
| vendors | 2,005 | `20261005T0522299980364Z-observe-model-d-vendors` | PASS 26/26 |
| first weather observer | 2,039 | `20261005T0536125560594Z-observe-whiteout-weather` | FAIL 5/6; overstrict event expectation, restoration PASS |
| corrected weather observer | 2,039 | `20261005T0540413348351Z-observe-whiteout-weather` | PASS 6/6 |
| weather catalog | 2,040 | `20261005T0556341614778Z-observe-whiteout-weather-catalog` | PASS 2/2 |

All successful checkpoints were committed and immediately pushed through the
approved wrapper. Exact earlier DLL / MVID / package identities:

- Firearm: `386675e6017377dc3608561aabb9e57f373366ef964d8efa632e9969983ec8a4` /
  `63b1456f-402d-48cb-b18f-d0ba148ada44` /
  `080e1faefb4fcbcc3c705c856f5fbc004c4f8f7e3bbdb417fcf9eeb8e832fa4f`.
- Vendor: `ace366cd36cdd625a6d052b7d23ebf043b2f7710484b83b0ca4ab2580f3b85fa` /
  `15c79d68-b35b-4691-8990-8375b5ba44cc` /
  `feb551e65183ef5082ece3b9d90e1cc2c7df0ba3d61facf2f59567703ea7f3df`.
- Weather PASS: `6a09eea3944a444a1007fc9c50a8297f74332e505ff8a5e97072ca9595681495` /
  `da93a2fc-a3ba-4ed8-a2b7-337ec4769e00` /
  `53eed3d5dc83c54cb4343b8b8f9a6e3ad39cda41e6d5a3fe37ecb524b39c2144`.
- Catalog: `d0c9c0cbc9033292e60baa99dcd4620c3583cb850e763299639cb786325f1b5a` /
  `c5375964-8425-43c6-8aa8-da2cad96346c` /
  `f0884a97481f4e4b2cdbc9a4d335f4e4005b4698cbe7844dc6576fcf86ac3ec7`.

Exact verified successful live backup restorations were
`20261005T0454403947334Z`, `20261005T0522221476379Z`,
`20261005T0540340565882Z` and `20261005T0556256463280Z`; the first weather
FAIL also restored its own deployment. Each owned runtime lease was released.

Meaningful failures were retained and repaired, not replaced with a clean story:

1. The fresh baseline had two stale current-file icon catalog hashes. Git object
   equality proved unchanged blueprint/icon manifests; only current pins were
   reconciled. Ignored `GamePath.props` initially lacked KingmakerManagedDir,
   causing two missing-reference compilation failures; the required local IL
   artifact was also absent, causing two domain failures. Existing installed
   SDK tooling generated it locally; no proprietary output was committed.
2. Preflight had stale positive version literals and five missing Phase 2A
   scenarios. Positives now derive Info.version; legacy/negative guards remain.
3. The tool's elevated token was rejected before runtime. Hidden desktop-shell
   launch supplied the existing normal user token to unchanged guarded scripts.
   No OS setting or runtime guard was bypassed.
4. Model D invalidated old stock/name/source guards. They were updated to the
   exact new contract. One Windows rewrite broke LF-specific assertions; UTF-8/LF
   was restored without weakening the tests. One generated Python cache was
   removed by its exact task-owned path before the passing validator.
5. The icon guard rejected bootstrap wiring changes. Automatic approval review
   rejected a broad generated integrity-hunk proposal; a read-only whole-file
   reversal proof reduced it to exactly five literal vendor-only substitutions.
   That narrower ledger was accepted. Original art/assignment hashes and earlier
   protections remain intact; the guide/catalog record protected-existing consumers.
6. The first weather run wrongly required a weather event while ActualWeather
   stayed Clear. Native area events and weather counts are now recorded honestly;
   no synthetic weather event was introduced. Both attempts restored game/live state.
7. The catalog's first Release build found an array/List adapter mismatch;
   `ToList()` repaired it before deployment, then the full gate and runtime passed.
8. Final batch attempt one failed preflight's artifact immutability check because
   its own live transcript changed. The delta identified only that log. Preflight
   was rerun independently (480 PASS), then batch two reused unchanged source.
   The first attempt never deployed or launched a game.

Local batch logs are under `artifacts/mission/`; final records are
`final-integrated-batch-2.log` and `.json`. Expected negative-test exceptions in
transcripts are separate from these real failures. The older weekend runs do
not qualify this branch.

## Safety and publication audit

PASS: exact base ancestry, no merge commits, protected baseline surfaces
unchanged, text-only scoped changes, no untracked or generated additions, and
no credential/private/proprietary or unexpected machine-path findings. Final
staged/clean-tree verification is performed by the same audit and push wrapper.

The ad hoc exact-scope audit command was
`python -X utf8 artifacts/mission/Audit-Followup.py`. It checks branch/origin,
base ancestry/no merges, protected unchanged surfaces, text-only changes,
generated/private/binary/credential patterns, untracked files and new absolute
paths. The only new machine paths are the explicitly required worktree path and
approved push-wrapper command. `git diff --check`, staged diff checks, full
tracked/untracked review and manual scope review passed. No raw save, assembly,
package, runtime log, credential or machine-local configuration is tracked.
Ignored local GamePath/build/test/inspection artifacts remain available locally.

After every coherent commit and for final closeout, the only push command was:

```powershell
powershell.exe -NoProfile -ExecutionPolicy Bypass -File C:/Dev/KingmakerGunslingerLab/codex-policy/Push-KingmakerGunslinger.ps1
```

The wrapper requires a clean worktree, checks repository/branch scope, unfinished
Git operations, generated/private artifacts and credentials, permits only a
fast-forward, and verifies the remote SHA equals HEAD. No raw push was used.
Final closeout requires and verifies empty `git status --porcelain=v1
--untracked-files=all`; the wrapper refuses publication otherwise. The final
chat gives the remote-verified documentation HEAD, whose tree contains this
report and no uncommitted source changes.

## Human review list

1. Manual in-game tooltip appearance for Pistol, Blunderbuss and The Last Word.
2. Later original icon authoring for Fiery Glare and Stoic Dignity.
3. Later visible Whiteout publication review, after its missing native-weather
   prerequisite and full combat qualification are resolved.
4. Later integration strategy with the active Phase 2B stack.

## Integration forecast - no integration performed

Read-only comparison used Phase 2B `a762ece553ab9b539a53a782102a29e5c70d44eb`
against the fixed Phase 2A base. That moving branch must be rechecked at actual
integration time; neither a merge nor a cherry-pick was performed here.

| File/surface | Evidence-based forecast |
|---|---|
| `tests/KingmakerGunslinger.DomainTests/Program.cs`, both project files | Both branches append tests/compile entries. Retain both sets and derive the actual suite count; never select either branch's hard-coded total blindly |
| `tools/validate_expanded_summoning_phase2a141.py`, `validation/static-validation.json` | Both change active counts/qualification records. Recompute integrated active pins and preserve historical provenance |
| `RuntimeTestRunner.cs` | Both add guarded scenario routing/helpers. Preserve all exact request/save/exit guards; keep scenario IDs unique |
| `RuntimeTestScenarioCatalog.cs`, `RuntimeTestRequest.cs`, `RuntimeAutomation.Common.ps1`, preflight list | This branch adds four observer scenarios. No Phase 2B edit to these catalog/metadata files was seen at the compared SHA, but future scenario work can overlap |
| `BetterVendorsProgressionTests.cs` | Phase 2B appends Sprint 14/15/16 manifest identities; this branch updates Oleg stock guards. Preserve both independent contracts |
| `PaperCartridgeFoundationTests.cs` | Phase 2B changes package counts from 289/287 to 305/303 and extends accepted package counts; this branch changes Oleg/vendor assertions. Preserve current packaging and Model D expectations |
| `assets-source/original-icons/icon-catalog.json` | Phase 2B adds art/manifest changes; this branch repairs two stale current pins and adds five exact authorized vendor wiring hunks. Recompute current-file hashes against combined bytes without erasing protected baselines |
| build/package scripts and `create_deterministic_package.py` | Phase 2B changes asset/package counts. This mission leaves these production scripts unchanged; retain the new Summoning assets and rerun strict package checks |
| production vendor files | No overlapping Phase 2B production vendor change was present in the inspected committed diff. Recheck its later head before integration; preserve all exact retained stock and native/foreign ownership boundaries |
| README/acquisition/placement docs | Carry Model D's current rows and finite-stock language without turning historical placement/runtime reports into new qualification |

Whiteout contributes no registered identities, serialized state, icons or
production attack patch, reducing current integration risk. Future native
binding/publication is separate work, not an invitation to enable it during
integration. Integration must rerun the combined suite, package checks and
applicable fresh guarded runtime scenarios; this branch's results alone do not
qualify the combined stack.
