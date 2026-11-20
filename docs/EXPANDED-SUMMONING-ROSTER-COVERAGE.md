# Expanded Summoning roster coverage: placements, not identities (INT-08)

Status: **development item closed: the review accepted draft PR #177 at `dcea649f5e` for its reviewed development
scope. Not merged, not released.** Branch `codex/int08-summoning-roster-coverage`, stacked on draft PR #176 at its
accepted head `a37a712e85` (which is stacked on #175 down to #169). Nothing here is a release qualification. See
"Review acceptance and closeout".

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
validation path ran the generator's `--check`, so nothing noticed. Since the review corrections, the focused suite,
which includes that freshness check, runs in the normal source-validation path (see "Review corrections").

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

## Review corrections (review of `4d2e1bfb10`)

The review accepted the placement comparison and the current census in principle and found three things to
correct. They are made in `d06e3b7626`. The figures for the current sources are unchanged: 1,232 ideal placements;
1,188 project choices and 29 native wrappers; 1,208 admitted; nine Stirge placements deliberately withheld; fifteen
missing under POL-04 and POL-05; no unexplained placement. The native evidence above is unchanged, and no native
casting was established.

**1. Unrecorded withholding was not counted as unexplained.** A withheld placement with no recorded reason was
labelled "no recorded reason", but the unexplained count matched only dispositions beginning "Unexplained", so it
dropped out of the headline and of "Unpublished placements without a recorded disposition". Each unpublished
placement now carries an explicit `recorded` flag, independent of its state and of the wording shown: a placement
may be withheld or missing, and either may be explained or not. POL-04 and POL-05 stay keyed by exact placement, and
the Stirge withdrawal stays recorded. The unexplained total is split by state, and the discrepancy table gains a
"Decision on record" column.

**2. Creature availability was read from display strings.** A family with none published, some withheld and some
missing was labelled "Partial 0/N", and anything labelled "Partial" counted as published. Each creature's covered,
withheld and missing placements are now counted, and its label, its category and the "available somewhere" count
are derived from those counts. Available somewhere requires a covered placement; complete requires every one. A
category is added for none published with withheld and missing placements mixed, and registered-withheld placements
are never described as unregistered. Partial labels now give the unpublished placements by state, for example
"Partial 3/9 (6 missing)".

**3. The suite is in the normal source-validation path.** `tools/test_expanded_summoning_ideal_roster.py` runs once in
the unconditional source fixtures of `scripts/validate-repository.ps1`, before the deterministic-package fixtures,
with a non-zero exit code turned into a thrown failure as the wrapper's other fixtures are. Its
`test_the_committed_documents_are_current` is the freshness check; it is not run a second time, and nothing
regenerates the documents during validation. A current report that accurately shows 1,208 of 1,232 passes; a stale
one fails.

What the reviewed head produced on the review's fixtures, through the production `build()` on coherent in-memory
sources (recorded before any change):

| Fixture | Reviewed head `4d2e1bfb10` | After correction |
|---|---|---|
| Brachiosaurus withheld beside Stirge, withheld count 9 to 12 | 3 withheld SNA placements labelled "no recorded reason", yet 0 unexplained in the summary and in the table; Brachiosaurus under "No ideal placement published or registered" | 3 unexplained (3 withheld, 0 missing); the 9 Stirge placements explained; Brachiosaurus "Missing 0/3" and "Withheld 0/3", in the mixed category; 143 available somewhere |
| Stirge registered at SNA II, constants 1,188 to 1,187, 9 to 8, 588 to 587 | "Partial 0/9", listed under "Some ideal placements published", 145 available somewhere | "Unpublished 0/9 (6 withheld, 3 missing)", mixed category, 144 available somewhere |

On the second fixture the review expected eight withheld placements and one missing. Exact matching gives six
withheld, three missing and two extra choices. Registered at II, Stirge's choices are II single, III 1d3 and IV to IX
1d4+1, because the quantity follows the registered tier; against the ideal I single, II 1d3 and III to IX 1d4+1, only
IV to IX match. The tests assert what exact matching yields. The behaviour the review asked for holds: no covered
Stirge placement, a mixed withheld and missing state described as such, and 144 creatures available somewhere.

### Verification of the corrections

| Check | Result | Time |
|---|---|---|
| Review fixtures through `build()` on the reviewed head | the three wrong summaries above reproduced | under 1 s |
| `python -B tools/test_expanded_summoning_ideal_roster.py -v` | 22 of 22 pass, none skipped (18 earlier, 4 new) | 1 s |
| The same suite against the reviewed head's reporter | 9 of 22 fail (3 failures, 6 errors on the new fields) | 1 s |
| Generator, write then `--check` | regenerated deterministically; current; coverage line unchanged | under 1 s |
| `powershell -File scripts/Test-RepositoryValidatorDispatch.ps1` | PASS, 46 checks (39 existing, 7 new): suite referenced once; run once by the source fixtures, before the deterministic-package fixtures; its failure throws and nothing downstream runs; neither version dispatch runs it again | 1 s |
| The dispatch test on a wrapper copy whose roster call lost its exit-code check | FAILED, 2 of 46: the failure does not stop validation, and downstream fixtures run | 1 s |
| The suite in a disposable source copy with a stale report | fails on `test_the_committed_documents_are_current` only; the stale copy is not regenerated | 1 s |
| The suite in the same copy before the report was altered | passes | 1 s |
| `powershell -File scripts/validate-repository.ps1 -DevelopmentPurpose roster-coverage-report -DevelopmentSummaryPath <scratch>` | PASS, exit 0: the roster suite ran once (22 of 22) in the source fixtures; development validation accepted the branch; the wrapper's dispatch regression passed 46 checks; every other child passed | 428 s |

The earlier results under "Verification" belong to head `4d2e1bfb10` and stay as recorded.

## Review acceptance and closeout

The review of draft PR #177 at `dcea649f5e2a3ee1742e13592ebe76cc638ba286` accepted it for its reviewed development
scope. INT-08 / S3 T7 is closed as a development item. Nothing was rebuilt or rerun for this closeout. Review
acceptance, merge and release are separate states; PR #177 stays an open draft and nothing is merged or released.

The accepted result of the Stirge fixture (registered at Nature's Ally II) is six withheld ideal-placement matches,
three missing ideal placements and two extra declared choices. The earlier review's expectation of eight withheld
and one missing was mistaken: the quantity follows the registered tier.

Unchanged by the closeout: the figures and evidence above under their original heads; the limits below; POL-04 and
POL-05, which remain Sprint 5 decisions.

## Later: POL-04 decided (S5 T3, 2026-11-19)

The figures above stay as accepted at INT-08, under their original heads.

**Decision:** the owner selected Alternative A, recorded as A-records (`docs/EXPANDED-SUMMONING-BRACHIOSAURUS-SCOPE.md`,
draft PR #188). It reaffirms the 2026-10-15 decision recorded in the `ExpandedSummoningCatalog` comment. Brachiosaurus
stays Summon Nature's Ally only.

**Approved out of scope:** exactly the three original-charter placements Summon Monster VII single, VIII 1d3 and IX
1d4+1. This is a deliberate product scope decision, not a gameplay repair and not fulfilment of those placements. The
Core Rulebook Summon Monster VII table includes Brachiosaurus
(<https://legacy.aonprd.com/coreRulebook/spells/summonMonster.html>), so the charter placement was rules-supported.

**What the reporter shows now:**
- The ideal roster and its 1,232 total are unchanged.
- From unchanged source: 1,208 admitted + 9 withheld + 12 missing (POL-05 only) + 3 approved out of scope = 1,232.
- The admitted count does not change.
- Brachiosaurus is reported as published within the approved scope, not as having every original ideal placement
  published.
- An approved placement that the source nevertheless declares is a scope conflict, which `--validate-scope` refuses.

**Native evidence:** the retained v0.0.191 parent census above is now also credited per identity in
`planning/expanded-summoning-native-placement-evidence.json`, under its own session and package.

POL-05 remained open at that point.

## Later: POL-05 decided (S5 T4, 2026-11-19)

The figures above stay as accepted, under their original heads.

**Decision:** the owner selected Alternative A (`docs/EXPANDED-SUMMONING-WRAPPER-LADDER-SCOPE.md`, draft PR #189): the five retained
native wrappers Redcap, Axiomite, Soul Eater, Manticore and Mite keep Owlcat's base-plus-two ladders.

**Approved out of scope:** exactly twelve 1d4+1 placements, each recorded separately:
- Redcap Summon Monster VIII and IX;
- Axiomite Summon Monster IX;
- Soul Eater Summon Monster IX;
- Manticore Summon Nature's Ally VIII and IX;
- Mite Summon Nature's Ally IV, V, VI, VII, VIII and IX.

No native option exists for those exact creature-and-tier combinations. The decision narrows the original promise; it
is not a gameplay repair and not fulfilment of those placements.

**What the reporter shows now:**
- From unchanged source: 1,208 admitted + 9 withheld + 0 missing + 15 approved out of scope = 1,232.
- The admitted count does not change, and the original ideal is still reported as incomplete.
- The five creatures, with Brachiosaurus, are reported as published within the approved scope.
- A declared choice for any excluded placement is a scope conflict, which `--validate-scope` refuses.

Implemented in the reporter and records only; accepted and closed for its reviewed Alternative A tooling and records development scope (principal review, 2026-11-19, at `acd50a042a`). The 15 existing wrapper rows, their 52 adapters, the
nine Stirge withholdings and the compiled coverage model are unchanged.

## Limits

Historical INT-08 limitations, as accepted at `dcea649f5e`:
- Source-derived publication only. Native menu observation rests on the retained v0.0.191 census above; native
  casting is not established for any placement.
- Assumptions: the Expanded Summoning module enabled; the source's own publication policy; no optional provider in
  the core population.
- At INT-08, POL-04 and POL-05 were open decisions; the report labelled their placements and did not decide them.
- The freshness check runs in source validation, which `Build-Local` runs; it does not run on its own schedule, so a
  stale report is caught when validation next runs, not when a catalog changes.

Current dispositions:
- POL-04 is decided and accepted (S5 T3, A-records, at `ada0ff2650`): the three Brachiosaurus Summon Monster
  placements are approved out of scope, as described in "Later: POL-04 decided".
- POL-05 is decided and accepted (S5 T4, Alternative A, reporter and records, at `acd50a042a`): the twelve higher
  native-wrapper placements are approved out of scope, as described in "Later: POL-05 decided".
