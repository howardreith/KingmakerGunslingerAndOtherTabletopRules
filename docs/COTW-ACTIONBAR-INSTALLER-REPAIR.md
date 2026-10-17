# CotW action-bar installer correction — output_log2, 2026-10-17

## Frozen local candidate / handoff

Final product `39c60fb5e9167f1b2fe35a07b545fa2a2ebaac2e` includes the installer
checkpoint `132398b46e4f` plus the separately reviewable readiness correction.
Product/source branch `codex/cotw-actionbar-installer-20261017` is guarded-pushed;
its original worktree remains clean/frozen. Subsequent records live in
`codex/cotw-actionbar-installer-review-20261017` / the separate worktree of that
name. No published alpha or previous receipt was replaced.

Local test package and adjacent development receipt/checksum:
`C:/Dev/KingmakerBuffPlannerLab/handoff/KingmakerGunslinger-0.0.157-development-cotw-actionbar-installer-39c60fb5e916.zip`.
UMM remains 0.0.157; candidateKind=development, releaseQualification=false.

- ZIP SHA-256: `6d4ed736a18cfdcac668eb5fad24e6b19001af635fd7e1fc53006834bd507aed`.
- DLL SHA-256: `d9fea590a604b81b5a14046ac88454a7eb8d4510e94e0b4ae5de082cbc2d606b`.
- DLL MVID: `c1a4d16b-efae-4ea8-b3ef-5966c3db9c6b`.
- Build/package **65.746 s**: development wrapper, 33/33 selected domain tests,
  exact-reference C# 7.3 / .NET 4.7 compilation, 37 native-contract companion
  assertions, strict package PASS. No complete 2,652-test suite or live run.
- Exact packaged DLL then passed **64/64** installer/callback assertions in
  **0.977 s**. This result includes the real Install route missing from the
  earlier 39-assertion evidence. It is still headless, not a saved-game PASS.
- Command: `Build-Local.ps1 -DevelopmentPurpose cotw-actionbar-installer
  -DevelopmentTestFilter 'brown-fur.startup,shield-other,teleportation.oracle,eastern-favored,favored-class.host-readiness'
  -ReferenceBundleDir C:/Dev/KingmakerGunslingerLab/private/refresh-2026-10-15/extracted/KingmakerGunslinger-private-build-references`;
  then `Test-CotwSpontaneousSlotCompatibility.ps1` on the produced DLL.
- Private logs in the frozen worktree: `artifacts/installer-inspection/candidate-build.log`,
  `candidate-regressions.log`, prior failing/passing regressions and the exact
  copied input under `input/output_log2.private.txt`. Package receipts remain
  under `artifacts/development-candidates/cotw-actionbar-installer/39c60fb5e916/`.

The next test must first confirm `cotw-actionbar/installed` and the new assembly
hash/MVID, then load without saving over the affected save; record Continue/Load
Game and actual input mode. Check party/page refresh before KBP no-cast entry/exit.
No companion Buff Planner or CotW change is required for these KMG corrections.
Native load remains NOT_RUN, including the known guarded controller-entry gap
in the preceding record. No game launch, install/settings/save mutation or
transaction occurred; no restoration debt. Public alpha publication requires
a new explicit owner request, not inherited alpha 2 authority.

Isolated branch/worktree `codex/cotw-actionbar-installer-20261017`, based on
`e50db0da63d4052c1c3107e9fb38a21a5d2eb9d3`. Preserve published alpha 2,
the frozen `92b442c06dd8` candidate, and Brown-Fur's lifecycle repair. Current
request authorizes investigation/source repair/local packaging, not another
automatic public release, default-branch merge or permanent installation.

## Confirmed causal chain

Private supplied log `C:/Dev/KingmakerBuffPlannerLab/handoff/output_log2.txt`,
2,000,875 bytes, SHA-256
`18b473dd944a3da1772f3d859f790aad819c3cd3359394d7ed9ab8a70181481a`.

- Line 106: KBP 0.5.4-controller.1, product `9aec44e374b26b3561fa4402912c77a23a4e08ac`.
- Lines 241–243: native CotW prefix ownership was read successfully, then the
  new KMG installer diagnostic called Harmony12.GetPatchInfo on CotW's own
  unpatched prefix method. The installed shim's ToHarmony12 converter throws
  for null metadata. The outer installer catch reported contract.refused and
  never installed the correction. It was not an unsupported CotW signature.
- The stack identifies KMG MVID `0fa8ccba-349e-47e1-a2ec-8b46d20ed69c`, matching
  alpha 2 product `92b442c06dd8`, expected DLL SHA-256
  `e78d79cfb334f6d59c2948b6258128ce73334db59e8f7dba16a01860cc946801`.
  Its hash is not independently logged on that computer because assembly
  diagnostics had not run. Do not substitute MVID for an observed file hash.
