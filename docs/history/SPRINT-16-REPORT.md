# Sprint 16 report — runtime qualification

## Result

Sprint 16 is complete as a source milestone.

```text
Version:                    0.0.16-s16-runtime-qualification
Source implementation:     complete
Portable source validation: passed
Dependency-free C# tests:  not executed — dotnet SDK unavailable
Kingmaker compilation:     not performed
UMM install ZIP:           not produced
Persistence gate:          NoGoIncomplete
Ammunition work:           blocked
```

The branch decision remained **persistence NO-GO**. No compiled Sprint 15 candidate or lifecycle evidence existed, so every Critical row remained unobserved beginning with I01 and I02. Sprint 16 therefore does not add powder, bullets, reload actions, or another persistence carrier. It supplies the shortest auditable path to the first locally compiled runtime candidate and the first three matrix rows.

## Delivered

### Trusted I01/I02 preflight

The pure `PersistenceRuntimePreflightEvaluator` accepts engine-independent observations and produces exactly two ordered checks:

- **I01** passes only when blueprint initialization completed exactly once and the runtime holds exactly eight expected custom registrations.
- **I02** passes only when `ItemEntityWeapon` exposes exactly one readable inherited member named `UniqueId` whose declared value type is `System.Guid` or `System.String`.

Unavailable measurements become `Blocked`; observed contract violations become `Fail`. The preflight cannot manufacture checks for any other matrix row.

`PersistenceRuntimePreflightProbe` gathers the runtime measurements, while the evidence recorder can append the resulting I01/I02 observations to the active build-fingerprinted evidence session. This is the only path allowed to record observations without firearm BEFORE/AFTER snapshots.

### Strict evidence identity

Persistence evidence now obtains `EngineItemId` only from `KingmakerFirearmItemIdentityProvider`, the same strict provider used by the identity-vault candidate. A visible exact firearm whose `UniqueId` is missing, malformed, empty, duplicated, or unsupported causes snapshot capture to fail closed. The previous diagnostic fallback is no longer accepted as save evidence.

### Deterministic I03 fixture

The development panel can create or normalize four exact Test Muskets, ordered by strict engine item ID:

| Firearm | State | Identity-vault record |
|---|---|---|
| A | Loaded / Normal | present |
| B | Empty / Broken | present |
| C | Loaded / Broken | present |
| D | Empty / Normal | absent |

The command adds missing Test Muskets up to four, rejects duplicate identities, verifies every resulting state, verifies records for A-C and no record for D, and leaves additional Test Muskets untouched.

### One-command local qualification

`scripts/qualify-runtime-candidate.ps1` performs seven blocking stages on Windows:

1. Source validation.
2. Explicit Kingmaker installation-path validation.
3. Exact environment fingerprinting.
4. Installed-assembly contract inspection.
5. Compilation and execution of the dependency-free C# tests.
6. Release compilation and UMM packaging.
7. Package validation, hashing, and generation of a qualification bundle.

A successful local run produces an inner `KingmakerGunslinger-0.0.16.zip` explicitly labeled as ready for a Kingmaker smoke test. That local result would establish only that the binary can begin runtime testing; it would not itself produce persistence `Go`.

## Stable blueprint ledger

Sprint 16 adds no blueprint IDs and changes no existing GUIDs.

```text
Manifest entries:          12
Active entries:             8
Reserved entries:           4
New Sprint 16 IDs:          0
Legacy migration tokens:    4
```

## Scope deliberately excluded

- No Black Powder Charge or Lead Ball item blueprints.
- No reload ability or inventory transaction.
- No attack-time loaded-state validation or consumption.
- No new save carrier.
- No use of buffs, names, slots, blueprint IDs, runtime hashes, or external evidence files as firearm state.
- No compiled DLL or UMM package built in this environment.

## Testing status

Portable validation verifies metadata, stable IDs, project inclusion, 371 named test declarations, pure-layer boundaries, preflight restrictions, strict evidence identity, fixture invariants, qualification-script structure, C#/PowerShell syntax, documentation links, source cleanliness, and independent reference models.

The portable runner was invoked and exited with code 2 because `dotnet` was not installed. The 371 C# tests were therefore declaration-, syntax-, and model-validated but not compiled or executed. The main mod also remains uncompiled because the installed Kingmaker/UMM assemblies and Windows .NET Framework toolchain are unavailable here.

## Gate after Sprint 16

Sprint 17 may begin ammunition only after a locally compiled Sprint 16 UMM candidate produces a persistence evidence session whose decision is `Go`, including all 30 Critical rows and the required two-run reproductions. Without that evidence, Sprint 17 remains on the specific persistence or qualification failure branch.
