# Elemental Races implementation report

## Current outcome

**AUTOMATED IMPLEMENTATION COMPLETE - HUMAN VISUAL ACCEPTANCE REQUIRED.** All
four production races, their common/race-specific rules, 68 stable identities,
racial SLAs, and atomic selector publication now exist. Guarded live evidence
proves module-OFF identity registration, no selector leakage, exact base stats
and speed, energy resistance, Keen Senses, affinity inclusion/exclusion,
multiclass total-level caster scaling, one-use resources, rest restoration, and
resource-state serialization. Separate guarded native command runs prove
Burning Hands save/damage delivery, Stone Fist delivery/expiry and unarmed
replacement, Feather Step delivery/expiry, Hydraulic Push resource and Bull
Rush behavior, native Oread armor/encumbrance movement, exact person-spell and
prerequisite behavior, and module-ON/module-OFF selector publication.
The live donor audit additionally proves complete native modular geometry and
palette inputs. Sixteen production visual blueprints, 28 stable asset proxies,
and a 56-case all-option production renderer matrix now pass guarded runtime
qualification. The expanded 224-record elemental Gunslinger equipment matrix
and an 80-case exact native clothing matrix across ten classes also pass. Eight
elemental Gunslinger fixtures pass the accepted 216-record native motion
matrix, and its original two-Human mode passes again. A three-process,
eight-fixture working-save transaction proves spent SLA state, exact
race/facts/appearance/outfit reload with the module OFF, rest, level-up,
total-level caster scaling, exact cleanup, and settings restoration. Sixty-four
additional SLA/prone/death/resurrection/polymorph transition records pass on
the development artifacts. The persistence prepare path now requires eight
distinct native Respec source/replacement commits before it may save. The exact
0.0.114 three-process persistence transaction passes, including module-OFF
reload, cleanup, and fresh-process absence. The exact 24-state boundary and
five-profile compatibility reruns also pass with exact restoration. The final
clean Release package and direct strict package revalidation pass. Subjective
appearance acceptance remains human-required.

## Authoritative baseline

- Upstream and starting SHA:
  `06c4d998f160df75ad3be7bfcf3de7e415c631d4`
- Branch: `codex/elemental-races`
- Starting version: `0.0.113` / `0.0.113-save-load-hotfix`
- Candidate version: `0.0.114` /
  `0.0.114-elemental-races-preview`
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
- Manifest arithmetic is now 1,706 total identities: 1,704 active and two
  reserved. The new reserved identity is not packaged as active content.

This Phase B checkpoint statement is retained historically; compatibility,
expanded visuals, and limitations are reported by the current sections below.

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
  profiles; all 24 engineering-artifact runtime launches passed with exact
  settings restoration.

## Production base-race inventory

- `ElementalRaceIdentityCatalog.cs`: 24 stable mechanical and 44 stable visual
  symbols/GUID-backed identities.
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
  and production race-specific visual sets with complete Aasimar-compatible
  fallback.
- `ElementalRacePublication.cs`: validated all-or-none append, third-party order
  preservation, idempotence, conflict refusal, and exact-reference rollback.
- `BlueprintBootstrap.cs`: unconditional identity registration and restart-bound
  selector publication with rollback integrated into the existing transaction.

The authoritative manifest now contains 1,706 entries: 1,704 active and two
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

These request-local scenarios are save-free. The later three-process
save-backed persistence qualification independently covers persisted resource,
identity, appearance, level-up, and module-disabled reload behavior.

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

## Guarded native visual-donor inventory

- The request-gated development probe now audits Human, Aasimar, Tiefling,
  Elf, Dwarf, Half-Elf, Half-Orc, and Gnome. It records exact resource IDs and
  names, sex and race compatibility, body and outfit parts, hidden body parts,
  color profiles, preset wrappers, skeletons, DLC flags, and native ramp
  texture metadata. It does not publish or persist a diagnostic race.
- Across the eight donors, all 358 declared references resolved (303 unique
  resources and 55 repeated references), all visual presets were complete,
  and both sexes met the required minimum of two heads and four hair choices.
  Eyebrows, beards, horns, and tail palettes are audited when present rather
  than treated as universal mandatory categories.