- Lines 5279–5281: Brown-Fur registered all 25 expected identities/types at
  package-blueprint-library-ready and published its Arcanist archetype. This
  confirms that repair independently; do not revert it.
- Lines 9734–9755: unchanged CotW spontaneous conversion helper fails during
  controller ActionBarSlotVM creation and becomes LoadGameException. There
  was no cotw-actionbar/installed event or corrected slot observation before it.

Exact DATA Harmony12 facade matches the failing shim MVID
`918c071f-383e-46dc-a374-6879300cbe15`, SHA-256
`aa1cd48317254985d8b700cc74953477d1b40c3022ce9aa4c95ed2b8327e1292`, at
`C:/Program Files (x86)/Steam/steamapps/common/Pathfinder Kingmaker/Kingmaker_Data/Managed/UnityModManager/0Harmony12.dll`.
Private installed IL confirms GetPatchedMethods delegates to Harmony's GLOBAL
GetAllPatchedMethods; it does not filter away foreign owners.

## Correction and regression

The diagnostic now checks the global patched-method set before reading metadata;
an unpatched method reports none. Each optional patch/assembly inspection is
isolated from installation. Actual contract validation, patching/transpilation,
CotW conversion logic and load exceptions remain authoritative and are not
caught/suppressed by the diagnostic helper. Native patch composition stays intact.

The expanded headless regression invokes the real Install method with the real
Harmony/CotW/game assemblies. Only ModContext's external UMM logger sink and
native-object allocation are fixtures. The former tests invoked the production
transpiler directly and did not cover this installation seam.

- Exact alpha 2 baseline: expected FAIL at “real installer reaches patch
  completion”; 4 preceding assertions pass; native shim null-metadata NRE is
  reproduced. Log `artifacts/installer-inspection/alpha-2-installer-baseline.log`.
- Corrected working-tree DLL: 47/47 PASS, including real installation,
  idempotent install, missing metadata, foreign owner/postfix preservation and
  execution, a failing diagnostic read, plus all prior conversion/variant,
  ownership/book, empty/unbound/rebind and no-spend checks.
  Log `artifacts/installer-inspection/installer-fixed.log`.
- `Invoke-KmgGate.ps1 -Filter brown-fur.startup`: production compile PASS
  (7.5 s), 6/6 of 2,652 registered startup tests PASS (1.7 s).
- No new native load result or publication is established by these source tests.

## Separate bounded readiness correction

First-update callbacks run before the blueprint library is ready (lines 308–316):
class-catalog diagnostics access an unavailable root; Shield Other and
Teleportation consume their final-live callback before their owned identities
exist; Eastern Weapons initially refuses but later recovers through Aid Another
(line 5334); Favored Class attempts integration before its 200 leaves register.
These are distinct readiness issues, not the thrown exception aborting this load.
The five first-update consumers now keep their single callback pending until
KMG's actual blueprint bootstrap is committed (or the mod context has failed).
They check readiness only: no discovery, reflection, settings writes or graph
mutation on early frames. Favored Class's own host retry allowance is not spent
while waiting for KMG registration. After readiness, the existing validation,
transactional publication and genuine-failure paths are unchanged. This is a
separate reviewable change after installer checkpoint `132398b46e4f`.

Expanded actual-assembly probe: **64/64 PASS**. It drives all five production
FirstUpdate callbacks through an isolated UMM delegate, verifies retained single
subscription through three early updates, no consumed host retry, and exit from
pending when the committed-readiness flag is supplied. Native blueprint graphs
are deliberately absent in that final boundary test, so it reaches existing
validation/failure, NOT successful optional publication. Log
`artifacts/installer-inspection/readiness-transitions.log` (1.009 s process).
Affected-neighbor selector:
`Invoke-KmgGate.ps1 -Filter 'brown-fur.startup,shield-other,teleportation.oracle,eastern-favored,favored-class.host-readiness'`:
**33/33 of 2,652 PASS**, production compile 0.4 s / domain phase 1.0 s.
Log `artifacts/installer-inspection/affected-neighbors.log`. No full suite.

The four ZFavoredClass KeyNotFoundException errors also occur in the previous
supplied output_log.txt, in the same loadCustomFeature/loadPrestigiousSpellCaster
methods with matching ZFavoredClass MVID `3efd38e7-8682-4b4d-8d53-e368a3664919`.
They concern custom/PrestigiousSpellcaster data and loading continues past them
to controller UI. This classifies a pre-existing installation issue, not a newly
introduced installer regression or proof of harmlessness. Their exact missing
asset IDs are not in these stacks. No external mod/data or save was modified;
further repair needs those actual records/IDs rather than guesses.

Native controller save loading, page/party refresh and KBP no-cast entry/exit
remain unqualified. No game process, installation/settings/save mutation or
runtime transaction has been started during this correction. Exact candidate
identity and any later focused readiness changes are recorded separately.
