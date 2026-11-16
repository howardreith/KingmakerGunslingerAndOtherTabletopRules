# Expanded Summoning roster coverage: placements, not identities (INT-08)

Status: **development change on a review branch, awaiting review. Not merged, not released.** Branch
`codex/int08-summoning-roster-coverage`, stacked on draft PR #176 at its accepted head `a37a712e85` (which is stacked
on #175 down to #169). Nothing here is a release qualification.

This is one bounded cut of the Gunslinger remediation epic of 2026-11-15: story S3 T7, inventory record INT-08
(Sprint 41 audit finding A-6). Current status is kept in `planning/GUNSLINGER-REMEDIATION-STATUS.md`. It repairs a
report. The selected charter, gameplay catalogs, publication policy and blueprint identities are unchanged.

## Disposition

**Reporting defect, corrected.** `tools/expanded_summoning_ideal_roster.py` now compares ideal placements with
declared choices placement by placement, so a creature's registered identity cannot stand in for a choice in a
spell family, parent tier or quantity. The report is source-derived; no product was built and no game was launched
for it.

Accurate reporting completes INT-08. It does not settle POL-04 (Brachiosaurus in Summon Monster) or POL-05 (the
truncated native-wrapper ladders), and it does not establish that any choice can be cast.

## The two defects

| Defect | What the reporter did | Consequence |
|---|---|---|
| Family-blind registration (A-6) | `coverage()` marked a family `Published` whenever the creature key was in `ExpandedSummoningCatalog`, without checking that the creature had a tier in that family | Brachiosaurus, registered only in Summon Nature's Ally, was reported `Published` in Summon Monster; the Summon Monster headline read 120 of 120 |
| Tier- and quantity-blind wrappers | `coverage()` collected native-wrapper tiers but treated any wrapper in a family as covering the whole family; the wrapper reader did not read the quantity at all | Redcap, Axiomite, Soul Eater, Manticore and Mite were reported `Published` although twelve of their ideal placements have no choice |

What the old reporter (`a37a712e85`) says about the controls, run on the same sources:

| Creature, family | Ideal tier | Old state | Placements actually offered |
|---|---|---|---|
| Brachiosaurus, Summon Monster | 7 | Published | none of 3 |
| Brachiosaurus, Nature's Ally | 7 | Published | 3 of 3 |
| Redcap, Summon Monster | 5 | Published | 3 of 5 |
| Axiomite, Summon Monster | 6 | Published | 3 of 4 |
| Soul Eater, Summon Monster | 6 | Published | 3 of 4 |
| Manticore, Nature's Ally | 5 | Published | 3 of 5 |
| Mite, Nature's Ally | 1 | Published | 3 of 9 |
| Stirge, Nature's Ally | 1 | Registered | 0 of 9, registered and withheld |
| Tyrannosaurus, Summon Monster | 7 | Published | 3 of 3 |

Old family counts: Summon Monster 120 Published; Nature's Ally 109 Published and 1 Registered.

## The repair

- **Ideal placements** come from `ExpandedSummoningIdealRosterCatalog`, unchanged: for each creature and family with
  tier *t*, parent spells *t* to IX at single, 1d3, then 1d4+1.
- **Declared choices are derived independently**, never from the ideal roster: project choices from
  `ExpandedSummoningCatalog` tiers under `GenerateVariants` semantics; retained native wrappers from
  `SummonNativeExpansionCatalog`, with their family (factory), tier and quantity; each with its choice identity
  (`KMG.Summoning.Ability.*` or `KMG.Summoning.NativeOption.*`), checked as an active identity in
  `blueprints/blueprints.json`.
- **Publication policy** is `SummonVisibilityCatalog`'s withheld set; wrappers are published unconditionally, as
  the publisher does.
- **Exact comparison**, keeping multiplicity: each ideal placement is covered (an admitted choice), withheld (a
  registered choice the policy withholds) or missing (no declared choice). Extra choices, duplicated placements and
  choices without a registered identity are listed. Every unpublished placement carries a disposition; one without
  a recorded decision is reported as unexplained.
