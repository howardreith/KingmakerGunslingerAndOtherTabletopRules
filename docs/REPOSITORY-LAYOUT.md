# Repository layout

## Sprint 17 qualification additions

`PersistenceRuntimePreflight*` contains the pure I01/I02 model, `Development/PersistenceRuntimePreflightProbe.cs` gathers Kingmaker observations, `docs/RUNTIME-QUALIFICATION.md` specifies the workflow, and `scripts/qualify-runtime-candidate.ps1` creates the local candidate bundle.

```text
KingmakerGunslinger/
  blueprints/                         Stable ID ledger and JSON Schema
  docs/                               Architecture, decisions, contracts, and history
  environment/                        Local fingerprint schema and example only
  planning/                           Next-sprint gate and entry criteria
  scripts/                            Windows build, inspection, test, and package tools
  src/KingmakerGunslinger/
    Blueprints/                       Creation, cloning, registration, verification
    Bootstrap/                        UMM/Harmony and blueprint lifecycle
    Development/                      Manual controls and reflection boundary
    Diagnostics/                      Marker lookup, correlation, and trace output
    Firearms/                         Definitions, immutable state, repositories, adapters
    Rules/                            Pure firearm rules and thin Kingmaker mutation adapters
    Properties/                       Assembly metadata
  tests/KingmakerGunslinger.DomainTests/
                                      Dependency-free domain/repository/trace harness
  tools/                              Portable validation and .NET 8 test-project generation
  validation/                         Generated portable validation result
```

## Sprint 14 persistence layers

`Firearms/` now separates identity, primary records, and legacy migration:

1. **Pure identity and repository boundaries**
   - `FirearmItemId`
   - `IFirearmItemIdentityProvider`
   - `IFirearmStateIdentityRecordStore`
   - `IdentityBackedFirearmStateVaultStore`
   - `VaultBackedFirearmStateRepository`

2. **Kingmaker identity/save adapter**
   - `KingmakerFirearmItemIdentityProvider`
   - `UnitPartFirearmStateVault`
   - `KingmakerFirearmStateVaultPartProvider`
   - `KingmakerFirearmStateVaultStore`

3. **Legacy migration**
   - Sprint 13 direct-reference `_records` retained inside the UnitPart only for migration
   - `MigratingFirearmStateRepository` for the four retained Sprint 12 token blueprints
   - separate reference- and token-migration snapshots

`FirearmRuntimeState` composes the identity-backed vault behind the existing `IFirearmStateRepository` and `FirearmItemStateService` boundaries. `WeakFirearmStateRepository`, `TokenBackedFirearmStateRepository`, and the Sprint 13 direct-reference API remain historical prototypes or migration inputs; none is the Sprint 14 source of truth for new writes.

`Development/` exposes disposable-save diagnostics for item identities, independent state, and both migration layers. `Rules/` retains range-limited touch AC; attacks still do not load, consume, or damage firearm state.


## Sprint 15 evidence layer

`Persistence/` contains the dependency-free 35-row catalog, observation validation, and gate evaluator. `Development/PersistenceEvidenceData.cs` and `PersistenceEvidenceRecorder.cs` provide the Kingmaker-facing snapshot and external JSON/Markdown recording boundary. No file under `Firearms/` reads or depends on evidence files.

`tools/run_portable_domain_tests.py` creates a temporary SDK-style test project from the classic project's explicit compile list; `scripts/test-domain-portable.ps1` is its Windows wrapper.
