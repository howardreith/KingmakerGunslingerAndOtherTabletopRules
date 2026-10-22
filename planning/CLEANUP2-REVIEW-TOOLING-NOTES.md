# Cleanup 2: review tooling and source-choice design

2026-10-22 independent lane and root integration update. This lane performed no
game/native launch, build, save write, Git or upload. The root integrator owns
the CurrentStack save/installation guards and completed the compilation,
focused validation and safety commit recorded below.

## Generated Working-save review roster

The existing `ResolveCreatureReviewVariants` looked up every key in
`ExpandedSummoningCatalog.All`, then selected its first-tier One variant,
preferring Nature's Ally when available. Frost is absent from that catalog:
advertising it in the CurrentStack wrapper could pass preflight and fail after
entering the actual Working-save scenario. Adding a generated Frost creature
would change the product model and is unnecessary.

New `Cleanup2CreatureReviewRoster` provides the exact 14-key mission scope,
whole-batch validation and the shared actual generated-variant selection.
It uses the existing catalog objects, families, tiers and multiplicity; it does
not introduce alternate blueprints or a text-only roster resolver. The native
observer now delegates its existing selection block to
`ResolveGeneratedVariant(key, quantity)` while retaining its surrounding crowd
and publication checks. It intentionally continues to resolve ordinary native
observer keys beyond this mission scope where the existing caller allows them;
`ValidateWorkingSaveRequest` is the narrower mission request authority.

The integrated CurrentStack planner removes `frost-giant` from its explicit
`$allowed` list and rejects an exact Frost anywhere in the incoming batch before
package/install/save entry, with this destination:

> `disposable-expanded-summoning-cleanup2-native-review`,
> `reviewPhase=giant-lineup`, existing SNA VII and SM VIII retained Frost routes.

That destination already resolves Frost in `Cleanup2GiantRoute`; it is a
save-free fixture, not this generated Working-save observer. No new debug spawn,
save selection, implicit reroute or public parameter was added here.

The new domain cases execute the shared selection for every advertised One
key, assert actual catalog object/family/tier, require publication and reject
Frost, mixed batches, unknown/case/whitespace/duplicate/null keys and unsupported
quantity. The wrapper advertises only One; this does not newly admit crowd
quantities. Literal wrapper-set inspection supplements actual resolver calls.
The CurrentStack behavioral suite also extracts the advertised wrapper roster
through PowerShell AST, exercises each of its 14 actual plans and the whole
batch, and proves exact/mixed Frost rejection before any package read,
installation/save transaction or launch entry.

Root integrated the source and guard repairs in safety commit
`7fb9ac54a251621e3e7ae8d4ba88754d7ee8d45e`. **Production compilation PASS;
two focused domain cases PASS (2/2).** The named domain filter is
`expanded-summoning.cleanup2-currentstack-roster`; this update records root's
completed results, not a new execution by this documentation lane.

### Original-process protection and completed safety checks

After the bounded ordinary native-exit wait fails, the **same original wrapper
process** enters `Wait-KmgSummonCleanup2ProtectedExit`. It waits without an
automatic deadline until `Assert-KmgSummonCleanup2NativeExit` confirms absence
through the precise native process query. Unknown/error is not absence. It retains the
live protected-save handles and exclusive transaction lease during that wait;
settings/installation restoration does not run while the game remains present.
The guard records its owner PID and waiting state. A waiting-status evidence IO
failure is caught and retained as `lastEvidenceError`; it cannot unwind the
owner, release its handles or permit early restoration.

Only confirmed absence allows lease-stream release and independent
settings/save verification and installation restoration; protected-save handle
disposal follows that boundary. Successful restoration does not turn
the original native failure into PASS: the record remains FAILED and the
original failure is rethrown, producing a nonzero wrapper exit. A restoration
failure also remains a failure, with its diagnostic retained.

Root reported these completed no-game checks on the integrated bytes:

| Check | Host | Result | Duration |
| --- | --- | --- | --- |
| `Test-SummonCleanup2CurrentStack.ps1` | PowerShell 7 | 124/124 PASS | 11.900 seconds |
| `Test-SummonCleanup2CurrentStack.ps1` | Windows PowerShell 5.1 | 124/124 PASS | 15.227 seconds |
| `Test-SummonCleanup2ProcessGuard.ps1` | Windows PowerShell 5.1 | 70/70 PASS | 13.065 seconds |
| `Test-SummonCleanup2ProcessGuard.ps1` | PowerShell 7 | 70/70 PASS | 12.330 seconds |

