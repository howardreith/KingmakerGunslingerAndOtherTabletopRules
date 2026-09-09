# Guarded fresh-process teleportation persistence

Use Windows PowerShell 5.1 and Steam App ID 640820. This owner-authorized workflow
never overwrites the working save or any pre-existing save. It requires an exact
Build-Local package and its verified deployment manifest:

```powershell
.\scripts\Invoke-TeleportationPersistenceQualification.ps1 `
  -ExpectedVersion 0.0.119 `
  -DeploymentManifestPath <exact-deployment-json> `
  -PackagePath <exact-tested-package-zip> `
  -Confirm:$false
```

Development checkpoints used 0.0.118. The final 0.0.119 release must repeat this
gate with its exact clean committed package/version. No manual UI
operation is needed or authorized by this procedure. Never substitute another
campaign for `KMG_AUTOMATION_WORKING`.

The orchestrator holds Windows read leases on every pre-existing save, recording
SHA-256, length, creation/write times and attributes. It captures the complete
Mods file/directory inventory and module settings. An exact native header check
identifies the working campaign before launch. The guarded loader suppresses only
the native LoadedTimes header update and its commit; it does not rewrite inputs.

Four separate native processes run the same `disposable-teleportation-persistence`
scenario with private phase plans inside the guarded evidence root:

1. A loads only the verified working save, uses native area loading to reach the
   world map, and makes two real two-edge journeys. Each intermediate/final
   boundary gains exactly one count. Late native unlocks, selection, cancellation
   and a real contextual Teleport add no counts. Real book fixtures are removed
   before native `Game.SaveGame` writes the unique transaction A save.
2. B loads only that exact hashed A save in a fresh process. It verifies one
   canonical main-character UnitPart owner and exact raw payload, migration flag,
   all counts and exploration boundary. Another real journey increments each
   crossed boundary once. Both Teleport families compose from positive restored
   counts. A real contextual cast spends exactly one prepared use without adding
   familiarity. The cleaned campaign is saved to unique B.
3. C disables only Teleportation, loads B in another process, and verifies absent
   publication/hooks/rows and exact saved fields. Native Travel starts once;
   its request-local route is immediately cancelled and its origin restored
   before saving unique C. No movement frame or ledger mutation is synthesized.
4. D restores the original settings bytes and loads C in another fresh process.
   Its canonical owner, full ledger and exploration boundary must equal B/C.
   D has no permission to create a save.

`SaveManager.CreateNewSave` supplies a new manual descriptor. The guard permits
one exact `SaveRoutine` invocation and its native cloned descriptor at
`PrepareSave`, which must generate an absent, exact transaction filename inside
the native save directory. Ownership is recorded durably before native Clear or
Save can write. Only that descriptor may pass `SaveStashedArea`; unrelated writes
are rejected. Completion requires native callback, idle commit/operation state,
a readable native ZIP, exact header, and one matching serialized UnitPart in
`party.json`. Subsequent phases require the preceding PASS receipt and hash.

Cleanup deletes only the A/B/C filenames recorded by this transaction, after
copying evidence. It rechecks every pre-existing save and restores settings bytes
and metadata, including any pre-existing `.previous` backup. A new settings
backup is accepted only when its contents differ solely in the authorized module
flag. Newly created KMG loader caches may be removed only when absent from the
initial inventory, correctly named, inside KMG's directory, and byte-identical to
the tested DLL. Unknown files/content fail closed. No unrelated mod is modified.

Native UMM `UnityModManager.ModEntry.Load` derives its cache suffix from assembly
write time, requested manager version and executing manager version; the suffix
is not a process ID. On this qualified assembly, it copies the DLL unchanged.
`FeatureModuleSettingsStore.Save` uses `File.Replace` with `.previous`. These are
known runtime products, not package files. Their cleanup is recorded separately.

The structured `transaction-result.json`, per-phase runtime results, native save
receipts, protected-save inventory and complete Mods inventories are the evidence.
Raw saves, native dumps and machine-local artifacts remain outside Git. The
installed-stack result does not qualify another UMM version or arbitrary late
campaign states.
