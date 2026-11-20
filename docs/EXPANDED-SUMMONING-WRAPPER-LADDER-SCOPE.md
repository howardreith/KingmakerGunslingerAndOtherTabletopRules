# Native-wrapper ladder scope (POL-05 / S5 T4): owner-decision packet

Status: **owner-decision checkpoint.** None of the twelve cells is approved or published. No gameplay, ideal-catalog
or runtime-publication change has been made. Principal review selects A or B.

Branch `codex/pol05-wrapper-ladder-scope`, draft PR #189, stacked on PR #188 (accepted S5 T3 head `ada0ff2650`). Not
merged, not released.

**What changed on this branch:**
- the S5 T3 acceptance records;
- a bounded reporter correction for these exact cells, with tests (below);
- the native-evidence ledger;
- this packet.

Compiled product source, blueprints and assets have not changed. No package, recompilation, native session or
installation.

## The question

The original ideal roster (charter) gives every creature a full ladder: single at its tier, 1d3 at the next and 1d4+1 at
every tier above that, through IX. These five creatures ship as **retained native wrappers**, which are KMG-owned,
creature-named clones of Owlcat's own summon options. Owlcat authored each of them only on a base-plus-two ladder, so
twelve 1d4+1 cells have no choice:

| Cell | Family | Parent | Quantity | Highest existing same-creature row |
|---|---|---|---|---|
| Redcap | Summon Monster | VIII | 1d4+1 | SM VII 1d4+1 |
| Redcap | Summon Monster | IX | 1d4+1 | SM VII 1d4+1 |
| Axiomite | Summon Monster | IX | 1d4+1 | SM VIII 1d4+1 |
| Soul Eater | Summon Monster | IX | 1d4+1 | SM VIII 1d4+1 |
| Manticore | Summon Nature's Ally | VIII | 1d4+1 | SNA VII 1d4+1 |
| Manticore | Summon Nature's Ally | IX | 1d4+1 | SNA VII 1d4+1 |
| Mite | Summon Nature's Ally | IV, V, VI, VII, VIII, IX (six cells) | 1d4+1 | SNA III 1d4+1 |