- Transaction `20260902T1026029043019Z-observe-elemental-race-blueprints`
  failed only because the first fixture incorrectly required eyebrows from
  Half-Orc, whose native race definition intentionally declares none. All
  358/358 links had already resolved. The requirement was corrected without
  changing production content.
- Corrected transaction
  `20260902T1030175263514Z-observe-elemental-race-blueprints` passed the full
  link/preset/options inventory. The extended texture-metadata transaction
  `20260902T1042298660656Z-observe-elemental-race-blueprints` also passed (run
  ID `20260902T1042298861108Z-ffb1e76c92564fda8db19251778e4ae5`).
  Evidence SHA-256:
  `e1bca7ba7357d0aef9e964fd02e6c5f254ee6368331c5ace0a9776202f5c5733`;
  runtime-result SHA-256:
  `6227ad7e44a917d98adcfbaaa8fd84fab31efe3a2a28f4e5af8fcfd9aa04a0c7`.
- Every inspected native ramp used the same compatible contract: 256 by 1,
  RGB24, bilinear filtering, clamp wrapping. The inventory exposes suitable
  vanilla warm, stone, pale/metallic, and blue/teal palettes without extracting
  or redistributing Owlcat textures. Human, Aasimar, and Tiefling presets use
  Human-compatible skeletons; geometry from the other donor skeletons remains
  excluded pending an explicit renderer qualification.
- Production visuals use four project-owned body wrappers, twelve stable
  standard/heavy/slender Aasimar-preset clones, and 28 project-owned
  `EquipmentEntity` proxies. Native donors are resolved before construction;
  proxy registration and validation roll back as one unit on failure, and no
  native donor is mutated. Geometry stays within the Human/Aasimar/Tiefling
  skeleton family, while audited native warm, stone, pale/metallic, and
  blue/teal 256x1 ramps provide seven skin choices per race without extracting
  or packaging Owlcat textures.
- Every sex exposes two heads, four nonempty hair styles plus no hair, two
  eyebrows, supported male beards, and three body presets. Ifrit alone offers
  the native empty choice plus two optional restrained Tiefling horn proxies.
  Kingmaker's race customization contract has no eye-color array, so eye
  appearance remains attached to native head/material assets and requires
  human review rather than an invasive subsystem.
- Guarded transaction
  `20260902T1301142529832Z-elemental-race-visual-audit` passed 56/56 cases and
  all eight race/sex coverage groups. It covered every preset and option, all
  seven skin indexes, and at least four hair-color indexes; every native doll
  produced a baked character renderer with complete non-null materials and
  shaders. It also proved 16/16 visual blueprints and 28/28 proxy resources
  resolve exactly and that cleanup preserves the shared race array and both
  blueprint indexes. Evidence SHA-256:
  `592e0d9f7c37bf850e9a79808102197d713a3e29b4655a765a66b1cdb5e8c699`;
  runtime-result SHA-256:
  `53879496629b6a8dac4cd5f41a4c99e41654c8b487736ee7525a1c6dde83a569`.
  The 224-record expanded elemental Gunslinger equipment matrix, 80-case
  ten-class native clothing matrix, 216-record native-motion matrix, and
  64-record SLA/prone/death/resurrection/polymorph matrix pass separately.
  Save-backed persistence also passes. Only subjective clipping, eye
  presentation, and overall aesthetics remain human review surfaces.

## Qualification status