- **Recorded dispositions are keyed by exact placement**, so a further missing placement of the same creature is
  never covered by POL-04 or POL-05.
- **Input diagnostics**: an ideal, project or wrapper entry that does not read completely, an unreadable withheld
  set or identity manifest, a wrapper naming a creature outside the ideal roster, or a derived total that differs
  from the total the source states for itself, stops generation with an explicit message.
- **Generation and freshness are separate from completeness.** `--check` passes when the documents are current and
  says so, then prints the coverage line, which here reads "incomplete". No message implies complete publication
  because generation succeeded.

`ExpandedSummoningCoveragePolicy` (C#) was inspected and not changed. It checks that the owned creature has a tier
in the requested family, so it does not have the family defect. Its `Published` state means "selectable somewhere
in this family", which is family-level by design; its consumers are the domain tests and the identity policy, not
this report.

## Placement populations

Derived from source at `58e0c4bd01` (the summoning catalogs are byte-identical to v0.0.191):

| Population | Placements |
|---|---|
| Ideal placements | 1,232 |
| Registered project choices | 1,188 (Summon Monster 600, Nature's Ally 588) |
| Registered native wrapper choices | 29 |
| Declared choices | 1,217 |
| Admitted by the publication policy | 1,208 |
| Ideal placements covered | 1,208 |
| Withheld (registered, not admitted) | 9 |
| Missing (no declared choice) | 15 |
| Extra, duplicate, unregistered, unexplained | 0, 0, 0, 0 |

Each derived total matches the total the source states for itself: `UnitCount` 134, `RegisteredLogicalPlacementCount`
1,188, the two `ValidateFamily` totals 600 and 588, `SuppressedLogicalPlacementCount` 9 and
`NativeExpandedOptionIdentityCount` 29.

Creatures: 138 complete in every family the ideal roster places them; 6 partial (Axiomite, Brachiosaurus, Manticore,
Mite, Redcap, Soul Eater); 1 withheld entirely (Stirge). 144 creatures have at least one published placement, which
the report labels "available somewhere" and keeps apart from complete coverage.

## The 24 unpublished ideal placements

| Creature | Family | Parent | Quantity | State | Disposition |
|---|---|---|---|---|---|
| Brachiosaurus | Summon Monster | VII | single | No declared choice | POL-04 / Sprint 5 T3 |
| Brachiosaurus | Summon Monster | VIII | 1d3 | No declared choice | POL-04 / Sprint 5 T3 |
| Brachiosaurus | Summon Monster | IX | 1d4+1 | No declared choice | POL-04 / Sprint 5 T3 |
| Redcap | Summon Monster | VIII, IX | 1d4+1 | No declared choice | POL-05 / Sprint 5 T4 |
| Axiomite | Summon Monster | IX | 1d4+1 | No declared choice | POL-05 / Sprint 5 T4 |
| Soul Eater | Summon Monster | IX | 1d4+1 | No declared choice | POL-05 / Sprint 5 T4 |
| Manticore | Nature's Ally | VIII, IX | 1d4+1 | No declared choice | POL-05 / Sprint 5 T4 |
| Mite | Nature's Ally | IV to IX | 1d4+1 | No declared choice | POL-05 / Sprint 5 T4 |
| Stirge | Nature's Ally | I | single | Registered, withheld | Owner withdrawal, R1 CUT-01 (2026-11-07) |
| Stirge | Nature's Ally | II | 1d3 | Registered, withheld | Owner withdrawal |
| Stirge | Nature's Ally | III to IX | 1d4+1 | Registered, withheld | Owner withdrawal |

That is 3 + 12 missing and 9 withheld. The Stirge placements are a deliberate owner withdrawal, recorded as such and
not as nine implementation defects. Brachiosaurus's three Nature's Ally placements are covered.

## Native evidence, kept separate

The report itself is source-derived. Two native observations bear on it, each under its own identities:

- **Identity-level, current (INT-01 harness listing).** Published v0.0.191 (package SHA-256 `c09ca625…`) beside
  Races Unleashed 1.0.11 in the disposable copy, session `20261116T181244Z`, harness `56e8dd08…`, retained as
  `tests/fixtures/summoning-publication/races-unleashed-parent-census.json`. Compared read-only with the
  source-derived placements: all 1,208 admitted placements were observed, each once and under its correct parent;
  none of the 9 withheld and none of the 15 missing placements was observed; nothing was observed that the source
  does not admit. The other 46 choices under those parents are 38 Spider Summoner rows and 8 Races Unleashed rows,
  an optional-provider population that does not count toward core coverage. The summoning catalogs on this branch
  are byte-identical to v0.0.191. This is a menu observation, not a cast.
- **Count-level, historical (Sprint 41).** `planning/SPRINT41-ROSTER-MENU-RECONCILIATION.md` measured 1,217 live
  choices and 15 unoffered placements on released artifact `b2be3ebe`, before Stirge was withdrawn on 2026-11-07.
  Those figures stay as recorded. They reconcile with today's: 1,217 less Stirge's 9 is 1,208, and 15 plus those 9
  is 24.

No native cast was exercised for any placement in this cut.

## A freshness finding

`planning/EXPANDED-SUMMONING-IDEAL-ROSTER.md` was already stale at the accepted base: it was last regenerated on
2026-11-06 (`8289dc0bc1`) and the Stirge withdrawal of 2026-11-07 (`8472fcafa0`) changed what it should say. No active
validation path runs the generator's `--check`, so nothing noticed. The new focused tests include a check that the
committed documents are current; that test is not yet part of any active validation path either.

## Verification

| Command | Result | Time |
|---|---|---|
| `python -B tools/expanded_summoning_ideal_roster.py` (generate) | writes both documents; reports coverage incomplete, 1,208 of 1,232 | under 1 s |
| `python -B tools/expanded_summoning_ideal_roster.py --check` | current; same coverage line | under 1 s |
| `python -B tools/test_expanded_summoning_ideal_roster.py -v` | 18 of 18 pass, none skipped | 1 s |
| Seven defects reintroduced into the reporter, one at a time | each caught by at least one test; file restored byte for byte | 8 s |
| `python -B tools/validate_development_candidate.py --root . --purpose roster-coverage-report` | PASS | 7 s |
| `python -B tools/test_development_candidate_validator.py` | OK | 2 s |
| Read-only comparison with the retained v0.0.191 census | as above | under 1 s |

The 18 tests run the production readers, comparison and generator on the real source texts, as they are or changed
in one stated way in memory. They establish that registration without a choice in the family does not cover it; a
missing parent tier stays missing beside the others; a wrong quantity fails its placement; same-count replacements
in family, tier or creature cannot pass through totals; duplicated choices stay visible, including a wrapper
duplicating a project choice; suppression is distinct from absent registration; complete valid placements pass;
malformed or incompletely read input stops generation with a named diagnostic; and output is deterministic and
`--check` detects a stale document. The expected discrepancies are written out placement by placement in the
tests.

The reintroduced defects were: registration in any family covering every family (the old A-6 semantics); any
wrapper in a family covering every tier (the old wrapper semantics); quantity ignored in matching; duplicates
collapsed by identity; suppression ignored; unreadable wrapper lines skipped; and a creature reported complete when
anything is published.

The C# domain suite was not run: no C# changed and no C# test reads these documents.

## Limits

- Source-derived publication only. Native menu observation rests on the retained v0.0.191 census above; native
  casting is not established for any placement.
- Assumptions: the Expanded Summoning module enabled; the source's own publication policy; no optional provider in
  the core population.
- POL-04 and POL-05 remain open decisions; the report labels their placements and does not decide them.
- The freshness test is not wired into any active validation path.
