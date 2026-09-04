# Elemental Races expansion implementation report

## Current outcome

**IN PROGRESS - FOUNDATION PASS; RELEASE A IMPLEMENTED WITH LIVE BLUEPRINT,
SELECTION, PROVIDER, PARAMETER, AND ALTERNATE-SLA COMMAND PROOF; FULL RELEASE A
QUALIFICATION AND RELEASES B/C REMAIN PENDING.**

The mission began from clean authoritative `master` commit
`6874dc15a27ded132456dbdd480f47c794543a05` on dedicated branch
`codex/elemental-races-expansion`. The first work phase is the required
0.0.114 foundation audit and hardening is complete and independently
qualified. Release A source, identities, versioning, focused tests, and live
blueprint graph are implemented; selection/reconciliation, exact SLA
parameters, resource lifecycle, and alternate SLA command delivery have live
proof, while persistence, respec/migration, visuals, and compatibility remain
open gates.
Historical Elemental Races evidence is
preserved as historical evidence only and does not qualify new release
behavior.

## Planned release inventory

| Release | Version | Scope | Status |
| --- | --- | --- | --- |
| Foundation | 0.0.114 baseline | affinity, SLA, movement/maneuver, ownership, runtime organization | PASS |
| A | 0.0.115-elemental-heritages | twelve heritage choices under four parent races | IMPLEMENTED; QUALIFICATION IN PROGRESS |
| B | 0.0.116-elemental-feats | shared, Ifrit, Sylph, and Undine feat catalog | NOT STARTED |
| C | 0.0.117-elemental-traits | replacement slots and required alternate traits | NOT STARTED |

Favored-class bonuses are out of scope.

## Authoritative baseline

- Starting and fetched master SHA:
  `6874dc15a27ded132456dbdd480f47c794543a05`
- Intervening master commits: none
- Starting version: `0.0.114` / `0.0.114-elemental-races`
- Starting manifest: 1,706 total, 1,704 active, two reserved
- Starting Elemental Races manifest: 69 total, 68 active, one guarded reserved
- Feature module schema/count/boundary: 10 / 11 / 24
- Inherited race identity model: four exact `BlueprintRace` objects using
  `RaceId.Aasimar`, no `OutsiderType`, Keen Senses adaptation
- Inherited publication model: unconditional identity registration plus
  module-gated atomic additive selector publication

## Qualification status

The clean 0.0.114 branch baseline is qualified: repository validation passed;
the complete dependency-free domain/reflection suite passed 1,390/1,390; the
clean Release build and packaging pipeline passed; and an independent strict
package validation passed. The baseline ZIP contains 135 entries and is
22,977,592 bytes with SHA-256
`b5c88113624879cc3c8a718d37ff39acb03f839ff41978f49f7716f9fefb6694`.
The 5,411,328-byte DLL has SHA-256
`09af96b95e2abfa39e45f30c8ccb4cb1e8772981dd3be17846f07cbbd2dd8262`
and MVID `dcd73856-39d4-40ce-9b05-77bf249103d7`.

Foundation behavior/runtime qualification is complete. The complete 0.0.115,
0.0.116, and 0.0.117 release gates remain incomplete. Foundation spell affinity, exact
SLA calculation and command behavior, native movement layering, Hydraulic
Push, visual ownership, blueprint publication, and the three-process
module-OFF persistence transaction have passing guarded evidence. The
deterministic suite passed 1,399/1,399.

The affinity predicate now requires the effective ability or one of its
parents to be exact `AbilityType.Spell`, plus reference-identical non-null
spellbook context. It applies once across a variant chain and rejects
SpellLike, item, supernatural, kinetic/nonspell, and context-free calls.
Kingmaker exposes no modifier-descriptor overload on the DC event, so exact
policy nonduplication is the documented engine-compatible equivalent.

The ownership audit found and repaired one concrete issue: visual-cache
rollback could partially remove owned entries before encountering a foreign
replacement. A test-first pure removal plan now validates the complete batch
before any reverse-order cache mutation. Race selector publication, donor
arrays, project proxies, optional entries, and bootstrap rollback ordering
otherwise passed the audit; Elemental Races destroys no Unity object.

The clean foundation package passed strict validation with 135 entries,
22,986,873 bytes, SHA-256
`db18732406bc3facdbeecb3d6305016db49b3fbde74e8bb7987afda4f30ab431`;
its DLL SHA-256 is
`17c6fd96652888aa8ad5781e216b5dab21606c8221f871f17538a7eedb8b6ca9`
and MVID is `112ead36-b1ed-4f1d-9b06-73376d3bd541`. Exact guarded run IDs,
runtime artifact hashes, settings restoration hashes, persistence results, and
the bounded diagnostic failures are maintained in the state and journal.