| Gate | Status |
| --- | --- |
| Baseline repository validation | PASS |
| Baseline domain suite | PASS - 1,373/1,373 |
| Phase B focused probe tests | PASS - 3/3 |
| Current complete domain suite | PASS - 1,390/1,390 on 0.0.114 source |
| Phase C clean Release package | PASS - 0.0.114 deterministic preview package and strict UMM validation |
| Guarded diagnostic runtime | PASS - `20260902T0409422132157Z-observe-elemental-race-blueprints` |
| Focused schema-10 runtime observation | PASS - `20260902T0440201720486Z-observe-feature-module-settings` |
| Guarded production identity/module-off runtime | PASS - `20260902T0538341591619Z-observe-elemental-race-blueprints` |
| Guarded base mechanics/resource runtime | PASS - `20260902T0626272331311Z-disposable-elemental-race-mechanics` |
| Guarded native donor-SLA delivery runtime | PASS - `20260902T0727505151505Z-disposable-elemental-race-slas` |
| Guarded Hydraulic Push runtime | PASS - `20260902T0828281705241Z-disposable-hydraulic-push` |
| Guarded native identity/Oread movement | PASS - `20260902T0947509229460Z-disposable-elemental-race-native-identity` |
| Guarded module-ON selector publication | PASS - `20260902T0955212013652Z-observe-feature-module-settings` |
| Guarded module-OFF selector absence/identity retention | PASS - `20260902T0958454418742Z-observe-feature-module-settings` |
| Guarded native visual-donor inventory | PASS - `20260902T1042298660656Z-observe-elemental-race-blueprints` |
| Guarded production all-option visual matrix | PASS - `20260902T1301142529832Z-elemental-race-visual-audit` (56/56) |
| Guarded elemental Gunslinger class/equipment matrix | PASS - `20260902T2120481218981Z-elemental-race-class-equipment` (224/224) |
| Guarded ten-class elemental clothing matrix | PASS - `20260902T1451545064731Z-elemental-race-class-clothing` (80/80) |
| Guarded elemental native-motion and transition matrix | PASS - `20260903T0012214812700Z-elemental-race-motion` (216 motion + 64 transition) |
| Existing Human native-motion regression | PASS - `20260903T0022352926189Z-gunslinger-outfit-production-motion` (54/54) |
| Three-process save-backed/module-OFF persistence | PASS on exact 0.0.114 - `20260903T0306144426995Z` / `20260903T0308580192907Z` / `20260903T0311577493294Z` |
| Eleven-module 24-state runtime matrix | PASS on exact 0.0.114 - 24/24, zero warnings, exact settings restoration |
| Guarded expanded equipment/noncovered transitions | PASS - 224 equipment + 64 transition records |
| Five required compatibility profiles | PASS on exact 0.0.114 - 5/5 profiles, 18/18 nested runs, exact restoration |
| Elemental native respec | PASS on exact 0.0.114 - 8/8 distinct source/replacement commits with race, facts, SLA, DollData, and Gunslinger presentation exact |
| Final human-review package | PASS - clean Release build and direct strict UMM validation at `2ceeb65e` |
| Visual Adjustments | NOT-RUN - not installed |
| Human visual acceptance | HUMAN REVIEW REQUIRED for the exact package below |

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

The guarded native visual-donor checkpoint passed the complete 1,385-test
suite and the required clean Release/package gate. Clean package SHA-256:
`f15d67d2334e197f64bc0eb4f7edb580876e1a737ee6f75ae689416692c56323`;
DLL SHA-256:
`d4861724104b211cf800ba13c5617a8da60c170548a007807919c2d49a439e8c`;
DLL MVID: `bcee4c97-da02-45e2-9c11-abaa8cf497f0`. This remains a version
0.0.113 engineering checkpoint, not the production visual candidate.

The production visual checkpoint passed its guarded 56-case renderer matrix
and the required clean Release/package pipeline with all 1,385 tests. Clean
package SHA-256:
`8167bc0b7294e7d6936b59e2f58ec9b6fb6954882ea6b25379129f2ac481f3f2`;
DLL SHA-256:
`89ba7f9cd2a3f5393c6034a6b1b99a416dae7117c0546ad6cd7f8679eab21a5e`;
DLL MVID: `ef41cc19-3e5c-41a2-a9f1-f0eb51d29887`. This remains a
version 0.0.113 engineering checkpoint, not the final preview artifact.

