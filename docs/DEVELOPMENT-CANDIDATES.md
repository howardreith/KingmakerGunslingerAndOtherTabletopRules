# Development candidates

A development candidate is an exact package of unreleased review-branch work,
built so that a guarded native observation can run against it. It is never a
release candidate, a qualification candidate or a public artifact, and nothing
in this route changes how a release is built or validated.

## Why it exists

`Build-Local.ps1` runs the repository wrapper, whose version-aware validator for
the active release (`validate_firearm_continuation_release156.py` at v0.0.156)
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
| `validate_repository.py` dispatch to the active release validator | `tools/validate_development_candidate.py`: released version metadata unchanged (no bump); the released v0.0.156 ledger as an exact prefix (append-only); schema, format and uniqueness of every identity (collisions); released Sprint 24 public boundary and original assets; released static record unchanged; `expanded_summoning_manifest.py` validation, which re-derives every adapter identity |
| complete domain suite | the named `-DevelopmentTestFilter` selection, run once, with the runner's own output kept as evidence |

Everything else is the release pipeline, unchanged: every corruption fixture and
offline contract in the repository wrapper (including the v0.0.156 release
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
