# Brachiosaurus spell-family scope (POL-04 / S5 T3): decision record

Status: **decided, Alternative A recorded as A-records; returned for principal review.** S5 T3 is not accepted until
principal review.
- Brachiosaurus stays available through Summon Nature's Ally only.
- Exactly three original-charter placements are approved out of scope: Summon Monster VII single, VIII 1d3 and IX 1d4+1.
- This is a product scope decision. It is not a gameplay repair and not fulfilment of the three original Monster
  promises.

Branch `codex/pol04-brachiosaurus-scope`, draft PR #188, stacked on PR #187 (accepted S5 T2 records head
`bbc8174f05`). Not merged, not released.

**What changed on this branch:** tooling (`tools/expanded_summoning_ideal_roster.py`), its tests
(`tools/test_expanded_summoning_ideal_roster.py`), a native-evidence ledger and records. Compiled product source,
blueprints and assets have not changed. No package was built, no C# was recompiled for this change, and nothing ran in
the game.

## The decision and its provenance

**Original decision, 2026-10-15 (owner).** Recorded in the `ExpandedSummoningCatalog` comment beside the Brachiosaurus
row:

> The owner settled that on 2026-10-15 against a recorded source discrepancy - the printed Summon Monster VII list and
> this project's own ideal-roster catalog both also place it on the Monster table - choosing the reversible direction,
> because adding Monster roots later is additive while withdrawing published ones is not.

**Reaffirmed, 2026-11-19 (S5 T3 principal review):** Alternative A, recorded as A-records.

**Rules citation:** the Core Rulebook Summon Monster spell, <https://legacy.aonprd.com/coreRulebook/spells/summonMonster.html>.
- Its standard seventh-level table includes Brachiosaurus.
- The spell's ordinary progression (one creature from the same-level table, 1d3 from the table one level lower, 1d4+1
  from a lower table) supports exactly the charter's three placements: VII single, VIII 1d3 and IX 1d4+1.
- The charter promise is therefore rules-supported. Its exclusion is a deliberate product-scope choice, made knowingly
  against both the printed table and the ideal roster.

**Rationale:** reversibility. Monster placements can be added later as an append-only change, whereas published
identities cannot be withdrawn without a load-time fixer. The decision keeps that option open and keeps the shipped,
qualified Nature's Ally scope unchanged.

**Historical release notes keep their original identity.** `docs/RELEASE-NOTES-0.0.159.md` says Brachiosaurus "is not on
the Summon Monster table". That sentence describes this project's Summon Monster table as shipped in v0.0.159, not the
printed Core Rulebook table. Those notes stay unchanged; this record carries the clarification.

## 1. Brachiosaurus as shipped (unchanged)

- **Creature key:** `brachiosaurus`.
- **Catalog:** `C("brachiosaurus","Brachiosaurus",null,false,7,"Mastodon")`. That is no Summon Monster tier, not
  Monster-templated, Summon Nature's Ally tier 7, with the Mastodon visual/body donor. Untemplated: "Caster alignment;
  never celestial/fiendish".
- **Identities:** 19 active ledger identities, none Summon Monster. They are all preserved:

| Kind | Symbols |
|---|---|
| Unit | `KMG.Summoning.Unit.Brachiosaurus` `9093aef3…` |
| Canonical Nature's Ally abilities | `SNA.Tier7.Brachiosaurus.One` `5ff406f7…`, `SNA.Tier8.Brachiosaurus.OneD3` `7ab83d22…`, `SNA.Tier9.Brachiosaurus.OneD4PlusOne` `10a645ec…` |
| Natural attacks | `Natural.Brachiosaurus.Tail4d6`, `.TailType`, `.UnitType`, `.Chassis` |
| Specials | `Special.Brachiosaurus.Trample`, `.Overrun`, `.GreaterOverrun` |
| Alternate adapters (Nature's Ally families) | Naturalist Tier 7/8/9, Feral Hunter Tier 7/8/9, Preservationist Tier 7 and 9 (Bottled Ally has no Summon Nature's Ally VIII) |