The subsequent elemental Gunslinger class/equipment transaction reused the
accepted production compatibility harness for all four races and both sexes.
It passed 128/128 exact equipment/rebuild states, 8/8 fixture restorations,
unchanged production class/shared-unit state, and no save API call. Runtime
transaction:
`20260902T1346079130473Z-elemental-race-class-equipment`; run ID:
`20260902T1346079340812Z-a1279a136b5c438dbaf91caddec0387b`.
Runtime-result SHA-256:
`9f766d5224c6eb7a2aafccfdbb6fd38123e4e152bf94ba21d9f13d6bcb033e71`;
matrix-index SHA-256:
`6af7737ebf9f1f46511b620e83f8ea2fee254c6e8dd438e924ffad551b99aa99`;
runtime package/DLL SHA-256:
`b7ceb091a8750a6eb2b5a5124d3bc6cca8b1985e8e615b45d31cbc6385e49749` /
`3c7a6f83bdeb2b378d87a474d7f1fa662a5927da098e134fe50986f4f4d91b1a`;
DLL MVID: `c2a5c72c-05b2-4b4a-bf57-b1856c915211`. Generated
images are only supporting evidence; clipping and aesthetics remain human
  review surfaces. Broader class clothing and motion subsequently passed;
  medium armor, robes, remaining accessory slots, and subjective review remain
  separate gates.

The required clean Release/package pipeline then passed with all 1,386 tests.
Clean package SHA-256:
`1c8065ae6a0d0218930de556ef30ab5648922a18d82c3e2fabefc77fcc89bb45`;
DLL SHA-256:
`d1a45067f0636eb4df19818c1780ca3deca49cebae6f73ab5edc0acae6b9a1bd`;
DLL MVID: `93c4538b-07ad-43cd-93c1-b9fc977a9be3`. This remains a
version 0.0.113 engineering checkpoint rather than the final preview
candidate.

The save-free elemental class-clothing transaction then qualified the exact
native class presentation for Ifrit, Oread, Sylph, and Undine, both sexes, and
Gunslinger, Fighter, Rogue, Ranger, Alchemist, Magus, Wizard, Cleric, Monk, and
Kineticist. It passed 80/80 planned cases, ten/ten exact class groups, complete
materials/shaders, exact blueprint/race graph restoration, and no selector or
save-state mutation. Runtime transaction:
`20260902T1451545064731Z-elemental-race-class-clothing`; run ID:
`20260902T1451545269918Z-bd479eeecec14d81b202649ee013234b`.
Runtime-result/evidence/runtime-evidence SHA-256 values:
`d4432e55fc47f503bab4167c79c7920ec01fedf1934e6be94435e360df2c84c5` /
`846b5d05ed6f573006856d9595310c1b0af7cc6ce7bf360f56a745f25e89fcc4` /
`da636404476081bc32256fe511cedb9aa1ebe58a32a95ab6347c65e28cc34053`.
Runtime package/DLL SHA-256:
`49415761e4abe0d68fbb82fd7e23a12aabc01850529b07c5fd5f4faf6751d498` /
`c450e8019d92cd70a365ef8611965e217525483cb8192bb2c5e5e10f8daab3db`;
DLL MVID: `232218ab-4a89-4447-ac65-465500f73cf6`.

Four preceding attempts failed closed and touched no save. They exposed, in
order: a reconstructed clothing order that differed from authoritative
`LoadClothes`; an incorrect assumption that class clothes appear in
`DollData.EquipmentEntityIds`; synchronous access before native view
attachment; and one remaining mixed avatar access in the settle path. The
fixture now maps exact observed clothing references, keeps DollData assertions
to identity/customization, waits for the attached `Character`, and validates
the baked native render. These corrections narrowed instrumentation without
weakening production assertions.

The required clean Release/package pipeline then passed with all 1,387 tests,
production compilation, build-output and SoundBank validation, deterministic
package creation, and strict standalone UMM validation. Clean package SHA-256:
`a60b04d199d3b20c3e63342ae896275a8e98b8117fecf106c891bfd5c5521382`;
DLL SHA-256:
`18be7cb6e5d5923471bf5d702eda8aa206f4b4a8ac2a72cc709d1c589f72fc56`;
DLL MVID: `5cd6ca86-8a25-42d9-a900-95ef2257ef65`. This remains a
version 0.0.113 engineering checkpoint rather than the final preview
artifact.

