# Tools

- `validate_sprint29.py` validates the 0.0.29 source, 599 declared dependency-free tests, stable repair-kit/Overhaul/Repair blueprints, exact-item transactions and rollback boundaries, complete maintenance qualification harness, documentation, sealed qualification evidence, and private-reference exclusion.
- `validate_repository.py` dispatches authoritative versions; validators inherit sealed prior-sprint requirements, with `validate_sprint32.py` validating the active scatter slice.
- `test_validation_dispatch.py` covers valid, mislabeled, unsupported, missing-file, historical, and top-level dispatch behavior.
- `create_deterministic_package.py` writes sorted, fixed-metadata ZIP entries from the validated eight-file staging tree.
- `validate_sprint29_package.py` validates the standalone 0.0.29 Unity Mod Manager ZIP, including version, exact eight-file layout, blueprint ledger, qualified DLL hash, path safety, CRC, one-project-binary rule, and packaged maintenance-loop smoke guide.
- The Sprint 28 validators remain for historical archive verification.
- `build_mod_from_private_references.py` compiles the main DLL against the extracted exact private Kingmaker references without redistributing them.
- `run_exact_net47_domain_tests.py` compiles the dependency-free harness against the .NET Framework 4.7 reference surface and executes repeatable runs on a supplied CoreCLR.

Older sprint validators are retained as historical evidence tools.