- **Routes:**
  - canonical Summon Nature's Ally VII (single), VIII (1d3) and IX (1d4+1), through the donor carriers `6d8d59aa…`,
    `256739c1…` and `9bd8cb61…`;
  - alternate: the Naturalist, Feral Hunter and Preservationist adapters only.
- No Monster root, template child or Summon Monster adapter is added.

## 2. Evidence, at its actual strength

| Evidence | Brachiosaurus SNA VII / VIII / IX | Brachiosaurus SM VII / VIII / IX |
|---|---|---|
| Registered identity (ledger) | yes, yes, yes | none |
| Declared and admitted (source catalogs) | yes, yes, yes | none (approved out of scope) |
| Graph built in game, current product line | yes: all three canonical identities and all eight adapters, in the accepted S5 T1 census `20261119T201715Z-owner-0.33.0-inherited-template-census` (disposition `OutsideSummonMonster`) | none |
| Per-placement native menu observation | yes: each listed once under its own parent (Summon Nature's Ally VII, VIII, IX) in the retained parent census of **published v0.0.191** (session `20261116T181244Z-owner-0.33.0-summoning-publication-boundary`, package `c09ca625…`, Races Unleashed 1.0.11 beside it; fixture `tests/fixtures/summoning-publication/races-unleashed-parent-census.json`). That is a menu observation of v0.0.191, not of current bytes | none: no Summon Monster row exists in that census |
| Native spell cast, credited per placement in the ledger | none mapped (see below) | none |

The decision packet at `800b615a02` said no per-placement menu observation was on record. That was wrong; the retained
v0.0.191 census above is one, and it is now credited in the ledger under its own session and package.

**Historical casts.** The Sprint 25 release record (`docs/SPRINT25-COVERAGE.md`) reports, under its historical product
identity `d81ac44546997cc6c812ebec51d9fe6cc962a3e4` (v0.0.159; ZIP `2625a976…`, DLL MVID `bb13331b-…`):
- "combined and standalone real casts";
- with quantity, template, alignment, slot, duration and cleanup checks;
- all three cells passing.

That record reports cast qualification of the Brachiosaurus placements as shipped then. Its exact per-placement
receipts have not been mapped into the new native-evidence ledger. So the ledger's cast fields for these identities stay
uncredited, and nothing from that old run is credited to current bytes.

The Sprint 41 live menu sizes (released artifact `b2be3ebe`: Summon Monster VII, VIII and IX at 103, 111 and 116) are
aggregate counts per menu, not per-creature observations, and are not credited to any placement.

## 3. Original ideal and compiled reference model (unchanged)

- **Original ideal:** the frozen ideal manifest `ExpandedSummoningIdealRosterCatalog.cs` keeps
  `R("brachiosaurus","Brachiosaurus",7,7,25,Essential,High,false,"Sauropod dinosaur")` and the original charter figures
  (145 creatures; 120 Summon Monster and 110 Nature's Ally base entries; 1,232 ideal placements).
- **Compiled coverage:** `ExpandedSummoningIdealRosterCatalog` and the compiled `ExpandedSummoningCoveragePolicy` remain
  derivations against that original charter, and neither was changed. `ExpandedSummoningCoveragePolicy` may still
  report these three Monster cells as `Planned`.
- **Selected development scope:** defined by the approved exclusions in the reporter
  (`APPROVED_OUT_OF_SCOPE`) and in this record. Nothing here claims that the compiled coverage changed, or that the
  original ideal is fully published.

## 4. Reporter: approved exclusions, scope conflicts and publication versus scope

`tools/expanded_summoning_ideal_roster.py` records the exact three approvals, each with this rationale and provenance
(`POL04_DECISION`).
- **Pending state:** `SCOPE_PENDING` is now empty.
- **Untouched:** no POL-05 placement is waived, and the nine Stirge withdrawals are unchanged.

**Approved exclusion versus publication:**
- *Excluded cell:* an approved placement with no declared choice is `out-of-scope`, which is excluded, not published and
  never counted as covered. Approval never raises the admitted count.
- *Category:* a creature whose in-scope placements are all published and whose others are approved out of scope is in
  its own category, "Every ideal placement in the approved scope published; the others approved out of scope, not
  published". It is no longer reported as "Every ideal placement published".
- *Visibility:* headlines, the accounting line, the comparison table, the unpublished-placement table, a new "Approved
  scope exclusions" table, the creature table ("Approved out of scope 0/3") and `--cells` all show the exclusions.

**Scope conflict:**
- *When it applies:* an approved placement that the source nevertheless declares.
- *Observations kept:* its actual registration, declaration and admission are preserved (for example covered, and
  registered "yes").
- *Explicit verdict:* the summary, the generated document and `--cells` all carry a `SCOPE CONFLICT` verdict naming the
  exact cells. It is never shown as a resolved exclusion.
- *Validation:* `--validate-scope` exits 1 and names each conflict.
- *Freshness is separate:* `--check` keeps its documented freshness role and states that it does not validate scope.

**Output on the unchanged sources** (`--check`):

> Source-derived coverage is incomplete: 1,208 of 1,232 ideal placements admitted; 9 withheld; 12 missing; 3 approved
> out of scope; 0 extra; 0 duplicate; 0 unexplained; scope contract: 3 approved exclusions, 0 scope conflicts.

> Accounting against the original ideal: 1,208 admitted + 9 withheld + 12 missing + 3 approved out of scope = 1,232
> ideal placements.

`--validate-scope`: "Scope contract valid: 3 approved exclusions, none declared, admitted or published."

`python tools/expanded_summoning_ideal_roster.py --cells brachiosaurus,tyrannosaurus,stegosaurus`:

| Creature | Family | Parent | Quantity | Original ideal | Scope | Exact identity | Registered | Declared | Admitted | Native graph | Native menu | Native cast |
|---|---|---|---|---|---|---|---|---|---|---|---|---|
| brachiosaurus | Summon Monster | VII | single | yes | approved out of scope | `KMG.Summoning.Ability.SM.Tier7.Brachiosaurus.One` | no | no | no | none | none | none |
| brachiosaurus | Summon Monster | VIII | 1d3 | yes | approved out of scope | `KMG.Summoning.Ability.SM.Tier8.Brachiosaurus.OneD3` | no | no | no | none | none | none |
| brachiosaurus | Summon Monster | IX | 1d4+1 | yes | approved out of scope | `KMG.Summoning.Ability.SM.Tier9.Brachiosaurus.OneD4PlusOne` | no | no | no | none | none | none |
| brachiosaurus | Summon Nature's Ally | VII | single | yes | in scope | `KMG.Summoning.Ability.SNA.Tier7.Brachiosaurus.One` | yes | yes | yes | PASS:20261119T201715Z-owner-0.33.0-inherited-template-census@3ef1bce7027b | PASS:20261116T181244Z-owner-0.33.0-summoning-publication-boundary@c09ca6255fee | none |
| brachiosaurus | Summon Nature's Ally | VIII | 1d3 | yes | in scope | `KMG.Summoning.Ability.SNA.Tier8.Brachiosaurus.OneD3` | yes | yes | yes | PASS:20261119T201715Z-owner-0.33.0-inherited-template-census@3ef1bce7027b | PASS:20261116T181244Z-owner-0.33.0-summoning-publication-boundary@c09ca6255fee | none |
| brachiosaurus | Summon Nature's Ally | IX | 1d4+1 | yes | in scope | `KMG.Summoning.Ability.SNA.Tier9.Brachiosaurus.OneD4PlusOne` | yes | yes | yes | PASS:20261119T201715Z-owner-0.33.0-inherited-template-census@3ef1bce7027b | PASS:20261116T181244Z-owner-0.33.0-summoning-publication-boundary@c09ca6255fee | none |
| tyrannosaurus | Summon Monster | VII | single | yes | in scope | `KMG.Summoning.Ability.SM.Tier7.Tyrannosaurus.One` | yes | yes | yes | PASS:20261119T201715Z-owner-0.33.0-inherited-template-census@3ef1bce7027b | PASS:20261116T181244Z-owner-0.33.0-summoning-publication-boundary@c09ca6255fee | none |
| tyrannosaurus | Summon Monster | VIII | 1d3 | yes | in scope | `KMG.Summoning.Ability.SM.Tier8.Tyrannosaurus.OneD3` | yes | yes | yes | PASS:20261119T201715Z-owner-0.33.0-inherited-template-census@3ef1bce7027b | PASS:20261116T181244Z-owner-0.33.0-summoning-publication-boundary@c09ca6255fee | none |
| tyrannosaurus | Summon Monster | IX | 1d4+1 | yes | in scope | `KMG.Summoning.Ability.SM.Tier9.Tyrannosaurus.OneD4PlusOne` | yes | yes | yes | PASS:20261119T201715Z-owner-0.33.0-inherited-template-census@3ef1bce7027b | PASS:20261116T181244Z-owner-0.33.0-summoning-publication-boundary@c09ca6255fee | none |
| tyrannosaurus | Summon Nature's Ally | VII | single | yes | in scope | `KMG.Summoning.Ability.SNA.Tier7.Tyrannosaurus.One` | yes | yes | yes | PASS:20261119T201715Z-owner-0.33.0-inherited-template-census@3ef1bce7027b | PASS:20261116T181244Z-owner-0.33.0-summoning-publication-boundary@c09ca6255fee | none |
| tyrannosaurus | Summon Nature's Ally | VIII | 1d3 | yes | in scope | `KMG.Summoning.Ability.SNA.Tier8.Tyrannosaurus.OneD3` | yes | yes | yes | PASS:20261119T201715Z-owner-0.33.0-inherited-template-census@3ef1bce7027b | PASS:20261116T181244Z-owner-0.33.0-summoning-publication-boundary@c09ca6255fee | none |
| tyrannosaurus | Summon Nature's Ally | IX | 1d4+1 | yes | in scope | `KMG.Summoning.Ability.SNA.Tier9.Tyrannosaurus.OneD4PlusOne` | yes | yes | yes | PASS:20261119T201715Z-owner-0.33.0-inherited-template-census@3ef1bce7027b | PASS:20261116T181244Z-owner-0.33.0-summoning-publication-boundary@c09ca6255fee | none |
| stegosaurus | Summon Nature's Ally | VI | single | yes | in scope | `KMG.Summoning.Ability.SNA.Tier6.Stegosaurus.One` | yes | yes | yes | PASS:20261119T201715Z-owner-0.33.0-inherited-template-census@3ef1bce7027b | PASS:20261116T181244Z-owner-0.33.0-summoning-publication-boundary@c09ca6255fee | none |
| stegosaurus | Summon Nature's Ally | VII | 1d3 | yes | in scope | `KMG.Summoning.Ability.SNA.Tier7.Stegosaurus.OneD3` | yes | yes | yes | PASS:20261119T201715Z-owner-0.33.0-inherited-template-census@3ef1bce7027b | PASS:20261116T181244Z-owner-0.33.0-summoning-publication-boundary@c09ca6255fee | none |
| stegosaurus | Summon Nature's Ally | VIII | 1d4+1 | yes | in scope | `KMG.Summoning.Ability.SNA.Tier8.Stegosaurus.OneD4PlusOne` | yes | yes | yes | PASS:20261119T201715Z-owner-0.33.0-inherited-template-census@3ef1bce7027b | PASS:20261116T181244Z-owner-0.33.0-summoning-publication-boundary@c09ca6255fee | none |
| stegosaurus | Summon Nature's Ally | IX | 1d4+1 | yes | in scope | `KMG.Summoning.Ability.SNA.Tier9.Stegosaurus.OneD4PlusOne` | yes | yes | yes | PASS:20261119T201715Z-owner-0.33.0-inherited-template-census@3ef1bce7027b | PASS:20261116T181244Z-owner-0.33.0-summoning-publication-boundary@c09ca6255fee | none |

The unrelated, correctly published control with native cast evidence (`--cells air-mephit`, excerpt):

| Creature | Family | Parent | Quantity | Original ideal | Scope | Exact identity | Registered | Declared | Admitted | Native graph | Native menu | Native cast |
|---|---|---|---|---|---|---|---|---|---|---|---|---|
| air-mephit | Summon Monster | IV | single | yes | in scope | `KMG.Summoning.Ability.SM.Tier4.AirMephit.One` | yes | yes | yes | PASS:20261119T201715Z-owner-0.33.0-inherited-template-census@3ef1bce7027b | PASS:20261116T181244Z-owner-0.33.0-summoning-publication-boundary@c09ca6255fee | PASS:20261119T201938Z-owner-0.33.0-inherited-template-casts@3ef1bce7027b |
| air-mephit | Summon Monster | VI | 1d4+1 | yes | in scope | `KMG.Summoning.Ability.SM.Tier6.AirMephit.OneD4PlusOne` | yes | yes | yes | PASS:20261119T201715Z-owner-0.33.0-inherited-template-census@3ef1bce7027b | PASS:20261116T181244Z-owner-0.33.0-summoning-publication-boundary@c09ca6255fee | PASS:20261119T201938Z-owner-0.33.0-inherited-template-casts@3ef1bce7027b |
| air-mephit | Summon Nature's Ally | IV | single | yes | in scope | `KMG.Summoning.Ability.SNA.Tier4.AirMephit.One` | yes | yes | yes | PASS:20261119T201715Z-owner-0.33.0-inherited-template-census@3ef1bce7027b | PASS:20261116T181244Z-owner-0.33.0-summoning-publication-boundary@c09ca6255fee | none |

Every native-evidence cell names its own session and product package (`PASS:<session>@<package>`): graph and cast
evidence from the accepted S5 T1 product `3ef1bce7027b`, menu evidence from published v0.0.191 `c09ca6255fee`.

**Inspected false-pass case: a coherently registered and declared Monster Brachiosaurus beside the approvals** (in
memory only):
- *Summary:* "1,211 of 1,232 ideal placements admitted; ... 0 approved out of scope; ... scope contract: 3 approved
  exclusions, 3 SCOPE CONFLICT (declared despite approval: brachiosaurus SM VII One, brachiosaurus SM VIII OneD3,
  brachiosaurus SM IX OneD4PlusOne)".
- *Document:* "**SCOPE CONFLICT**: declared despite the approval (admitted)" on each cell, with the conflict count at 3.
- *`--cells`:* registered, declared and admitted "yes", with scope "SCOPE CONFLICT: approved out of scope but declared".
- *`--validate-scope`:* exits 1.

## 5. Alternative B, not selected

Publishing the three Monster cells would need:
- 21 append-only identities (3 abilities, 6 Celestial/Fiendish children and 12 Summon Monster adapters);
- the Tyrannosaurus donors `2920d485…`, `ddc1d195…` and `22b9580c…`;
- moved catalog, census, menu and validator counts;
- its own candidate and native qualification.

It stays available later as an additive change; the reversibility rationale above depends on that.

## Records reconciled

- The generated `planning/EXPANDED-SUMMONING-IDEAL-ROSTER.md`.
- `docs/EXPANDED-SUMMONING-ROSTER-COVERAGE.md`: its accepted INT-08 figures keep their identity, with a dated S5 T3
  note.
- `KNOWN-ISSUES.md`, `planning/GUNSLINGER-REMEDIATION-STATUS.md` and the PR #188 body.

**Kept unchanged:**
- the original 1,232 reference total;
- the 12 POL-05 missing cells;
- the nine Stirge withholdings and their dispositions;
- S5 T2's acceptance and both aggregate-FAIL sessions;
- the unclassified console-learning issue (not touched);
- the DEF-05 next-release-validator obligation.

## Focused checks

| Check | Result | Time |
|---|---|---|
| `python -B tools/test_expanded_summoning_ideal_roster.py` | 33 of 33 | 1 s |
| `python -B tools/expanded_summoning_ideal_roster.py` (regenerate) | documents written; 1,208 + 9 + 12 + 3 = 1,232 | under 1 s |
| `python -B tools/expanded_summoning_ideal_roster.py --check` | documents current (freshness only) | under 1 s |
| `python -B tools/expanded_summoning_ideal_roster.py --validate-scope` | scope contract valid | under 1 s |
| `git diff --check` | clean | — |

No package was built, no C# was recompiled for this change, no native session ran, and nothing was installed. The
earlier decision-packet commit `800b615a02` compiled production once through the focused domain gate (11 of 11), with no
product source changed.
