# Elemental Races expansion implementation report

## Current outcome

**IN PROGRESS - FOUNDATION PASS; RELEASES A, B, AND C REMAIN PENDING.**

The mission began from clean authoritative `master` commit
`6874dc15a27ded132456dbdd480f47c794543a05` on dedicated branch
`codex/elemental-races-expansion`. The first work phase is the required
0.0.114 foundation audit and hardening is complete and independently
qualified. Historical Elemental Races evidence is preserved as historical
evidence only and does not qualify new release behavior.

## Planned release inventory

| Release | Version | Scope | Status |
| --- | --- | --- | --- |
| Foundation | 0.0.114 baseline | affinity, SLA, movement/maneuver, ownership, runtime organization | PASS |
| A | 0.0.115-elemental-heritages | twelve heritage choices under four parent races | NOT STARTED |
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

Foundation behavior/runtime qualification is complete. Every 0.0.115, 0.0.116,
and 0.0.117 release gate remains incomplete. Foundation spell affinity, exact
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

## Publication status

No pull request has been created. Nothing has been merged, tagged, or publicly
released, and no generated release package is tracked.
