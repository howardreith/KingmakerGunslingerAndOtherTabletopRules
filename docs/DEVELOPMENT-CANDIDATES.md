# Development candidates

Third explicit owner exception on 2026-10-17: publish exact installer/readiness
repair product `39c60fb5e916` as `v0.0.157-controller-load-alpha.3`, through
the additional fixed `Publish-CotwActionBarAlpha.ps1 -AlphaVersion 3` profile.
Existing alpha 2 pins/default and stable qualification refusals remain intact;
candidateKind=development and releaseQualification=false still apply. See
[authorization](BUILD-AND-RELEASE.md#controller-load-alpha-3-authorization--2026-10-17).

Second explicit owner exception on 2026-10-17: publish exact CotW action-bar
repair product `92b442c06dd8` as test prerelease
`v0.0.157-controller-load-alpha.2` through its hash-pinned
`Publish-CotwActionBarAlpha.ps1`. No rebuild or native qualification is inferred;
candidateKind=development and releaseQualification=false remain. See
[authorization](BUILD-AND-RELEASE.md#controller-load-alpha-2-authorization--2026-10-17).

One subsequent owner exception: on 2026-10-17 the owner explicitly authorized
publishing the exact Brown-Fur product `913dfd6e8938` as a public test alpha.
Only `Publish-BrownFurAlpha.ps1` accepts that hash-pinned preview, with a
prerelease tag, unchanged bytes and `releaseQualification=false`. This does not
make development evidence eligible for stable release/closure or authorize
publication of any other candidate. See
[the explicit alpha authorization](BUILD-AND-RELEASE.md#brown-fur-alpha-1-authorization--2026-10-17).
The general private-candidate rules below remain unchanged.

A development candidate is an exact package of unreleased review-branch work,
built so that a guarded native observation can run against it. It is never a
release candidate, a qualification candidate or a public artifact, and nothing
in this route changes how a release is built or validated.

## Why it exists

`Build-Local.ps1` runs the repository wrapper, whose version-aware validator for
the active release (`validate_alternate_summoning_release157.py` at v0.0.157)
pins that release's exact content: no identity change and production scope
limited to the release's own files. That is correct for a release, and it means
a review branch that appends identities without bumping the version cannot
produce a native test package at all. Bumping the version, adding a new release
validator, replacing a DLL by hand or skipping validation would each misrepresent
the work. This route does none of them.

## What is replaced, and what is not

`Build-Local.ps1 -DevelopmentPurpose <purpose> -DevelopmentTestFilter <filter>`
replaces exactly two release steps:

| Release step | Development replacement |
| --- | --- |
| `validate_repository.py` dispatch to the active release validator | `tools/validate_development_candidate.py`: released version metadata unchanged (no bump); the released ledger as an exact prefix (append-only); schema, format and uniqueness of every identity (collisions); released Sprint 24 routes public with their original assets present and packaged; released static record unchanged; `expanded_summoning_manifest.py` validation, which re-derives every adapter identity |
| complete domain suite | the named `-DevelopmentTestFilter` selection, run once, with the runner's own output kept as evidence |

Everything else is the release pipeline, unchanged: every corruption fixture and
offline contract in the repository wrapper (including the active release
validator's own fixtures), private-reference provenance against the installed
game, the exact-reference Release build, supply-icon and companion checks,
deterministic package assembly and strict package validation.

The build refuses a dirty tree, a purpose without a filter or a filter without a
purpose, a blank or zero-test selection, any failed or inconsistent test
evidence, runner output on standard error, and a second build over an existing
receipt for the same commit and purpose.

## Identity

The package is written to
`artifacts/development-candidates/<purpose>/<commit12>/KingmakerGunslinger-<version>-development-<purpose>-<commit12>.zip`,
never under `artifacts/local-runtime`, with a
`.development-receipt.json` beside it instead of a `.build-local.json`. The
receipt binds: `candidateKind=development`, `releaseQualification=false`, the
purpose, commit, branch and source-state fingerprint, package and DLL SHA-256 and
MVID, firearm audio hashes, the exact private reference identity, the
development validation summary and its hash, and the selected tests (filter,
selected/registered counts, failures, a hash of the passed names and the log
hash). Build-Local reads its own receipt back through the consumer's checks
before it finishes.

## Consumption

Only a caller that names the same purpose can consume the candidate:
`Read-KmgBuildLocalManifest -DevelopmentPurpose`, `Deploy-Local.ps1
-DevelopmentPurpose`, `Invoke-KingmakerRuntimeTest.ps1 -ReuseInstalledArtifact
-DevelopmentPurpose`, `Collect-Runtime-Evidence.ps1 -DevelopmentPurpose` and
`Invoke-KingmakerCompatibilityProfile.ps1 -ReuseInstalledArtifact -PackagePath
<package> -DevelopmentPurpose <purpose>`. The candidate must still describe the
exact current clean commit and source state. Leases, archive safety, intake
snapshots, staging, rollback and exact restoration are the unchanged profile and
deployment transactions.

Without the purpose, every reader refuses a development candidate, and with it
every reader refuses a release candidate. Deployment records, runtime evidence
and profile receipts made under a development candidate say
`candidateKind=development`; release deployment reuse refuses such a deployment,
and closure qualification (`Assert-KmgClosureProofFiles`) refuses any proof that
carries it. Release publication reads only `artifacts/local-runtime`.

`scripts/Test-DevelopmentCandidate.ps1` (run by the repository wrapper) and
`tools/test_development_candidate_validator.py` exercise the success path and
each refusal, including the executed nested calls of the development build up to
its exact-reference compiler boundary.

## Baseline

The baseline is the release of the active version, resolved from the tag the
release publisher pushes on the released commit: `Info.json` version
`0.0.157` means tag `v0.0.157` (`8726ec737a`). The tag must exist locally
(`git fetch --tags origin`), must be an ancestor of the candidate, and its
`Info.json` must carry that version; otherwise the candidate is refused. The
version metadata, ledger prefix and static record are compared with that
commit, so the validator needs no edit when a new version is released. (The
first version pinned the v0.0.156 commit and refused every candidate once
v0.0.157 shipped.)

A development candidate does not apply a release's content contracts. In
particular the exact bytes of released art are not pinned: the v0.0.155
Sprint 24 contract pinned the Tyrannosaurus art, so a branch refining that
art could not be observed natively. The released Sprint 24 routes must still
be public and their original assets present and packaged. The release
validators keep their own pins unchanged; the next release's validator is
where changed released art is accepted for release.
