# CotW action-bar installer correction — output_log2, 2026-10-17

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