The guarded `elemental-race-motion` mode reused the accepted production motion
transaction without changing its Human fixture path. It covered Ifrit, Oread,
Sylph, and Undine, both sexes, with native idle, slow walk, normal run, turn,
pistol and musket attacks, production musket reload, and shortsword melee.
Transaction `20260902T1520151111405Z-elemental-race-motion` passed all 14
assertions: 8/8 exact fixtures, 216/216 records and PNG/sidecar pairs, 864
labelled views, 16 movements, 8 turns, 24 attacks, 8 reloads, 8 exact avatar
restorations, 8 exact combat-boundary reconciliations, unchanged published
class data, and no save call. Run ID:
`20260902T1520151280510Z-1512e7f8e1fe41b4bc268018108d6941`.
Runtime-result/index/runtime-evidence SHA-256 values:
`9fa5484e3b5c2eb96778305f785387c271783d7958945ab77bb4e7b49d744eab` /
`7d2616e5a6c1647625ec3605720713dbfddf538542ad1627230256d558bb315d` /
`04c0078e909f7aad3a0c4caaf20d62185f725841e4e7cb02ebc42351b2b6c8e3`.

The original accepted Human mode was then rerun on the same source. Transaction
`20260902T1531223520715Z-gunslinger-outfit-production-motion` passed all 14
assertions, 2/2 fixtures, 54/54 records, 216 views, all native action outcomes,
both restorations and combat boundaries, blueprint immutability, and no save
call. Run ID:
`20260902T1531223770733Z-ed026c1eaa5240c5893cb70313f6c8e3`.
Runtime-result/index/runtime-evidence SHA-256 values:
`da1da6742ab2852541928475f1f5375783df660f426f5976f289f7e7f1a4f886` /
`5f04ac4cf1da0159cd98f5ddf6b692e02714349ddaffa9edccc62abfb052bf07` /
`6731baa03fd3439542ba835ecefbbf0a3c1904ba9f9c969afaf6497487fdc3c9`.
Both runs used local-runtime package SHA-256
`c5e6431d97a116d737584fe85a0d05bbf5e713b3be8f915dc9c4bc72f184815d`,
loaded DLL SHA-256
`f7f69f3f0c96425497639bc7845e971205e28b652115c8773c11d57b0d5b4ec7`,
and DLL MVID `39b4cd0b-71e9-470f-a032-33cec7ea7f14`.

The required clean Release/package pipeline then passed on the elemental-motion
source tree: repository validation, all 1,388 tests, production compilation,
build-output and SoundBank validation, deterministic package creation, and
strict standalone UMM validation. The 22,940,247-byte package SHA-256 is
`497625066222aa5c08fe2323e7134e8c257a8dddf58b193687c20a7d0f05c279`;
the 5,294,080-byte DLL SHA-256 is
`7cf1181feec82b606189fd22a3bffbd15d9db8ae44ee76c519e2c6ef325145a8`;
DLL MVID is `399e6d2a-4593-4e90-b59d-44a49ca9058a`. `Info.json` remains
version `0.0.113`; this is an engineering checkpoint rather than the final
preview candidate.

The three-process save-backed persistence transaction then qualified all four
races and both sexes in `KMG_AUTOMATION_WORKING`. Prepare transaction
`20260902T1759420290804Z-elemental-race-persistence-prepare` (run
`20260902T1759420540785Z-9764141caa2645208e08ba64e1870d23`) passed 10/10:
eight native level-1 elemental Gunslingers retained exact production
customization/outfit state and persisted their racial SLA from one use to zero.
With Elemental Races disabled, transaction
`20260902T1802161725612Z-elemental-race-module-disabled-persistence` (run
`20260902T1802161745656Z-11fbec67bb7e4939b4de818d6695e538`) passed 10/10:
selectors stayed hidden while all identities, facts, statistics, abilities,
spent resources, sex-specific DollData, renders, and class clothes reloaded;
rest restored exactly one use and promotion produced level/caster level two
before exact cleanup. After original settings bytes were restored, transaction
`20260902T1805097536949Z-elemental-race-persistence-verify-absent` (run
`20260902T1805097767152Z-7e0adc8b36e343e8b2f9891c5a7713fa`) passed 6/6 with
zero fixture residue and exact baseline party/global-unit structure.

