# Icon overhaul checkpoint — September 14, 2026 UTC

Paused at the owner's request to preserve the remaining weekly allocation.
Branch: `codex/icon-art-overhaul-v2`. Qualified implementation checkpoint:
`a42ba694cc4cb5910390341566f691a76352e435`. It includes master
`e5f1426a6347793e1e978237be4b0a88d4c9d662` (v0.0.129) through merge
`a20b0d236a1259f4c92521ab3a7780956095631e`. Published history and the original
icon branch are preserved. This checkpoint records approval and resume state;
unfinished runtime instrumentation is preserved locally, outside this commit.

## Completion

| Work | Status |
|---|---|
| Artwork | Complete: 89 paintings plus Rapid Reload; all 90 exact images owner-approved. |
| Durable authoring standard | Complete: guide, actual references, catalog, AGENTS/README discovery, deterministic export and validation. |
| Consumer integration | 284 dispositions; 137 painted assignments; native reuse, monograms and protected assignments recorded. |
| Native UI qualification | Most surfaces complete; remaining surfaces listed below. |
| Final acceptance | Pending final native UI review and saved-parameter verification. |

The owner approved all 80 production paintings in this session. Their exact
source/export hashes are frozen in [PRODUCTION-APPROVAL.json](PRODUCTION-APPROVAL.json)
and [the approval table](PRODUCTION-APPROVAL.md). The ten pilot approvals remain
separate. Superseded revisions are not approved. No pixels changed here.

Completed native evidence covers character creation, heritages/alternate traits,
eleven racial feats, Weapon Focus/Rapid Reload P/M/B selected facts and character
sheets, eastern controls, strategic spell learning/books, scroll inventory and
merchant presentation, strategic text controls, and thirteen racial buff icons.
The v0.0.129 integration was separately qualified, including Oracle Recall and
the updated scroll behavior. Individual reports retain their own exact artifacts
and limits; earlier captures are not claimed as new captures on later DLLs.

Accepted action/item art, native spells and feat-root art, eastern lettering,
meshes/materials/audio, blueprint GUIDs, firearm parameters and mechanics remain
protected. Out-of-scope audit dispositions remain in the catalog. Known unrelated
fixture presentation findings include long headers, empty creator dolls and
repeated native Weapon Focus labels.

## Remaining work

1. Qualify 39 native racial action consumers: 26 parents, ten variants, two
   held-touch deliveries and one activatable. The fixture is implemented locally
   but not runtime-qualified. Builds 1–4 failed increasingly narrow fixture
   checks; the latest issue was an empty native action-type cache created by menu
   rendering. Build 5 owns and removes that exact empty cache, but was not run.
2. Qualify the four higher firearm feat roots across P/M/B in real eligible
   selectors, previews, cancellation, committed disposable facts and sheets.
   Seven isolated draft files compile against the installed APIs. They are not
   integrated or runtime-qualified.
3. Verify saved firearm parameters. Read-only inspection found no supported
   firearm parameter GUIDs in the permitted working save. Mission section 8
   requires separate authorization for new save creation/writes. No such write
   is authorized or performed. Prepare a concrete bounded fixture before asking.
4. Assemble and obtain final native UI acceptance, then reconcile the remaining
   evidence with the final artifact. The 90-image art approval is complete.

Artwork and policy are finished. The remaining work is qualification and final
acceptance; the action fixture is the principal current engineering uncertainty.
This is not a completed mission or a release.

## Checks and installation

Latest published buff checkpoint: five guarded Steam PASS runs, 63 assertions,
393 native captures, thirteen inspected buff identities, twelve complete
disposable mercenaries. Repository validation, 23 catalog, 25 capture, six paired,
eleven request and all 1,639 domain tests passed, with a clean Release build,
strict 224-file package and same-artifact working-save smoke.

Qualified buff artifact:

- Source: `7e15cd13cbcf0917c9eaefab78a90d52e89467708c1b6fa93c3eac1572a0eb62`
- Package: `30030682eda80a3a41f65dc0a0c42993e0eb3f5dfd45ec034088eeaeebcdd994`
- DLL: `804f0b30ab8e2dd417e1c6fd267c9bb23c8efa675889e87b3bff198eff2906f3`
- MVID: `ff0b4a9c-7daa-4a94-8f82-f9ca3bb47693`

Local action build 5 passed repository validation, 23 catalog, 34 capture, six
paired, eleven request and 1,639 domain tests, clean Release and strict package
validation. **Runtime NOT RUN; never deployed.** Its archived package hash is
`cad380f151353e3d22a4f2eb816335d36e2626b62e123562dab098e62d75c4a4`, DLL
`f065aa0c4a031fb4471f3b4fe57e6beaf6856ab19dc76ab1da9564895174db53`, MVID
`161b909c-e67f-4b1e-afb3-93d6fa96bcc8`, source
`9e3557ff3dd2a94ddc9ae2d9f9fe6bbd8b3d6a6947e9b46c70f3475ff1c014a9`.
Local archive prefix: `artifacts/icon-overhaul-v2/native-racial-actions-build-5-UNDEPLOYED-*`.

After the last runtime run (build 4), all **136 original installation files and
settings were independently restored** at `2026-09-14T01:07:53.7005325Z`. No game is running, no save writes
occurred, and no runtime matrix remains active. The approval checkpoint is a
metadata/documentation change plus adaptation of an existing negative approval
test and its completed-scope guard for the new approved status. Repository
validation passed: 23 catalog, 25 native capture, six paired and eleven request tests, with all source/export/protected checks. No compiled runtime source changed in this approval commit.

## Resume precisely

The eleven action source/project/test files are preserved in local stash
`2bf5ec3ded6b81f29c2058ec872cf5088d521bf7` (based on `a42ba694`). Existing older
stashes remain untouched. To resume, inspect the worktree/current master first,
then apply that exact stash without dropping it. Rebuild before deployment;
the archived build predates this approval metadata checkpoint. Use build number
6 for the next guarded matrix through `Deploy-NativeRacialActions.ps1` and
`Run-NativeRacialActionMatrix.ps1`. Do not rerun the stale initial integration
script or old `NativeRacialActions.pending.cs`.

The helpers, failed-run evidence, restoration audits, native API research,
seven `NativeHigherFeat*.pending.cs` drafts and compiler helper remain under
`artifacts/icon-overhaul-v2`. They are local and not pushed. The action curator's
recorded capture-test count needs updating from 33 to 34. Read runtime JSON only
after normal game exit; in-flight Windows reads can interfere with atomic writes.

Once action qualification, inspection and restoration pass, curate/commit/push
that source before integrating the higher-feat scenario. Its request allowlist,
runner hooks, strict selector evidence validation and focused tests still need
integration. No further launch or implementation is scheduled during this pause.

## Review and policy links

- [All 90 approved images](PRODUCTION-REVIEW.html)
- [Native buff qualification](NATIVE-RACIAL-BUFF-QUALIFICATION.json)
- Local native gallery: `artifacts/icon-overhaul-v2/NATIVE-RACIAL-BUFF-REVIEW.html`
- [Authoring guide](../../docs/ICON-ART-GUIDE.md)
- [Reference index](../../docs/art/ICON-REFERENCE-INDEX.md)
- [Canonical catalog](../../assets-source/original-icons/icon-catalog.json)
- [Implementation history](IMPLEMENTATION-REPORT.md)