The separate-process guard fixtures exercise both ordinary waiting evidence
and deliberate waiting-evidence IO failure, plus a process-query failure that
continues after stand-in exit until the query recovers. While a separate stand-in process
is alive, another process cannot write, delete or rename protected fixture
saves or acquire the exclusive transaction boundary; the original owner stays
alive and the staged installation is retained. After the stand-in exits
normally, the tests require exact fixture restoration, released handles, a
FAILED receipt and nonzero exit from the original wrapper. They use no game,
human installation, registry or real save. These checks prove the orchestration
boundary exercised by the fixtures, not current-stack gameplay compatibility
or a fresh live Working-save observation.

## Portable gallery export

Reusable exporter/reader:
`scripts/Export-SummonCleanup2ReviewGallery.ps1`.
Documentation: `docs/SUMMON-CLEANUP2-PORTABLE-GALLERY.md`.
It packages only existing identity-listed original native PNGs, with seven
source/DLL/MVID/run/result groups and bounded fixture observations. It excludes
four derived aids. Complete raw requests/results/graphs and proprietary assets
are not included; their selected metadata and original-file hashes are recorded.

Executed commands:

```powershell
powershell.exe -NoProfile -ExecutionPolicy Bypass -File scripts/Test-SummonCleanup2ReviewGallery.ps1
powershell.exe -NoProfile -ExecutionPolicy Bypass -File scripts/Export-SummonCleanup2ReviewGallery.ps1 -OutputPath artifacts/cleanup2-portable-review-gallery-v2.zip
powershell.exe -NoProfile -ExecutionPolicy Bypass -File scripts/Export-SummonCleanup2ReviewGallery.ps1 -Mode Verify -OutputPath artifacts/cleanup2-portable-review-gallery-v2.zip
```

Results: **8/8 synthetic behavior checks PASS** (3.409 seconds). Positive export
and verify, overwrite refusal, changed PNG, extra DLL, changed index, duplicate
path and source/gallery mismatch were exercised. A preceding synthetic fixture
attempt exposed a parent PowerShell stderr-promotion issue; the test now captures
expected native-child refusals before checking their exit codes. No native
product behavior is inferred from these tooling fixtures.

Actual v2 export and independent archive consistency verification: **PASS**,
188 PNGs, seven groups, 191 ZIP entries, **182,743,270 bytes**.
SHA-256: `eb112333d06becfc9662a8766f2071dbefe29b3d5735f4817a4ebb3d577a17c4`.
All PNGs matched both their gallery identity and original local capture bytes.
The separate read-only Verify invocation also passed in 1.738 seconds with
the same ZIP hash and counts.
The initial packet remains preserved as `cleanup2-portable-review-gallery.zip`;
v2 adds document hashes and explicit failed-assertion names. Only v2 is the
current export result. Both ZIPs remain local ignored artifacts; neither was
uploaded or sent. The owner selected local review on 2026-10-22; no upload is
requested.

The current packet keeps original FAIL groups as FAIL. Native menu pages and
the five text-only native tooltip cards are bounded presentation evidence,
not a combined candidate PASS, owner art approval, native scale calibration,
natural learning, old-save reload, current installed-stack or release proof.
Archive verification checks internal consistency, not cryptographic authorship.

## Option B

`CLEANUP2-OPTION-B-SCREEN-DESIGN.md` specifies one desktop action-bar Spells-tab
chooser, common elemental presentation and explicit original-source choices.
It covers prepared/spontaneous pools, metamagic, child execution, native debit,
exceptions and teardown. It preserves known/prepared/pinned identities and all
special/domain/item/NPC routes. No hiding, rules normalization, slot remap or
implementation was performed. The mixed-source click/lifetime seam still needs
native proof after owner design selection.

The consistency review clarified that prepared counts are matching original
source preparations, canonical mixed-creature/quantity SM/SNA parents stay
visible, and only same-rank One choices enter this first proposed chooser.
Other quantities and special routes remain native. No implementation approval
or portal/destination choice is inferred; the gallery remains local.
