# Elemental Races implementation report

## Current outcome

**IN PROGRESS - PHASE C SOURCE/DOMAIN QUALIFIED.** The request-gated diagnostic race and
`observe-elemental-race-blueprints` scenario prove stable hidden registration,
native character-generation fact application, Human-compatible male/female
doll rendering, accepted Gunslinger outfit compatibility, blueprint-reference
persistence, and exact rollback. No production elemental race, rule feature,
SLA, visual proxy, or selector publication exists yet. The single preview-off
feature-module setting and its selector-publication intent are implemented.

## Authoritative baseline

- Upstream and starting SHA:
  `06c4d998f160df75ad3be7bfcf3de7e415c631d4`
- Branch: `codex/elemental-races`
- Version: `0.0.113` / `0.0.113-save-load-hotfix`
- Baseline domain qualification: 1,373/1,373 PASS
- Local game: Pathfinder: Kingmaker 2.1.7b accepted assembly fingerprint
- Local UMM: 0.32.4; Harmony 1.2.0.1

## Phase B implementation inventory

- `ElementalRaceDiagnosticIdentityCatalog.cs`: one manifest-reserved GUID,
  available only to the explicit guarded scenario.
- `ElementalRaceDevelopmentProbeScenario.cs`: request-owned registration,
  exact collision scan, native race-selection/fact fixture, two-sex donor doll
  audit, accepted outfit-link audit, hidden-reference JSON round-trip, and exact
  cleanup.
- Runtime catalog/runner and PowerShell preflight integration for
  `observe-elemental-race-blueprints`; the scenario is classified save-free and
  does not permit manual input.
- Three focused domain tests cover reserved identity/manifest arithmetic,
  guarded scenario wiring, and the no-publication/atomic-cleanup/outfit
  contract.
- Manifest arithmetic is now 1,638 total identities: 1,636 active and two
  reserved. The new reserved identity is not packaged as active content.

Production stable GUID inventory, race definitions, mechanics, visual proxies,
publication behavior, fallback behavior, compatibility scenarios, final
artifact hashes, and limitations remain pending exact implementation evidence.

## Phase C feature-module inventory

- `elemental-races` is the eleventh module, represented by bit 1024 and one UMM
  checkbox: `Elemental Races: Ifrit, Oread, Sylph, and Undine (preview)`.
- Schema 10 preserves every explicit legacy value. Missing or malformed files
  use the established ten ON defaults plus Elemental Races OFF. An absent
  Elemental Races key in schemas 0 through 9 migrates OFF; an explicit true or
  false value survives.
- Active and pending state, equality, hash code, formatting, ordered JSON,
  compatibility profile transactions, guarded request validation, runtime
  observation, and selector-publication planning all include the eleventh key.
- Domain tests enumerate all 2,048 configurations. The authoritative
  PowerShell catalog independently generated 24 unique `2 + 2N` boundary
  profiles; those real runtime launches remain pending the production graph.

## Qualification status

| Gate | Status |
| --- | --- |
| Baseline repository validation | PASS |
| Baseline domain suite | PASS - 1,373/1,373 |
| Phase B focused probe tests | PASS - 3/3 |
| Current complete domain suite | PASS - 1,377/1,377 |
| Phase C clean Release package | PASS - strict UMM validation |
| Guarded diagnostic runtime | PASS - `20260902T0409422132157Z-observe-elemental-race-blueprints` |
| Focused schema-10 runtime observation | PASS - `20260902T0440201720486Z-observe-feature-module-settings` |
| Guarded production mechanics runtime | NOT-RUN |
| Eleven-module 24-state runtime matrix | NOT-RUN |
| Guarded visuals/runtime persistence | NOT-RUN |
| Compatibility profiles | NOT-RUN |
| Human visual acceptance | NOT-READY |

Three preceding probe transactions failed closed and are retained in the
journal with their exact causes. None touched a save or published a race.

The Phase B checkpoint artifact is not a production Elemental Races candidate:
it remains version 0.0.113 and contains only development-gated probe support.
Its package SHA-256 is
`160b21230624d3ebc66f2a6c7f3da4e33b3abb0a2605bff250cded143ff6c8c9`;
the DLL SHA-256 is
`a0887d6061a35b213f7e9ad8df6e65543de66c2fbd39250c13f27cfa3b209320`;
the DLL MVID is `c900ae62-326b-4ec8-a36a-b672122b4266`.

The current Phase C checkpoint package remains a non-production 0.0.113
artifact. Package SHA-256:
`a3fa4c26704f59ce3bc8eed61325a4443a04b3d504a6f3d3518d3f26461b5d5a`;
DLL SHA-256:
`5875845dd31e1b4c6a5ea4f764df08d4e325df88b58069c933b174661e204eaf`;
DLL MVID: `1dbe88b3-acc6-45e0-b6c4-f981d9a135f4`.
