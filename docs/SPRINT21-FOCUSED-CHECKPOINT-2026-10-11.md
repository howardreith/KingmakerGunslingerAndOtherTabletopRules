# Sprint 21 focused takeover checkpoint - October 11, 2026

Status: REVIEWABLE CHECKPOINT. Compatibility and stable release qualification
remain incomplete. No release, master merge or Sprint 22 is authorized by this pass.

## Intake and effective instructions

Host: HOWARD-LAPTOP. Git located the active worktree at
`C:/Dev/KingmakerGunslingerLab/repo/KingmakerGunslinger`, on
`codex/expanded-summoning-sprint21-bebelith-giant-crab` (draft PR #36).
Origin is `git@github.com:howardreith/KingmakerGunslingerAndOtherTabletopRules.git`.
Intake HEAD and remote feature tip were both
851f2a4dcc9cae23329d5e8d1cdbdc4c2a5b6e8d; its ancestor
20efcd71d5b9d0ac67ca872867fc08a8679dad4d contained the compiled-only census correction.
No uncommitted or unpushed source was present. Git worktree inspection did not
require changing another worktree. Relevant process inspection found the current
Codex process and application daemons, no Claude, Kingmaker, compiler or test runner.
No active compatibility transaction or lock existed.

CODEX_HOME is `C:/Dev/KingmakerBuffPlannerLab/codex-home`; its AGENTS.md and
AGENTS.override.md are absent. Its configuration specifies no instruction/fallback
paths. No ancestor or nested AGENTS.override.md masks this worktree. `C:/AGENTS.md`
and the old root AGENTS.md contained a full-suite cadence. The owner's October 11
instruction and imported root AGENTS.md / docs/VALIDATION-POLICY.md control this
pass. Global instructions and approval/sandbox settings were not modified. The
already-running session's injected old instructions were superseded by the owner's
explicit task; a subsequent session must start freshly in this worktree.

Fetched master is a756a498028313e3c95f93882580212080953b0d, merging PR #37's
42b9644c0f6a71cec9f58dda30fd5a1981acbd3b. The older bfb9192f implementation was
not cherry-picked. Both intake tips have local safety refs:
refs/safety/20261011-sprint21-codex-intake and
refs/safety/20261011-sprint21-workflow-master. The authorized merge into the feature
branch resolved four conflicts: handoff, Build-Local, package, and matrix. The
imported instructions were explicitly reread before selecting checks.

Sprint 21 bodies, identities, version 0.0.150 and packaging contents are preserved.
Build-Local resolves its version directory from Info.json and delegates package
assembly/strict validation once. The matrix resolves the version dynamically,
keeps reports outside source, and uses fresh per-profile deployment receipts.
The publisher fixture now tests this branch's 0.0.150 / sprint21 asset identity.
Current task metadata selects Sprint 21 verification, rather than the imported
historical workflow-repair task. Claude's original handoff/authorship is retained
in docs/history and Git; Codex work has no invented Claude co-author/footer.

GitHub independently confirmed PR #36 OPEN/draft/CONFLICTING at intake, PR #37
MERGED, and alpha prerelease metadata. The immutable alpha tag peels to
adddef438c9bc5798d462d95f6667671ca852344. Its ZIP is
KingmakerGunslinger-0.0.150-expanded-summoning-sprint21.zip, SHA-256
e18244e100b7598ccf8c348b7c413da727f63073fad11834fafbd6053feebc06. No tag/asset changed.

## Focused source verification

20efcd71 agrees with ConfigureBebelith: primary native bite, two additional owned
2d4 claws, no secondary limbs. The observer's bite check is strengthened to read
the actual BlueprintItemWeapon's dice and GUID; correct GUID with wrong dice,
or correct dice with wrong GUID, fails. No creature gameplay changed.

Raw logs are outside source at
`C:/Dev/KingmakerGunslingerLab/runtime-evidence/sprint21-focused-takeover-20261011-codex`.
The selected checkpoint has one aggregate 15-minute planning target / 30-minute
validation ceiling. Necessary safe completion/restoration is excluded.

| Check | Command / selection | Evidence |
| --- | --- | --- |
| PowerShell parse | Parser.ParseFile over the 11 changed scripts | PASS |
| Policy, filter and refusal / child calls | powershell.exe -NoProfile -ExecutionPolicy Bypass -File scripts/Test-KmgGate.ps1 | 19 gate assertions plus DevelopmentGate 8/8; 2.96s |
| Candidate reuse / transactions | powershell.exe -NoProfile -ExecutionPolicy Bypass -File scripts/Test-CompatibilityCandidateReuse.ps1 | 6/6; 2.96s; one simulated build, seven deployments, 19 reuse calls; offline |
| Existing runtime orchestration decisions | powershell.exe -NoProfile -ExecutionPolicy Bypass -File scripts/Test-ExpandedSummoningRuntimeOrchestration.ps1 -ScriptRoot C:/Dev/KingmakerGunslingerLab/repo/KingmakerGunslinger/scripts | 168 assertions; 1.65s; offline |
| Private references | Assert-KmgReferenceBundleMatchesInstall | All 14 hashes match the actual Steam installation, including PhysicsModule / Harmony12 / UMM; raw provenance retained locally |
| Focused gate | scripts/Invoke-KmgGate.ps1 -Filter 'expanded-summoning.release143-inventory;expanded-summoning.sprint21;expanded-summoning.compatibility-' -TimingPath '<log-root>/gate-timings.jsonl' | Production compilation 15.9s; 28/28 selected domain tests; gate 21.11s |

The first orchestration invocation without explicit ScriptRoot failed parameter
binding; the supported explicit-root invocation passed. The first production
compile of the added dice check failed because PrimaryHand is a base hand-item
type. A guarded BlueprintItemWeapon cast corrected it; failed logs are preserved.
No unchanged batch was restarted and no expected test count was edited.

Runner build used MSBuild from
`C:/Program Files (x86)/Microsoft Visual Studio/18/BuildTools/MSBuild/Current/Bin/MSBuild.exe`,
the domain csproj, /t:Build /p:Configuration=Release /p:Platform=AnyCPU; build and
runner exited zero. Executable:
`artifacts/tests/Release/KingmakerGunslinger.DomainTests/KingmakerGunslinger.DomainTests.exe`.
Modification time: 2026-10-11T16:27:33.5762315Z. SHA-256:
18ffc8fc0741c4f714972512b08cab01921027c6541114c3b15503150eb8dac0.
It reports 28 executed / 2559 registered; the new named bite test executed.
The earlier false 2558/2558 remains corrected history. Focused compilation used
the resolved merge tree over intake HEAD; this does not claim execution of the
unchanged intake commit or the frozen alpha.

Native confirmation is pending the single validated candidate and guarded
gunslinger-only inventory invocation. Estimated candidate plus scenario and
restoration: 6-10 minutes, within the selected checkpoint ceiling. This necessary
package preparation is not selection of the full release campaign.

Integration checkpoint 5541ac07217cab05f95320d87669c9d4af433726 was pushed through
the owner helper. GitHub subsequently reported PR #36 OPEN/draft/MERGEABLE/CLEAN.
The first package-preparation invocation stopped at source validation after
32.71s, before domain execution, compilation, packaging, deployment or launch.
It exposed two inherited validation assumptions during the imported workflow:

- The alpha record's 2558 test registrations were being compared to today's
  runner. Its original record remains unchanged. The validator now binds that
  count to adddef43's immutable metadata and source, requires every qualified
  registration to remain present, and independently reports the current 2559.
  All 28 Sprint 21 validator tests passed in 22.90s, including positive and
  negative provenance cases.
- The inherited package check demanded count logic in both Build-Local and
  package.ps1. It now follows the explicit output-bound delegation to the
  authoritative package owner, preserving the 354/352 inventory and strict
  validation. Three focused positive/negative tests passed; output-binding loss
  and package-count drift remain failures.

The corrected Sprint 21 source validator passes with all 200 protected master
files and 391 protected summoning files still enforced. No released record,
package count, alpha asset or current runner expectation was changed to obtain
a pass. Failed preparation/validator logs remain alongside the corrected ones.

The actual current installation was independently snapshotted before package
preparation: version 0.0.117, 136 Gunslinger files, tree SHA-256
216A9DC2B8E95CD644BA3CADC69A638463C25E60F40A11F8D4B2065C69D5AAF3. These newly
observed values happen to match the handoff; the full current Mods manifest and
current settings presence/hashes, rather than the historical pin, govern restore.

## Boundary classification

Common current environment B: HOWARD-LAPTOP, gunslinger-only transaction, staged
mod surface KingmakerGunslinger 0.0.150 only; CotW absent. UMM/Harmony are the
installed runtime. The raw request enables Gunslinger, Acadamae, Shield Other,
spears, eastern weapons, Urban Barbarian, Bodyguard, protection, elemental races,
teleportation and Magic Circle; Brown-Fur is false. Expanded Summoning differs
only ON/OFF. Four BTSL tables are loaded, with firearm rows 20 and eastern BTSL
rows 24; Steam DLC entitlement itself was not captured in these observations.

Current raw results: 20261011T1232096308556Z-observe-feature-module-settings and
20261011T1238223138300Z-observe-feature-module-settings, both 40/47 FAIL on
adddef43, loaded DLL 452a74b09a216fbadbab30caae45328f4957b965e68cff35e8e7d8e4c41ab933.
Historical H: 20260927T1030110774186Z-observe-feature-module-settings at v0.0.140,
78a5172399973dc0963d5dfa4000e94fcd42f7b7, module Brown-Fur false / summoning false.
H is historical occurrence evidence, not a matched v0.0.149 comparison. No stored
v0.0.149 feature-module result was located. Optional-mod/provider/DLC equivalence
between H and B is not established.

| Assertion | Expected / observed | Exact environment | Baseline evidence | Changed dependency | Classification | Next action |
| --- | --- | --- | --- | --- | --- | --- |
| feature-module-brown-fur-publication-gate | Available 25-identity CotW contract, unpublished / absent contract, 0 identities, 0 selector refs | B; Brown-Fur false, CotW absent | Same unavailable/0 result in H | Brown-Fur coordinator and predicate unchanged from released 0.0.149; shared initialization still considered | Fixture/expectation defect: predicate unconditionally requires Available despite the optional coordinator explicitly leaving absent-CotW profiles unavailable/unregistered; historically failed too | Separate provider-absent and provider-present expectations with positive/negative coverage; no waiver or unrelated production repair here |
| teleportation-scroll-template-2c283c993b233df53b487fc7eeac2ba3 | Full economics/teaching/isolation/consumption conjunction / FAIL; donor 02086fbbda266ed4b8e9124abe5abd75, primary present | B; teleportation true | Same assertion/donor failed in H | Scroll builder and publication observer unchanged from 0.0.149 | Demonstrated pre-existing failed assertion; product versus observer cause unresolved | Narrow instrumentation of the failing clause; existing raw report does not contain every conjunct |
| teleportation-scroll-template-2a2b0185be1d2dba5aa1b5a24774e17d | Same full contract / FAIL; fallback 013c0f5972c1b794b869b284ba426542, primary absent | B; teleportation true | Same assertion/fallback failed in H | Same unchanged direct dependencies | Demonstrated pre-existing failed assertion; product versus observer cause unresolved | Same narrow clause observation; do not infer a PASS from donor identity |
| teleportation-scroll-template-42d3daeb7d503687df8953b47727372b | Same full contract / FAIL; fallback 0437d7a2ea4b01542907c4d5fb12c4da, primary absent | B; teleportation true | Same assertion/fallback failed in H | Same unchanged direct dependencies | Demonstrated pre-existing failed assertion; product versus observer cause unresolved | Same narrow clause observation; preserve failed historical verdict |
| feature-module-gunslinger-publication | Capital stock 10 / 7; BTSL 20 / expected 20 | B; Gunslinger true; four BTSL tables | H capital 10; no matched released149 observation | Stock/loot production unchanged from 0.0.149; profile/initialization/observer attribution unresolved | Unresolved | One matched v0.0.149 gunslinger-only baseline, same DLC/module/mod/reference state and exact observation |
| feature-module-eastern-weapons-publication-gate | Campaign plus installed BTSL 66 / 54; eastern BTSL 24 | B; eastern true; four BTSL tables | H 66; no matched released149 observation | Eastern vendor production unchanged from 0.0.149; shared lifecycle not exonerated | Unresolved | Same single baseline comparison; inspect campaign rows, not assumed missing DLC |
| feature-module-elven-branched-spears-publication-gate | Campaign plus installed Honest Guy BTSL 34 / 28 | B; spears true; four BTSL tables | H 34; no matched released149 observation | Spear vendor production unchanged from 0.0.149; shared lifecycle not exonerated | Unresolved | Same single baseline comparison; inspect campaign rows and actual provider state |

ON/OFF symmetry does not establish innocence. No assertion was removed, historical
FAIL converted to PASS, or owner release-risk acceptance invented. No balance,
Brown-Fur, vendor or teleportation production repair was performed.

## Evidence impact and next boundary

Reusable as original observations, subject to final dependency review: the
Sprint 21 review/publication 44/44 runs; 1063 paths / 29 wrappers and Sprint
18/19/20 regression counts 29/39/22 on 7b37df94; persistence 125/125 on cf4bbc4a;
module-disabled 11/11 on 6b1ef853. Their candidate identities remain distinct.
Reader-only reinterpretation requires sufficient immutable raw observations and
both old/new reader verdicts; missing conjunct values cannot be reconstructed.

Affected rerun: compiled inventory observer, including the pending layout
correction and real bite-dice check, on the new candidate in gunslinger-only.
Workflow child-call/selection/receipt changes received the focused offline checks.
No production/shared creature lifecycle change was made by this takeover.

Not yet run: affected native inventory, high-risk-combined, remaining compatibility
profiles and disposable mechanical case, matched released149 boundary comparison,
final candidate closure and stable release. The full matrix is not this pass.

The next release-qualification action is one matched v0.0.149 gunslinger-only
observe-feature-module-settings comparison for the disputed vendor condition,
with the same current machine, DLC availability, optional-mod versions, module
state and fixture. Separately plan 10-20 minutes including artifact validation,
transaction and exact restoration; declare/freeze its baseline inputs first.
If it demonstrates a real baseline product failure, record the smallest explicit
release-risk decision instead of repairing an unrelated system in this handoff.