Release A adds four obligatory three-choice selections and 53 stable manifest
identities without changing the four parent race or legacy provider GUIDs.
All 12 heritage definitions, exact stat overlays, affinity presentations, and
SLA graphs are implemented. The installed donor audit selected native
Firebelly, Flare Burst, Color Spray, Expeditious Retreat, Shocking Grasp, and
Blur; project-owned bounded implementations cover absent Unerring Weapon and
Chill Touch. The complete suite passes 1,407/1,407, and the post-runtime clean
Release build plus independent strict package validation pass.

Guarded Steam run
`20260904T0106348081056Z-7258c85fa8e14ca498201baac7f51ef4`
passed 19/19 live blueprint assertions against DLL SHA-256
`d04710ae349308a51fb7ce814420537b31eb524b7d0b1361212a98911584d5b3`
and MVID `45a12bec-2f12-49af-93cb-a0849d3d48aa`. It proved exact top-level
race counts, selection shape/order, General reference reuse, alternate
SpellLike provider separation, complete presentation, and 53/53 exact live
registrations without touching a save. Runtime-result SHA-256 is
`1acc4b3a2078a45086118330797ce67f463e281f1d3e3545a48cb2383fe53d6d`.
This is implementation proof, not a Release A PASS; the required mechanics,
respec/migration, visual, persistence, and compatibility gates remain pending.

The first dedicated heritage-mechanics run then passed 64/68 and exposed one
real activation-order defect in all four parent races: marker-first hydration
could leave both alternate and inherited General providers active. A narrow
owned controller on the existing trailing heritage-selection fact now performs
post-race reconciliation. Corrected guarded Steam run
`20260904T0152229922454Z-3991ff2bbbb44a2096ce6085328a6b39` passed
68/68 live assertions across all twelve choices and all four transition
matrices. It proves exact live stats, provider/resource uniqueness, exact
multiclass CL and current-Charisma DC calculation, affinity exclusion,
spend/no-level-refill/rest, add-before-remove, explicit and legacy General,
idempotence, and marker-first activation. Runtime-result SHA-256 is
`6ec91796fddfe146a5330505017212895b76a40096e175f767c973d73951bd16`
and companion SHA-256 is
`7a8ab109f8d8d4014f6557e0783ab20d33c47cb9bd93c1432c0976a04f9a2b87`.
The dedicated save-free alternate-SLA run
`20260904T0405120089434Z-cb642458ce4041d989b242982630fda0` then passed
20/20 with zero warnings. It proves native command cancellation, exact
one-use commitment, zero-use blocking, rest recovery, all six donor-backed
effects, exact-item Unerring Weapon duration/confirmation behavior, and both
living and undead Chill Touch delivery with 20 -> 19 persistent charges. It
also proves the explicit Harmony-before contract required alongside the
installed Call of the Wild sticky-touch prefix. Runtime-result SHA-256 is
`80cdc2dd846c5f1de49b3575b522145603f4b243dee3c0314d6dc33d33d5675c`;
companion SHA-256 is
`e34d40ed88e27daf02340359e8c55f1aae971c11706aa7fc9b3570becffb4c7c`.

The final post-runtime package contains 135 entries and is 23,038,804 bytes,
SHA-256
`23014b77c1e43fa85773eee5d09299a65364d057dfa8355ab70504b6c8a9e20b`.
Its 5,603,328-byte DLL has SHA-256
`af9ae270441a898216301e9f612199b85b8d10ac7fc4bd1f2200f684feba5a16`
and MVID `f2980361-84e5-4034-aca7-1e4a4e7a241d`. This remains an interim
Release A qualification checkpoint, not a release PASS: real respec, 0.0.114
migration, three-process persistence, visuals, and compatibility are pending.

## Publication status

Foundation checkpoint
`9c0b7d7bdfe39dd54947c7a37d601cd91db98027`, Release A implementation
checkpoint `543ccdfc91bf2d31916176336985baef6d0720b8`, and reconciliation
qualification checkpoint `aca9aece0933d4713d5eae5cd98e1097fca52325` exist
locally. The exact mandated push wrapper refused each because
`codex/elemental-races-expansion` is absent from its external branch
allowlist; no bypass was attempted. No pull request has been created. Nothing
has been merged, tagged, or publicly released, and no generated release
package is tracked.
