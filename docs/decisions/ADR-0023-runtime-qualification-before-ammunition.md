# ADR-0023: produce a fingerprinted runtime candidate before ammunition

## Status

Accepted for Sprint 16.

## Context

The persistence evidence set is empty because no compiled UMM candidate exists. Every Critical lifecycle row is therefore incomplete, beginning with I01 and I02. The Sprint 16 branch rules prohibit ammunition work and prohibit choosing another persistence carrier without observed runtime evidence.

## Decision

Sprint 16 changes no firearm persistence representation. It adds:

- a deterministic I01/I02 runtime preflight;
- a recorder path restricted to those two trusted checks;
- a strict A-D fixture for I03;
- strict engine identity in evidence snapshots;
- a one-command local qualification workflow that compiles, tests, fingerprints, packages, and validates a UMM candidate.

Manual PASS recording still requires BEFORE/AFTER snapshots for every row except the built-in I01/I02 checks.

## Rejected alternatives

### Begin ammunition while persistence is unproved

Rejected because state loss would invalidate reload and ammunition transactions.

### Mark source validation as I01/I02 PASS

Rejected because source parsing cannot prove Harmony installation, Kingmaker blueprint registration, or the installed `ItemEntityWeapon.UniqueId` contract.

### Generate or infer item identities for the fixture

Rejected because the persistence candidate must fail closed when Kingmaker's own identity is unavailable.

### Use evidence sidecar files as firearm state

Rejected because diagnostic files are not part of Kingmaker's save graph and cannot be authoritative gameplay state.

## Consequences

A developer with Kingmaker and Windows build tools can produce the first honest UMM candidate in one command. The project still cannot claim persistence GO until the full matrix passes. Ammunition remains blocked.
