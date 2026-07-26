# Sprint 4 completion report

**Milestone:** `0.0.4-s04-diagnostic-blueprint`
**Prepared:** 2026-07-12
**Scope:** Stable manifest loading, collision-safe blueprint registration, and one invisible diagnostic feature.

## Delivered

### Stable manifest runtime

`BlueprintManifest.Load` resolves `blueprints/blueprints.json` beneath the executing mod assembly's directory and fails closed when the file is missing, empty, oversized, malformed, encoded invalidly, contains unknown JSON members, changes the schema or namespace, permits runtime GUID generation, releases retired IDs, contains duplicate symbols or GUIDs, or declares invalid entry metadata.

`BlueprintId` accepts only lowercase 32-character hexadecimal GUIDs in `N` format. It exposes no generation method, and project source contains no `Guid.NewGuid` call.

The original nine Sprint 1 identifiers remain byte-for-byte unchanged. `KMG.Diagnostic.InitializedFeature` is the only entry whose status changed: `reserved` → `active`.

### Collision-safe Kingmaker registry

`BlueprintRegistry`:

1. resolves an active manifest entry and verifies its declared blueprint type;
2. rejects repeat registration by symbol;
3. checks `LibraryScriptableObject.BlueprintsByAssetId` before invoking the Unity object factory;
4. assigns Kingmaker's private `m_AssetGuid` field only from the manifest;
5. rechecks the live dictionary immediately before mutation;
6. appends the asset to `GetAllBlueprints()`;
7. inserts with dictionary `Add`, never a replacing indexer;
8. verifies the registered instance; and
9. rolls back both indexes if the transaction fails after mutation begins.

The implementation is stricter than the common mod helper pattern because a duplicate can never overwrite another asset.

### Diagnostic blueprint

Sprint 4 registers exactly one `BlueprintFeature`:

```text
symbol: KMG.Diagnostic.InitializedFeature
guid:   6294cc6964914ea7bf450d5ef82fadde
name:   KMG_Diagnostic_InitializedFeature
ranks:  1
hidden: true
components: 0
```

The feature is not referenced by a class, selection, progression, unit, item, ability, or root blueprint. It therefore has no acquisition path and no behavior.

### Bootstrap integration

The Sprint 3 ordering guarantees remain intact:

- exactly one `HarmonyInstance.Create` call;
- exactly one `PatchAll` call;
- a zero-argument `LibraryScriptableObject.LoadDictionary` postfix;
- first-observed library remains authoritative;
- initialization happens only after both library observation and successful patch installation;
- duplicate lifecycle observations do not re-register or replace the diagnostic asset;
- any manifest or registration error marks initialization failed for the process.

### Runtime-contract inspection

The reflection script now verifies, in addition to the Sprint 3 loader contracts:

- `BlueprintScriptableObject.m_AssetGuid` exists as a non-static string field;
- `LibraryScriptableObject.BlueprintsByAssetId` exists;
- exactly one zero-argument instance `GetAllBlueprints` method exists;
- `BlueprintScriptableObject.ComponentsArray` exists;
- `BlueprintFeature.HideInUI` and `BlueprintFeature.Ranks` exist.

## Validation completed in this environment

- All C# files parse without tree-sitter errors.
- All PowerShell files parse without tree-sitter errors.
- All JSON and MSBuild/XML documents parse.
- The blueprint JSON validates against its included Draft 7 schema.
- The portable Sprint 4 validator passes.
- Modeled negative cases cover duplicate symbols, duplicate GUIDs, malformed GUIDs, enabled runtime generation, planned-type mismatch, an existing library collision, transaction rollback, successful dual-index insertion, and repeated bootstrap initialization.
- The source tree contains no DLL, executable, PDB, MDB, local game path, or runtime-contract fingerprint.
- Internal SHA-256 and ZIP integrity checks pass in the final package.

## Environment-dependent gates still open

This environment lacks Windows MSBuild, the .NET Framework 4.7 targeting pack, the installed Kingmaker managed assemblies, Unity Mod Manager, and a running game. Therefore this milestone does **not** claim:

- type-checking against the exact installed assemblies;
- successful Debug or Release compilation;
- successful Harmony installation in Kingmaker;
- runtime manifest loading;
- runtime collision behavior;
- in-game lookup of the diagnostic GUID;
- character creation, new-game, save, and load smoke tests;
- an installable UMM archive.

Those gates are specified in [TESTING.md](../../TESTING.md). No substitute/stub DLL is supplied.

## Sprint result

Sprint 4 is complete as a source milestone. The next implementation sprint is bounded to an immutable firearm-definition domain model and marker component, without adding a weapon or combat rule.
