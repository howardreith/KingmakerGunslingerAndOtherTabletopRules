# Sprint 17 runtime qualification

## Purpose

Sprint 17 addresses the first blocked persistence rows without changing the persistence carrier. The source-only environment cannot compile against proprietary Kingmaker assemblies, so I01 and I02 remain unobserved here. The runtime-qualification workflow creates a locally compiled, fingerprinted UMM candidate and then records trusted first-launch evidence inside Kingmaker.

The workflow does **not** declare the persistence carrier safe. It only produces the binary needed to begin the lifecycle matrix.

## One-command local qualification

Run from Windows PowerShell in the repository root:

```powershell
Set-ExecutionPolicy -Scope Process Bypass

.\scripts\qualify-runtime-candidate.ps1 `
  -KingmakerInstallDir 'C:\Path\To\Pathfinder Kingmaker' `
  -Storefront Steam `
  -DisplayedGameVersion 2.1.7b
```

The script fails before packaging when any source, build, test, runtime-contract, or package check fails. On success it creates:

```text
artifacts\qualification\KingmakerGunslinger-0.0.17-runtime-candidate\
├── KingmakerGunslinger-0.0.17.zip
├── KingmakerGunslinger-0.0.17.zip.sha256
├── runtime-candidate.json
├── RUNTIME-CANDIDATE.md
├── runtime-contracts.json
└── environment.json
```

The adjacent bundle ZIP is for archiving the qualification evidence. The inner `KingmakerGunslinger-0.0.17.zip` is the file installed through Unity Mod Manager.

## Trusted I01/I02 preflight

The UMM panel can write I01 and I02 observations without manual BEFORE/AFTER snapshots because those two rows are evaluated from mod-owned, deterministic runtime checks:

- **I01:** blueprint bootstrap initialized exactly once and exactly eight expected custom blueprints are present.
- **I02:** exactly one inherited member named `UniqueId` exists on `ItemEntityWeapon`, it is readable, and its declared value type is `System.Guid` or `System.String`.

The recorder cannot use this bypass for any other matrix row. I03 through I35 retain the normal evidence requirements.

## A-D fixture

The development panel can create or normalize four Test Muskets in shared inventory:

| Label | Required state | Save-owned identity record |
|---|---|---|
| A | Loaded / Normal | Present |
| B | Empty / Broken | Present |
| C | Loaded / Broken | Present |
| D | Empty / Normal | Absent |

The command uses strict `ItemEntityWeapon.UniqueId` values, rejects duplicate identities, verifies the four states after writing, and verifies that A-C have records while D does not. Additional Test Muskets are not removed.

For I03, capture BEFORE first, run the fixture command, then record PASS only after the recorder's AFTER snapshot shows four distinct nonempty engine item IDs and the expected states.

## Strict evidence identity

Sprint 15 evidence called a diagnostic fallback value `EngineItemId`. Sprint 17 corrects this: evidence snapshots now accept only the same strict engine-issued identity used by the persistence candidate. A visible firearm with an unreadable or malformed `UniqueId` causes snapshot capture to fail instead of substituting `m_UniqueId`, `Id`, `EntityId`, a runtime hash, or a display name.

## Security and privacy

`environment.json` contains the local Kingmaker installation path. It is useful for exact reproduction but should be reviewed before public sharing. The UMM package contains only the project DLL, `Info.json`, and the blueprint manifest/schema; it contains no game or UMM assemblies.
