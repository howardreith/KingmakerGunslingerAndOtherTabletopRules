# Elemental Races implementation report

## Current outcome

**IN PROGRESS - BASE-RACE MECHANICS AND MODULE-OFF RUNTIME QUALIFIED.** All
four production races, their common/race-specific rules, 24 stable identities,
racial SLAs, and atomic selector publication now exist. Guarded live evidence
proves module-OFF identity registration, no selector leakage, exact base stats
and speed, energy resistance, Keen Senses, affinity inclusion/exclusion,
multiclass total-level caster scaling, one-use resources, rest restoration, and
resource-state serialization. Native ability delivery/combat effects,
module-ON publication, distinctive visual proxies, save-backed persistence,
compatibility profiles, and human acceptance remain pending.

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

Distinctive visual proxies, compatibility scenarios, final artifact hashes,
and final limitations remain pending exact implementation evidence.

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

## Production base-race inventory

- `ElementalRaceIdentityCatalog.cs`: 24 stable symbols/GUID-backed identities.
- `ElementalRaceDefinition.cs` and `ElementalRaceCatalog.cs`: strongly typed,
  fixed Ifrit/Oread/Sylph/Undine rule definitions and original player text.
- `ElementalRaceBlueprintSet.cs`: exact six-object inventory per race and fixed
  deterministic order.
- `ElementalRaceRuleComponents.cs`: descriptor-scoped +1 DC and total-character
  level/Charisma racial SLA parameter components; no global spell patch.
- `ElementalRaceAbilityFactory.cs`: one-use resources; sanitized native Burning
  Hands, Stone Fist, and Feather Step clones; narrow native Bull Rush Hydraulic
  Push reconstruction.
- `ElementalRaceBlueprintFactory.cs`: separate Aasimar-compatible Medium race
  objects, exact racial stats, native Outsider/Keen Senses/Slow and Steady
  donors, resistance 5, affinities, SLAs, and complete Aasimar visual fallback.
- `ElementalRacePublication.cs`: validated all-or-none append, third-party order
  preservation, idempotence, conflict refusal, and exact-reference rollback.
- `BlueprintBootstrap.cs`: unconditional identity registration and restart-bound
  selector publication with rollback integrated into the existing transaction.

The authoritative manifest now contains 1,662 entries: 1,660 active and two
reserved. Each race owns these six identity categories: race, resistance,
affinity, SLA feature, daily resource, and SLA ability. The exact GUID list is
maintained in `blueprints/blueprints.json`; tests require the full 24-entry map.

The donor `RaceId` is Aasimar. This intentionally follows Kingmaker's native
outsider/person-spell precedent and Human skeleton contracts. It may cause
dialogue or other checks that consult only `RaceId` to treat an elemental race
as Aasimar; no broad rewrite is applied.

## Guarded base-mechanics inventory

- `ElementalRaceMechanicsScenario.cs` creates disposable native character
  generation units for each production race, applies real facts, performs 2
  Fighter plus 3 Wizard level-up, resolves matching/nonmatching energy damage,
  exercises the real SLA resource and rest paths, calculates real ability
  parameters, and round-trips the spent resource record without using a save.
- Native rule observations prove resistance 5 (`8 -> 3` matching and `8 -> 8`
  nonmatching), a source-specific +2 racial Perception modifier, total caster
  level 5, spell level 1, Charisma-based SLA DC, matching affinity `+1` exactly
  once, and nonmatching affinity `+0` for every race.
- The first run caught an unsafe Unity serialization surface: Kingmaker's
  `SpellDescriptor` enum has an `Int64` backing type, unsupported in serialized
  component fields in this Unity version. Production now stores a validated
  32-bit mask for the four low-bit descriptors and casts only during event
  handling. This is a local component fix, not a global spell patch.
- Guarded transaction
  `20260902T0626272331311Z-disposable-elemental-race-mechanics` passed all 28
  assertions. Run ID:
  `20260902T0626272562123Z-f9463005dae440f0a17e4b6268bb1800`;
  standalone evidence SHA-256:
  `902f8b81d87883230f344a67db017829c897ef3e74a55ce534b0674ba2934c65`.
  The fixture restored the exact global-unit reference sequence and reported
  `saveStateTouched=false`.

This scenario does not claim actual Burning Hands delivery/save resolution,
Stone Fist buff behavior or expiry, Feather Step buff delivery, Oread armored
or encumbered movement, or Hydraulic Push combat-maneuver resolution. Those
delivery-specific gates remain required.

## Qualification status

| Gate | Status |
| --- | --- |
| Baseline repository validation | PASS |
| Baseline domain suite | PASS - 1,373/1,373 |
| Phase B focused probe tests | PASS - 3/3 |
| Current complete domain suite | PASS - 1,382/1,382 |
| Phase C clean Release package | PASS - strict UMM validation |
| Guarded diagnostic runtime | PASS - `20260902T0409422132157Z-observe-elemental-race-blueprints` |
| Focused schema-10 runtime observation | PASS - `20260902T0440201720486Z-observe-feature-module-settings` |
| Guarded production identity/module-off runtime | PASS - `20260902T0538341591619Z-observe-elemental-race-blueprints` |
| Guarded base mechanics/resource runtime | PASS - `20260902T0626272331311Z-disposable-elemental-race-mechanics` |
| Guarded native SLA delivery runtime | NOT-RUN |
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

The production-rules guarded checkpoint also remains a non-final 0.0.113
artifact. Package SHA-256:
`abab69dfa4d593421c6fb40ff72021da07ec064651bd2072630f0827045efd46`;
DLL SHA-256:
`b95cb93e35bb4338673c0a532367346264de487cce775ebc024c8ce71df2a3c5`;
DLL MVID: `6d810d87-4a3b-49ca-91e2-b2b7ff423a57`. This is not the
human-acceptance candidate.

The subsequent required clean Release/package gate passed on the documented
checkpoint. Its package SHA-256 is
`bb05bc9ba75ebb596ba57d2eee36fe71b95d9b3baff4a079ec2cb1c44a8ab4d4`,
DLL SHA-256 is
`8ee289d26d2754d394a570dc2dd3f0fee6cb3360a8f7163d7fcff2cacfefcfeb`,
and DLL MVID is `6ef0225c-8e4e-4a60-9853-84db65f331b9`. It also remains a
development checkpoint rather than the final preview candidate.

The base-mechanics clean Release/package gate passed with all 1,382 tests.
Package SHA-256:
`599e65d26fb92ae8146296c8265043849d7394d77482fe70084eb593793d3c44`;
DLL SHA-256:
`cab862592cb85a732c565c4811b44f77f39d68db6a7a57ed7f7a06419c8606b1`;
DLL MVID: `284f7252-665f-417d-ba47-5786cfe95236`. This remains a version
0.0.113 engineering checkpoint, not the human-acceptance preview candidate.
