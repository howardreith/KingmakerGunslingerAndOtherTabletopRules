# ADR-0011 — Collision-safe blueprint registration

**Status:** Accepted
**Date:** 2026-07-12

## Context

Kingmaker mods commonly register custom assets by setting `BlueprintScriptableObject.m_AssetGuid`, appending the object to `LibraryScriptableObject.GetAllBlueprints()`, and assigning it into `BlueprintsByAssetId`. A replacing dictionary indexer is unsafe for this project: a collision could overwrite a game asset or another mod's asset even when the manifest itself is internally valid.

## Decision

All custom blueprint registration passes through `BlueprintRegistry`.

The registry must:

- accept IDs only from the validated deployed manifest;
- require an `active` entry whose `plannedType` matches the requested blueprint type;
- check the live dictionary before creating the Unity object;
- assign `m_AssetGuid` through a reflected field contract;
- recheck immediately before mutation;
- use dictionary `Add`, not indexer assignment;
- insert into both Kingmaker indexes as one rollback-capable transaction;
- verify reference identity after insertion;
- reject repeat registration by symbol;
- log the exact symbol and GUID on success.

## Consequences

A conflicting ID prevents this mod's content initialization and requires a process restart. This is intentionally disruptive but safe: silently replacing content would be worse.

The registration adapter depends on private Kingmaker implementation details. The exact target assembly surface must therefore be reflected and recorded before runtime certification.
