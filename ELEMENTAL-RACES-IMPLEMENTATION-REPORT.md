# Elemental Races implementation report

## Current outcome

**IN PROGRESS - BASE-RACE, IDENTITY, PUBLICATION, AND RACIAL-SLA MECHANICS RUNTIME QUALIFIED.** All
four production races, their common/race-specific rules, 24 stable identities,
racial SLAs, and atomic selector publication now exist. Guarded live evidence
proves module-OFF identity registration, no selector leakage, exact base stats
and speed, energy resistance, Keen Senses, affinity inclusion/exclusion,
multiclass total-level caster scaling, one-use resources, rest restoration, and
resource-state serialization. Separate guarded native command runs prove
Burning Hands save/damage delivery, Stone Fist delivery/expiry and unarmed
replacement, Feather Step delivery/expiry, Hydraulic Push resource and Bull
Rush behavior, native Oread armor/encumbrance movement, exact person-spell and
prerequisite behavior, and module-ON/module-OFF selector publication.
Distinctive visual proxies, save-backed persistence, compatibility profiles,
the full 24-state runtime boundary matrix, and human acceptance remain pending.

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
  objects, exact racial stats, validated Aasimar/Tiefling type precedent,
  native Keen Senses/Slow and Steady donors, resistance 5, affinities, SLAs,
  and complete Aasimar visual fallback.
- `ElementalRacePublication.cs`: validated all-or-none append, third-party order
  preservation, idempotence, conflict refusal, and exact-reference rollback.
- `BlueprintBootstrap.cs`: unconditional identity registration and restart-bound
  selector publication with rollback integrated into the existing transaction.

The authoritative manifest now contains 1,662 entries: 1,660 active and two
reserved. Each race owns these six identity categories: race, resistance,
affinity, SLA feature, daily resource, and SLA ability. The exact GUID list is
maintained in `blueprints/blueprints.json`; tests require the full 24-entry map.

The donor `RaceId` is Aasimar. Guarded inspection proved that installed
Aasimar and Tiefling base races and their first heritage facts do not grant the
empty native `OutsiderType` fact; Hold/Charm/Enlarge/Reduce Person therefore
target them. The project races deliberately match that installed precedent,
while their exact `BlueprintRace` references remain distinct. Checks that use
only `RaceId` may still classify an elemental race as Aasimar; no broad rewrite
is applied.

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

## Guarded racial-SLA and Hydraulic Push inventory

- `ElementalRaceSlaScenario.cs` creates request-local disposable native units
  and executes the production Ifrit, Oread, and Sylph abilities through
  `UnitUseAbility`. It proves cancel-before-commit, exactly one committed use,
  second-use unavailability, native rest restoration, and exact cleanup.
- Ifrit uses the cloned native 15-foot/5-foot Burning Hands cone. At total
  level 5 it observed caster level 5 and DC 17. With the same natural roll 10,
  a failed Reflex target took 20 and a successful Reflex target took 10; the
  action graph remained Fire d6-per-rank with native half-on-save behavior.
- Oread applied native `StoneFistBuffMedium`
  (`af56c42a31a264648b42d725f362c18d`) for 60 seconds at level 1 and 300
  seconds at level 5, expired through the native duration path, and changed
  the empty-hand weapon to native Stone Fist.
- Sylph applied native Feather Step buff
  (`c748cceadcab2614b942e56ff257cfbc`) for 600 seconds at level 1 and 3,000
  seconds at level 5, then expired through the native duration path.
- Guarded transaction
  `20260902T0727505151505Z-disposable-elemental-race-slas` passed all 13
  assertions. Run ID:
  `20260902T0727505399772Z-38fc5469392f4871af7e367a5dd10f22`;
  standalone evidence SHA-256:
  `166c5fe7ed1846de64bbdf28ee113b9145b609859cf36621526f6f19322cd322`;
  DLL SHA-256:
  `7d3558f05a999542a15a8bd231e11367cf83f1b075e599dd745036ca15163e81`;
  DLL MVID: `44c0f06d-569d-4226-8487-6fa8ec15c2e5`.
- `HydraulicPushScenario.cs` exercises the production ability against
  request-local hostile native units. It covers Intelligence, Wisdom, and
  Charisma maxima; all-negative modifiers; deterministic ties; 2 Fighter / 3
  Wizard total-level scaling; ordinary success/failure; immunity; native force
  movement; and actual `UnitUseAbility` lifecycle.
- Hydraulic Push now uses ordinary `AbilityResourceLogic` for availability and
  an idempotent request-owned commit action immediately before the native Bull
  Rush action. This preserves normal zero-resource gating while ensuring an
  instant native effect spends exactly once whether command debit happens
  before or after synchronous delivery. Cancellation spends zero. No global
  patch is used.
- Guarded transaction
  `20260902T0828281705241Z-disposable-hydraulic-push` passed all 12 assertions.
  Run ID: `20260902T0828281945254Z-55efda7cf4584851b7bc869178dd9a8b`;
  evidence SHA-256:
  `c5afc2e51c9227f0d9c8acb71a39d232ff721c9fc067fdbff256f99d4a3d1cb5`;
  DLL SHA-256:
  `8f19b9528df358ae14f2e28fb945b7a9ea7041c72c38dfeeb962f4d806b85b82`;
  DLL MVID: `d62cbb4f-bd4e-4dbc-a177-2034a04efb15`. The native maneuver
  path constructed no unrelated attack, saving throw, or opportunity attack.
  It used native Bull Rush resolution and `UnitPartForceMove.Push`.