**Rules context.** The Core Rulebook summon spells allow 1d4+1 creatures from a lower-level list
(<https://legacy.aonprd.com/coreRulebook/spells/summonMonster.html>), so the original ideal's higher 1d4+1 cells are
rules-supported. Either alternative is a product-scope choice, not a rules correction.

## How a retained native wrapper is built today

Source: `SummonNativeExpansionCatalog` and `ExpandedSummoningNativeOptionBuilder`, as read from source; not re-executed.
- **Source:** each catalog row names an exact Owlcat summon option (`SourceAbilityGuid`) and a unit.
- **Build:** the builder deep-clones that option's components:
  - spell component;
  - duration, spawn count expression and rank configuration;
  - pool and after-spawn actions.
- **Branch:** it keeps only the spawn branch for the row's unit. A Summon Monster umbrella has a NonEvil and an Evil
  branch; a Summon Nature's Ally option has one direct spawn.
- **Summon Monster rows:** the builder also attaches the cleric alignment gate.
- **Parent:** the publisher places the row under the parent of its own tier, so it converts from that parent and spends
  one source of that parent (a slot or spontaneous use, or an alternate family's resource or extract through its
  adapter).
- **Template:** none added. The wrapper keeps the native option's own after-spawn behaviour. The S5 T1 inherited-template
  policy applies to generated placements, not to these wrappers.
- **Module off:** the original options are restored.
- **Spawn replacement:** the builder has one spawn-replacement mode (`ReplacesSpawnUnit`), used for the Frost Giant at
  Summon Nature's Ally VII–IX, carved from the native Mastodon options. It requires a single direct spawn.

## Audit of the twelve cells

Every cell shares these facts:
- **Original ideal:** yes.
- **Disposition:** `missing`, with the recorded POL-05 disposition, and now in the pending-owner-decision set.
- **Exact identity:** none declared.
- **Registered / declared / admitted:** no / no / no, each read separately; no ledger identity exists for any of them.
- **Native graph, menu, cast:** none.
- **Alternate routes:** none today.

| Cell | Same creature's highest existing wrapper (identity; native source; branch; unit) | Native 1d4+1 option at the cell's own parent (candidate carve donor) |
|---|---|---|
| Redcap SM VIII | `NativeOption.SM.Tier7.Redcap.OneD4PlusOne` `c8f60413…`; `SummonMonsterVIId4plus1` `6e805a9e…`; Evil; `af8bcff0…` | `SummonMonsterVIIId4plus1` `12815be3…`: two branches (Axiomite NonEvil, Soul Eater Evil) |
| Redcap SM IX | as above | `SummonMonsterIXd4plus1` `22b9580c…`: one direct spawn (Bogeyman) |
| Axiomite SM IX | `NativeOption.SM.Tier8.Axiomite.OneD4PlusOne` `25f16a94…`; `SummonMonsterVIIId4plus1` `12815be3…`; NonEvil; `5adafaa6…` | `22b9580c…`: one direct spawn (Bogeyman) |
| Soul Eater SM IX | `NativeOption.SM.Tier8.SoulEater.OneD4PlusOne` `2b816c03…`; `12815be3…`; Evil; `1832be68…` | `22b9580c…`: one direct spawn (Bogeyman) |
| Manticore SNA VIII | `NativeOption.SNA.Tier7.Manticore.OneD4PlusOne` `74e71679…`; `SummonNaturesAllyVIId4plus1` `b81bb947…`; direct; `7b7701ff…` | `SummonNaturesAllyVIIId4plus1` `86f42875…` (catalogued equivalent: dire tiger) |
| Manticore SNA IX | as above | `SummonNaturesAllyIXd4plus1` `9bd8cb61…` (Mastodon; also the Frost Giant IX donor) |
| Mite SNA IV | `NativeOption.SNA.Tier3.Mite.OneD4PlusOne` `cf7dd0af…`; `SummonNaturesAllyIIId4plus1` `bb1bac85…`; direct; `0c433dcd…` | `SummonNaturesAllyIVd4plus1` `3050599c…` (giant frog) |
| Mite SNA V | as above | `SummonNaturesAllyVd4plus1` `87c64591…` (leopard) |
| Mite SNA VI | as above | `SummonNaturesAllyVId4plus1` `7aefdbd7…` (dire boar) |
| Mite SNA VII | as above | `b81bb947…`, which is the Manticore option itself |
| Mite SNA VIII | as above | `86f42875…` (dire tiger) |
| Mite SNA IX | as above | `9bd8cb61…` (Mastodon) |

**Lower rows, all preserved.** The 15 existing wrapper rows for these creatures (SM: Redcap V/VI/VII, Axiomite and Soul
Eater VI/VII/VIII; SNA: Mite I/II/III, Manticore V/VI/VII) are registered, declared and admitted. Each was observed
once under its own parent in the retained parent census of **published v0.0.191**:
- session `20261116T181244Z-owner-0.33.0-summoning-publication-boundary`;
- package `c09ca625…`;
- fixture `tests/fixtures/summoning-publication/races-unleashed-parent-census.json`.

That is a menu observation of v0.0.191, not of current bytes. The S5 T1 census covers generated placements, not
wrappers, so no wrapper graph is credited. No exact per-placement wrapper cast receipt is mapped into the ledger. Those
rows' 52 alternate adapters stay as registered:
- Monster Tactician, Summoner, Master Summoner and Occultist for the Summon Monster rows;
- Naturalist and Feral Hunter for the Summon Nature's Ally rows;
- Preservationist only at the Summon Nature's Ally levels its Bottled Ally covers (I, II, IV, V, VII and IX).

**Reporter output** (`python tools/expanded_summoning_ideal_roster.py --cells redcap,axiomite,soul-eater,manticore,mite`,
excerpt: the twelve cells and each creature's highest existing 1d4+1 row):

| Creature | Family | Parent | Quantity | Original ideal | Scope | Exact identity | Registered | Declared | Admitted | Native graph | Native menu | Native cast |
|---|---|---|---|---|---|---|---|---|---|---|---|---|
| redcap | Summon Monster | VII | 1d4+1 | yes | in scope | `KMG.Summoning.NativeOption.SM.Tier7.Redcap.OneD4PlusOne` | yes | yes | yes | none | PASS:20261116T181244Z-owner-0.33.0-summoning-publication-boundary@c09ca6255fee | none |
| redcap | Summon Monster | VIII | 1d4+1 | yes | pending owner decision | none declared | no | no | no | none | none | none |
| redcap | Summon Monster | IX | 1d4+1 | yes | pending owner decision | none declared | no | no | no | none | none | none |
| axiomite | Summon Monster | VIII | 1d4+1 | yes | in scope | `KMG.Summoning.NativeOption.SM.Tier8.Axiomite.OneD4PlusOne` | yes | yes | yes | none | PASS:20261116T181244Z-owner-0.33.0-summoning-publication-boundary@c09ca6255fee | none |
| axiomite | Summon Monster | IX | 1d4+1 | yes | pending owner decision | none declared | no | no | no | none | none | none |
| soul-eater | Summon Monster | VIII | 1d4+1 | yes | in scope | `KMG.Summoning.NativeOption.SM.Tier8.SoulEater.OneD4PlusOne` | yes | yes | yes | none | PASS:20261116T181244Z-owner-0.33.0-summoning-publication-boundary@c09ca6255fee | none |
| soul-eater | Summon Monster | IX | 1d4+1 | yes | pending owner decision | none declared | no | no | no | none | none | none |
| manticore | Summon Nature's Ally | VII | 1d4+1 | yes | in scope | `KMG.Summoning.NativeOption.SNA.Tier7.Manticore.OneD4PlusOne` | yes | yes | yes | none | PASS:20261116T181244Z-owner-0.33.0-summoning-publication-boundary@c09ca6255fee | none |
| manticore | Summon Nature's Ally | VIII | 1d4+1 | yes | pending owner decision | none declared | no | no | no | none | none | none |
| manticore | Summon Nature's Ally | IX | 1d4+1 | yes | pending owner decision | none declared | no | no | no | none | none | none |
| mite | Summon Nature's Ally | III | 1d4+1 | yes | in scope | `KMG.Summoning.NativeOption.SNA.Tier3.Mite.OneD4PlusOne` | yes | yes | yes | none | PASS:20261116T181244Z-owner-0.33.0-summoning-publication-boundary@c09ca6255fee | none |
| mite | Summon Nature's Ally | IV | 1d4+1 | yes | pending owner decision | none declared | no | no | no | none | none | none |
| mite | Summon Nature's Ally | V | 1d4+1 | yes | pending owner decision | none declared | no | no | no | none | none | none |
| mite | Summon Nature's Ally | VI | 1d4+1 | yes | pending owner decision | none declared | no | no | no | none | none | none |
| mite | Summon Nature's Ally | VII | 1d4+1 | yes | pending owner decision | none declared | no | no | no | none | none | none |
| mite | Summon Nature's Ally | VIII | 1d4+1 | yes | pending owner decision | none declared | no | no | no | none | none | none |
| mite | Summon Nature's Ally | IX | 1d4+1 | yes | pending owner decision | none declared | no | no | no | none | none | none |

### Bounded reporter correction for these cells

**Defect.** For an undeclared cell, `--cells` filled the identity column from the project-creature convention, so a
missing wrapper cell would show `KMG.Summoning.Ability.SM.Tier8.Redcap.OneD4PlusOne`. That is not a wrapper identity,
and it was inferred.

**Fix:**
- *Identity shown:* a declared identity; else an active ledger identity of the exact placement (reported as registered
  but undeclared); else, for a project creature only, its deterministic generated identity (the accepted Brachiosaurus
  behaviour). An undeclared wrapper cell with no ledger identity shows "none declared" and probes no guessed symbol.
- *Pending set:* the twelve POL-05 cells. Their recorded POL-05 disposition keeps precedence, so the generated document
  is unchanged.
- *Evidence ledger:* now holds the v0.0.191 menu rows for these five creatures' 15 wrapper identities, each under its own
  session and package.

**Controls:**
- each of the twelve cells is undeclared, unregistered and pending, and never shows a project-convention identity;
- the existing rows keep their exact wrapper identity and their v0.0.191 menu row;
- an observation of another tier or quantity never fills a cell;
- a ledger identity without a declaration is reported as registered only;
- the cells keep their POL-05 disposition and are not approved.

## Alternative A: keep the native base-plus-two ladder

**Disposition:** all twelve cells approved out of scope, each recorded individually with its rationale and provenance.
Every existing wrapper, lower row, foreign row and adapter is unchanged.

**What would change:**
- `APPROVED_OUT_OF_SCOPE` gains the twelve cells, and they leave `SCOPE_PENDING` and the POL-05 recorded dispositions.
- Regenerate the ideal-roster document; update the coverage record, `KNOWN-ISSUES.md` and the status register.
- No source, identity, compiled reference model or publication change. The frozen ideal and its 1,232 total stay; the
  compiled coverage model is unchanged.

**Derived effects:**
- Reporter: 1,208 admitted + 9 withheld + 0 missing + 15 approved out of scope = 1,232.
- Every live menu unchanged; the largest is still Summon Monster IX at 116.
- Five creatures move from "partial" to "published within the approved scope".

**Behaviour:** unchanged (native template, quantity, duration and debit as shipped).

**Validation:** reporter tests, freshness, `--validate-scope` and records checks. No build, no native run.

## Alternative B: extend the larger-group choices through IX

**Disposition:** all twelve cells published through the existing retained-wrapper publication path.

**New permanent identities: 49.**
- 12 canonical wrapper rows: `NativeOption.SM.Tier8.Redcap.OneD4PlusOne`, `.SM.Tier9.Redcap…`, `.SM.Tier9.Axiomite…`,
  `.SM.Tier9.SoulEater…`, `.SNA.Tier8.Manticore…`, `.SNA.Tier9.Manticore…` and `.SNA.Tier4–9.Mite.OneD4PlusOne`.
- 37 adapters:
  - Monster Tactician, Summoner, Master Summoner and Occultist for the four Summon Monster cells (16);
  - Naturalist and Feral Hunter for the eight Summon Nature's Ally cells (16);
  - Preservationist for Manticore IX and Mite IV, V, VII and IX (5).
- All append-only; permanent once released.

**Source chain: an owner sub-decision.** No native option exists for any of these cells, so each needs a source:
- **B1, re-host the creature's own native option:** clone the same creature's highest existing 1d4+1 source under the
  higher parent.
  - Sources: Redcap from `6e805a9e…`; Axiomite and Soul Eater from `12815be3…`; Manticore from `b81bb947…`; Mite from
    `bb1bac85…`.
  - Keeps each creature's already-shipped native spawn branch and after-spawn behaviour exactly.
  - The cloned components come from a lower-tier spell; spell level and debit still follow the new parent.
  - The catalog would reuse source identities across tiers, so `SummonNativeExpansionCatalog.Validate` (29 rows, 24
    sources) and the publisher's per-tier `Replaces` suppression check must be reviewed.
- **B2, carve from the cell's own-tier native 1d4+1 option:** the Frost Giant precedent, through `ReplacesSpawnUnit`.
  - Fits all eight Summon Nature's Ally cells and the three Summon Monster IX cells, whose donors are single direct
    spawns.
  - The new creature inherits that donor's after-spawn behaviour, as generated placements do.
  - Redcap SM VIII's own-tier option is two-branch (Axiomite/Soul Eater), so it needs a builder extension or a B1
    exception.
  - Mite SNA VII would be carved from the Manticore option itself.

**Template:** no new celestial or fiendish template.
- B1 keeps each creature's native after-spawn actions.
- B2 inherits the donor's, which must be read before implementation; any native template application in a donor would
  need the S5 T1 policy's treatment.
- The cleric alignment gate stays on Summon Monster rows.

**Quantity, duration, debit:** the source option's own 1d4+1 expression (with the Superior Summoning rank bonus) and its
native duration (one round per caster level); one source of the new parent. The exact expressions of the candidate
sources are assumed from the shipped wrappers and generated placements, not re-read for this packet.

**Derived effects:**
- Reporter: 1,220 admitted + 9 withheld + 0 missing + 3 approved out of scope = 1,232.
- Menus: Summon Monster VIII 111→112 and IX 116→119 (the new largest single menu); Summon Nature's Ally IV, V, VI and VII
  +1 each, VIII and IX +2 each.
- Catalog and identity counts move (wrapper count 29→41; the native-expanded identity count and the adapter counts), and
  the next release validator must account for them beside the DEF-05 obligation.

**What would change:** source (catalog rows, the builder for any two-branch carve), identities, tooling expectations,
records and compiled reference counts. The ideal catalog would not change.

**Validation for B** (one candidate):
- focused domain tests for the catalog, builder and publication;
- one native session covering:
  - the parent membership of every new row;
  - a representative cast for each distinct wrapper and quantity path: Summon Monster re-host or two-branch (Redcap
    VIII), Summon Monster single-spawn carve (IX), and Summon Nature's Ally carve or re-host (Mite and Manticore);
  - controlled-dice quantity, exact duration and one debit;
  - one alternate-family cast per family type (for example Master Summoner IX and Naturalist or Preservationist Summon
    Nature's Ally);
  - the existing lower wrapper rows unchanged.

## Recommendation

**Alternative A**, unless the owner deliberately chooses the content expansion.
- Every shipped wrapper is an exact Owlcat option with a native source. The twelve cells are exactly where Owlcat
  authored none.
- Publishing them needs a source that does not exist: either a lower-tier spell re-hosted under a higher parent (B1) or
  a donor carve, with a builder extension for Redcap VIII (B2). Neither is how a wrapper is built today. It also needs 49
  permanent identities and a new candidate with native qualification.
- The affected menus already offer many larger-group choices from generated placements. At the Sprint 41 measurement
  (released `b2be3ebe`), Summon Monster IX listed 100 1d4+1 choices and Summon Nature's Ally IX listed 98. A keeps the
  published surface, the largest menu (116) and every identity unchanged.
- Like POL-04, the original ideal is rules-supported, so A is a deliberate product-scope choice, recorded cell by cell,
  not a claim that those placements are invalid.

## Owner decisions needed

1. A or B. One decision may apply to all twelve, but each cell is recorded separately either way.
2. If B: B1 (re-host the creature's own option), B2 (carve from the own-tier option), or B2 with a B1 or builder
   exception for Redcap Summon Monster VIII.
3. If B2: whether a donor's native after-spawn behaviour (including any template application) is acceptable on the
   carved creature, or must be cleaned as for generated placements.

## Unchanged

- POL-04's three approved exclusions.
- The nine Stirge withholdings.
- S5 T2's acceptance and both aggregate-FAIL sessions.
- The unclassified console-learning issue.
- The release-validator obligations, including DEF-05.

## Focused checks

| Check | Result | Time |
|---|---|---|
| `python -B tools/test_expanded_summoning_ideal_roster.py` | 38 of 38 (5 new) | 1 s |
| `python -B tools/expanded_summoning_ideal_roster.py --check` | documents current (freshness only) | under 1 s |
| `python -B tools/expanded_summoning_ideal_roster.py --validate-scope` | valid, 3 exclusions | under 1 s |
| `git diff --check` | clean | — |