Final persistence runtime-result/index SHA-256 pairs are
`ac8d16c4d8d2fa7bdc382e5c3836ce3190371c7008b26b9042ccb6ec6a54921a` /
`ac1ae1a5748a4fc97e21da297824ca9269dd7a262b04fc486f5b784f6e25a17d`,
`f16a93ab55be1bbbf87622bdeae4b927b65a352fe637add66d050e1a3bf697d4` /
`f285ce0e8d762748cbfa63055300e3333114c089457a9f501b11b8d81ddd2ee8`,
and
`27d69f12cba8e77482fd012122fe1b5d47c115763d886fc6ffb9c36e8aacc12f` /
`d285ec257df1910a437c81f8eb635f94a86833cadb6496def2fb5d82d5bbcd02`.
All used deployment `20260902T1758236782720Z`, package/DLL SHA-256
`b4a1762c3dfd2d91c025a8f2ed9ce6cac8dc49a443ce5f404c601e60c731a843` /
`17f85ab3142fcf4aea91ec96870d00a09411a8121a929deac9bed97eaef4cf47`,
and DLL MVID `b9eb12f4-f96b-4374-b046-bd5c78e88127`. The exact original
and restored module-settings bytes hash to
`d07a06e1b67d35107ffd84da0e02453bfa0adcfaac59bcb68a4353444c7ec52e`.

The required clean Release/package pipeline then passed repository validation,
all 1,389 tests, production compilation, output/SoundBank validation,
deterministic packaging, and strict UMM validation. The 22,957,333-byte clean
package SHA-256 is
`4e2faaa7a0671ef4463750434e07b54057766a97e4478b3141262da47de65ecd`;
the 5,349,376-byte DLL SHA-256 is
`ac846bde870dfe7c2bd2355aa0e8bf15fe69cd77b42bcf5a9f6e3eaeea1f2770`;
DLL MVID is `5b9fc86a-0d95-4a75-b2cd-8a9237b4e516`. This remains a
version `0.0.113` engineering checkpoint rather than the final preview.

The exact 0.0.114 native-Respec persistence qualification used commit
`e42461bad81212a6f1cefbd08a2a62e301888d86`, deployment
`20260903T0305586600979Z`, package/DLL SHA-256
`95f0e240576690dc97fe880fa525eb99d8bb535da48f45f94bf3a6d5646ee45a` /
`96a7a21b514bec0c92db1612e855fdcd7a72b82782d755d65234db22db1fd7a9`,
and DLL MVID `c079f498-586c-4675-abe4-cde1d2b79e8f`.

Prepare run
`20260903T0306144647588Z-097cd7bdb7f047dbbfbe543d8755cc96`
passed eight distinct native Respec commits, eight exact rule/SLA records,
eight spent resources, 16 images, 40 labelled views, exact membership, and one
authorized working-save write. Module-disabled run
`20260903T0308580202889Z-778e1392b87f40f6888fc62bc8693a76`
passed all eight reload/rest/level-up/appearance/outfit records and one exact
cleanup save. Fresh-process run
`20260903T0311577733283Z-92bc4f6035804e8394297cf7222f571a`
proved all fixtures absent with zero writes. Runtime-result/index/runtime-
evidence SHA-256 triples are
`f1d0e7cc6827c21d7a9e3d0ce15671ac462df4471935f74c7cd4ef60ddce6683` /
`1f3c7810db9180073e58e6afcea90f4d7e3ed18bf9a3b02d1b40badd17578ec9` /
`3609a28c4469c85db3acb3c6af77d6aecfcfb53ee6596842341b1ecf6bee5551`,
`dbfc60070fd8a05f51e26f6277b2a801bdcd31789eacae73e015c281d142001a` /
`8e86a2037bee5ebed23c580e5a3ca5fc0bc831f9ed72753928cc2a47618d3c97` /
`9eaf554735be5e92a7688db0fade55d28871dcf78a80541ab5025d7f6ccefd6f`,
and
`b9e61790b825b7d9c8c3ab8f58231a5cd87365684b58438aa3c8249bae36ab55` /
`e2fdf8e020579bbdcd27728d402809ab9eb6768747de7d2b898833d39107c2a8` /
`9be3be51483fa7f529494ad056f6ac2b852bf9f1e0dd67cea71599f655cbe5dc`.
The original settings bytes were restored exactly to SHA-256
`d07a06e1b67d35107ffd84da0e02453bfa0adcfaac59bcb68a4353444c7ec52e`.