These request-local scenarios are save-free. They do not replace the required
two-process save-backed persistence qualification.

## Guarded native identity, movement, and publication inventory

- `ElementalRaceNativeIdentityScenario.cs` uses audited native medium and heavy
  armor, `EncumbranceHelper`, `UnitPartEncumbrance`, native speed stats, and
  request-local generic speed facts against Oread, Dwarf, and Human controls.
  Oread/Dwarf observed `20/20/20` feet unarmored/medium/heavy and stayed at 20
  under a calculated heavy equipped load; Human observed `30/20/20` and 20
  under the same heavy load. All returned exactly after fixture removal.
- The same scenario tests Human, Aasimar, Tiefling, and all four production
  races with native `AbilityData.CanTarget` for Hold Person, Charm Person,
  Enlarge Person, and Reduce Person plus real feature/no-feature and exact-race
  prerequisites. All seven matched the installed targetable person-spell
  behavior; exact project race prerequisites remained distinct despite the
  shared donor `RaceId`.
- Two fixture attempts failed closed before the final PASS. Transaction
  `20260902T0933001067567Z-disposable-elemental-race-native-identity` exposed a
  party-context requirement in the patched private encumbrance controller, so
  the fixture moved to the underlying native per-unit APIs. Transaction
  `20260902T0937414169613Z-disposable-elemental-race-native-identity` proved
  immunity is applied when modifiers are evaluated (not by changing the raw
  `-10` penalty) and that personal inventory is excluded from this unit load;
  the fixture moved to audited equipped full plate. Neither touched a save.
- Corrected transaction
  `20260902T0947509229460Z-disposable-elemental-race-native-identity` passed all
  16 assertions (run ID
  `20260902T0947509479461Z-478bab6aa9d44475ba40e7b36219205a`). Evidence SHA-256:
  `b3378be2e628d3b7896bcaefb2dcb56498f2026118ba0b28ecdc89b021b68161`;
  runtime-result SHA-256:
  `265c4562ce2b04e12ed803c290f52f724d1981f0aaf994521739c5962ec2df06`.
- The existing feature-module observer now inventories the live
  `BlueprintRoot.Progression.CharacterRaces` catalog by both reference and GUID.
  Enabled transaction
  `20260902T0955212013652Z-observe-feature-module-settings` published all four
  once at indexes 9-12 in the required contiguous order. Disabled transaction
  `20260902T0958454418742Z-observe-feature-module-settings` published zero while
  retaining all 24 registered identities. Both shared catalogs were unique and
  both settings transactions restored the exact original SHA-256.

## Qualification status

| Gate | Status |
| --- | --- |
| Baseline repository validation | PASS |
| Baseline domain suite | PASS - 1,373/1,373 |
| Phase B focused probe tests | PASS - 3/3 |
| Current complete domain suite | PASS - 1,385/1,385 |
| Phase C clean Release package | PASS - strict UMM validation |
| Guarded diagnostic runtime | PASS - `20260902T0409422132157Z-observe-elemental-race-blueprints` |
| Focused schema-10 runtime observation | PASS - `20260902T0440201720486Z-observe-feature-module-settings` |
| Guarded production identity/module-off runtime | PASS - `20260902T0538341591619Z-observe-elemental-race-blueprints` |
| Guarded base mechanics/resource runtime | PASS - `20260902T0626272331311Z-disposable-elemental-race-mechanics` |
| Guarded native donor-SLA delivery runtime | PASS - `20260902T0727505151505Z-disposable-elemental-race-slas` |
| Guarded Hydraulic Push runtime | PASS - `20260902T0828281705241Z-disposable-hydraulic-push` |
| Guarded native identity/Oread movement | PASS - `20260902T0947509229460Z-disposable-elemental-race-native-identity` |
| Guarded module-ON selector publication | PASS - `20260902T0955212013652Z-observe-feature-module-settings` |
| Guarded module-OFF selector absence/identity retention | PASS - `20260902T0958454418742Z-observe-feature-module-settings` |
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

The donor-SLA/Hydraulic source tree passed the complete 1,384-test domain suite,
both guarded native scenarios, and the required clean Release/package gate.
Clean package SHA-256:
`da6efee6dd435b917cd2191bb6aebca6f255eaca18532f1a30dd10d3d342d816`;
DLL SHA-256:
`fee51842a8144e1e75324b968f322b2bbf5de7a181ad0c5bd2523443e31ced6b`;
DLL MVID: `0670cdd2-e916-4d20-942d-ebcf8340cfec`. This remains a
development version 0.0.113 artifact, not the final preview candidate.

The native-identity/movement/publication checkpoint passed the complete
1,385-test suite, both selector states, and the required clean Release/package
gate. Clean package SHA-256:
`abb0639d5942fb4692a6cb455671436a47de6d7ce31f5833976db7d37768dacd`;
DLL SHA-256:
`b25b2e76e9c10d900fa391c432f7838a7023cbeed9381de5338e0afe90756ce0`;
DLL MVID: `57a10073-0765-4107-986e-de9ef987ca0b`. This is still a version
0.0.113 engineering checkpoint rather than the final preview artifact.
