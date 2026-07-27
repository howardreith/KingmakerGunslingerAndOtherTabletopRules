# Sprint 30 validator repair

## Root cause

Commit `c355d9d` advanced `Info.json` and assembly metadata to 0.0.30, but
`scripts/validate-repository.ps1` continued to invoke
`tools/validate_sprint29.py` directly. That historical validator correctly
requires 0.0.29, 599 tests, and the byte-identical 0.0.29 current smoke guide.
`Directory.Build.props` and `validation/static-validation.json` also retained
Sprint 29 metadata.

The reconstruction appeared qualified because it ran the portable domain suite
and exact-reference compiler directly. It did not run the top-level repository
validator or strict eight-file packaging path after advancing the version.

## Repair design

`tools/validate_repository.py` now reads authoritative `Info.json` metadata and
dispatches only supported versions. Sprint 29 remains strict and directly
executable. Sprint 30 calls the Sprint 29 validator with explicit 0.0.30
metadata and 611-test parameters, inheriting its blueprint, transaction,
rollback, project-list, documentation, evidence-integrity, and private-binary
checks before applying Sprint 30-specific checks.

Unknown versions fail closed. Sprint 30 additionally requires its qualification
record, report, smoke guide, generic action sources, project links, adapter
migration, ammunition round trip, and focused tests.
