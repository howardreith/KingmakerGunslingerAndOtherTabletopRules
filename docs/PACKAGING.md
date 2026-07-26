# Packaging policy

## Standalone UMM package contract

The current standalone smoke-test package must contain exactly one root named `KingmakerGunslinger` and these eight project-owned files:

```text
KingmakerGunslinger/
  CHANGELOG.md
  Info.json
  KingmakerGunslinger.dll
  LICENSE
  README.md
  SMOKE-TEST-GUIDE.md
  blueprints/
    blueprints.json
    blueprints.schema.json
```

Exactly one binary is permitted: `KingmakerGunslinger.dll`. No PDB is included in the standalone candidate. No Kingmaker, Unity, Harmony, Unity Mod Manager, Newtonsoft.Json, private-reference, or third-party mod DLL may appear anywhere in the archive.

The package validator rejects duplicate entries, traversal paths, symlinks, an unexpected root, missing or extra files, a version mismatch, an altered blueprint ledger, a DLL-hash mismatch, CRC failure, or any foreign binary.

## Artifact set

A qualified milestone produces:

1. a standalone UMM install ZIP and SHA-256 checksum;
2. a complete milestone ZIP and checksum; and
3. a source ZIP and checksum.

The complete milestone ZIP embeds the validated standalone install ZIP and source ZIP, reports, evidence, and checksums. It does not contain the private build-reference archive or loose private assemblies.

## Source-package exclusions

The source ZIP excludes:

- `.git`, IDE state, and generated `artifacts` output;
- `GamePath.props`, absolute install paths, and local environment fingerprints;
- private reference archives and all game/Unity/UMM/Harmony/Newtonsoft binaries;
- compiled DLL, EXE, PDB, MDB, PYC, and cache files;
- local saves, logs, and test-campaign data; and
- generated outer-package manifests or checksums that would create circular hashes.

Source evidence may include plain-text compiler/test output, JSON reports, patches, and the user-supplied runtime screenshots.

## Labeling

A source or milestone archive is never the install target. Use this label only when the standalone UMM ZIP actually exists and passes validation:

> **READY FOR KINGMAKER — INSTALL THIS ZIP THROUGH UNITY MOD MANAGER**

Runtime acceptance remains separate from compile/package readiness. Version 0.0.29 still requires the complete action-bar maintenance-loop, interruption, resource, isolation, and persistence smoke test before Sprint 30 can begin.