The exact 0.0.114 feature-module boundary qualification reused committed
artifact `b90cb3038984b49d818690da702e2ca94ea85c14` across 24 guarded
fresh launches. Deployment `20260903T0324413582448Z`; source-state,
package, and DLL SHA-256 values:
`752118f23f10154c084844dd82c87197620cae25875da2e986420dc1c57fcc0b`,
`dcd7b7750e36ff13c087e29ab1dc9ae58f64e902d13faa5da48510b2bf2f7fe1`,
and
`7740bfd9f96706d349babeedd3abcaf779169d2ff20fc71a7045f5d719db08da`;
DLL MVID `dd706f3c-ddad-4bc2-888c-fe4c68cb66e4`. The deployment
manifest hashes to
`2fc3a58efafdee5781e0ea7e09c1a4cec7bd34d8ba5848530924036fc6eebaa8`.

All 24 `observe-feature-module-settings` results passed with zero warnings
and exact expected/active equality: all ON, each single module OFF,
Gunslinger only, each non-Gunslinger module alone, and all OFF. Evidence spans
`20260903T0325021042437Z-observe-feature-module-settings` through
`20260903T0407238227609Z-observe-feature-module-settings`; the complete
directory/run-ID ledger is in `ELEMENTAL-RACES-JOURNAL.md`. The matrix
restored `FeatureModules.json` exactly; an independent post-run hash remained
`d07a06e1b67d35107ffd84da0e02453bfa0adcfaac59bcb68a4353444c7ec52e`,
and no Kingmaker process remained. This does not qualify the five exact
0.0.114 compatibility profiles; those were qualified separately below.

The exact 0.0.114 compatibility qualification reused commit
`967f896dc6e7441660e8d7a3c99bf173a4d52c14`, package SHA-256
`bc50a684f76679e164035b46496ee02bbaa3a933145c4f29f2f15a2ac587760d`,
DLL SHA-256
`302cd4c81977c6aa5f7b2ca8e5dbf132f2e4c15fa3a9b70410e982700d0914bf`,
and DLL MVID `88951bb3-a82e-4f4c-a2d5-1fb73a4ddcd6`. Deployment
`20260903T0422357690930Z` manifest SHA-256:
`d8ebc807786257ea2dd880f2f17f29e76e9e454192a43ed691756749179adf5d`.

All five required profiles passed:

- `gunslinger-only`:
  `compat-20260903T042304Z-6ab63512362b`
- `gunslinger-call-of-the-wild`:
  `compat-20260903T042749Z-2987d5aa2784`
- `gunslinger-races-unleashed`:
  `compat-20260903T043352Z-52546b4a381d`
- `gunslinger-call-of-the-wild-races-unleashed`:
  `compat-20260903T043806Z-fc082022c750`
- `gunslinger-high-risk-combined-favored-class`:
  `compat-20260903T044415Z-0f01ef146905`

The 18 nested fresh-process results all report PASS with zero warnings. The
high-risk observer loaded exactly KMG, Call of the Wild, Favored Class, Tweak
or Treat, and Races Unleashed; retained 47/47 Call of the Wild classes in a
49-class final catalog; and retained the singular Gunslinger class and
selector input. Its shared race catalog contained 20 unique entries: all
eight audited native races, Ifrit/Oread/Sylph/Undine once each and contiguous
at indexes 9-12, and all seven Races Unleashed races once each at indexes
13-19. Repeated Elemental Race reconciliation was an exact no-op. Aid Another,
Helpful/Bodyguard, and archetype reconciliation also passed in the high-risk
stack. The complete nested directory/run-ID ledger and result hashes are
recorded in `ELEMENTAL-RACES-JOURNAL.md`.

Every profile restored the complete pretest Mods tree and relevant settings.
`FeatureModules.json`, Call of the Wild settings, Favored Class enabled-trait
settings, and managed SoundBank SHA-256 values remained respectively
`d07a06e1b67d35107ffd84da0e02453bfa0adcfaac59bcb68a4353444c7ec52e`,
`24cc3f80269992a53ebbfd1f5986e5aab056841d6b2f43d8e22e764cdb73f6e8`,
`bdceed77d2bf4a31dd9e4eeb64ef9d55a42ef59d23f46abcb1ddbcc6ef66754b`,
and
`0e9f88c562f4f937a8941ace0f241bb31a7ed56b46fbca549c98f764392edf18`.
Visual Adjustments remains NOT-RUN because it is not installed; subjective
UI and appearance acceptance are not inferred.

## Final human-review artifact

The final canonical package was built from clean product/evidence commit
`2ceeb65e9c2d0d78189f78ead18e538c8e01eb90` on
`codex/elemental-races`. The identically named upstream branch matched that
commit, while a final fetch confirmed `origin/master` remained
`06c4d998f160df75ad3be7bfcf3de7e415c631d4` at version 0.0.113. The
repository source-state fingerprint was
`baa36e497a6e372af4234f38dc6630a88037fb1814af3802f0ec5ebc3dd02505`.

`powershell -NoLogo -NoProfile -ExecutionPolicy Bypass -File
.\scripts\build.ps1 -Configuration Release -Clean -Package` passed
repository validation, 1,390/1,390 domain tests, production compilation,
build-output and SoundBank validation, deterministic packaging, and strict
UMM validation. A separate direct `scripts/validate-package.ps1` invocation
also passed.

- Package:
  `artifacts/packages/KingmakerGunslinger-0.0.114-elemental-races-preview.zip`
- Package bytes: 22,977,802
- Package SHA-256:
  `ee78b29e4fd4c8b3407d6dcd0d326a0ed1a6352c597ee169a4bd7cd09da8aa41`
- DLL bytes: 5,411,328
- DLL SHA-256:
  `827f10cd09efe8c9a15b718624c277253ca270f5ef9af222aff8c015f5d8745b`
- DLL MVID: `61ff8880-9f96-4657-bda8-37e9f2454ea9`
- DLL file version: 0.0.114
- DLL informational/product version:
  `0.0.114-elemental-races-preview`
- Packaged `Info.json`: ID `KingmakerGunslinger`, version `0.0.114`,
  UMM `0.32.4`
- Package entries: 135
- Blueprint manifest: 1,706 entries, 1,704 active, two reserved, no duplicate
  GUID or symbol; Elemental Races contributes 69 manifest entries, of which 68
  are active identities and one is the permanently development-gated
  diagnostic identity.

The complete guarded compatibility matrix ran against exact-reference commit
`967f896dc6e7441660e8d7a3c99bf173a4d52c14`. A path-scoped Git comparison
from that commit to `2ceeb65e` reports no change under `src`, `assets`,
`blueprints`, `Info.json`, or `Directory.Build.props`; only evidence,
packaged documentation, compatibility-profile disposition, and validation
status changed. The canonical package above has a distinct SHA-256 and MVID
and was not itself relaunched after packaging, so no byte-for-byte runtime
claim is inferred for it. Its runtime inputs are source-identical to the
fully qualified artifact.

All automatable gates are complete. Visual Adjustments is NOT-RUN because it
is not installed. Subjective clipping, appearance, and option quality remain
**HUMAN REVIEW REQUIRED** for the exact package above.
