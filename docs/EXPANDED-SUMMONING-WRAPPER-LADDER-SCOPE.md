# Native-wrapper ladder scope (POL-05 / S5 T4): decision record

Status: **Alternative A selected by the owner** at the S5 T4 review of the decision packet (head `a3a55dac69`,
2026-11-19), and **implemented in the reporter and records only**. S5 T4 is **accepted and closed for its reviewed Alternative A tooling and records development scope (principal review, 2026-11-19, at `acd50a042a`)**.
Acceptance is not merge, release or native gameplay qualification.

Closeout (S5 T5 branch `codex/pol06-cotw-conversion-semantics`): the generated report now qualifies its zero as "0
in-scope placements have no declared choice" (the fifteen exclusions also lack declared choices); the accounting is
unchanged.

**The decision.** These five creatures keep their existing base-plus-two ladders. Exactly twelve 1d4+1 cells are
approved out of scope, each recorded separately with this decision's rationale and provenance. This narrows the
original product promise; it does not fulfil those placements and is not a gameplay repair.

Branch `codex/pol05-wrapper-ladder-scope`, draft PR #189, stacked on PR #188 (accepted S5 T3 head `ada0ff2650`). Not
merged, not released.

**What changed on this branch:**
- the S5 T3 acceptance records;
- a bounded reporter correction for these exact cells, with tests (at `a3a55dac69`; below);
- the native-evidence ledger;
- this record, first as the decision packet and now as the decided outcome;
- the Alternative A scope disposition in the reporter, its tests and the generated documents.

Compiled product source, blueprints, assets, C# tests and the runtime publication path have not changed. No package,
recompilation, native session or installation. The frozen ideal manifest, its 1,232 reference total and the compiled
coverage model are unchanged.

## The question

The original ideal roster (charter) gives every creature a full ladder: single at its tier, 1d3 at the next and 1d4+1 at
every tier above that, through IX. These five creatures ship as **retained native wrappers**, which are KMG-owned,
creature-named clones of Owlcat's own summon options. Owlcat authored each of them only on a base-plus-two ladder, so
no native option exists for twelve exact creature-and-higher-tier 1d4+1 combinations:

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
rules-supported. The decision is a product-scope choice, not a rules correction and not a claim that the placements are
invalid.

## Recorded disposition (Alternative A)

Each of the twelve cells is an independent entry in the reporter's approved exclusions (`APPROVED_OUT_OF_SCOPE`), with
this text:

> POL-05 / S5 T4, Alternative A: product scope decision by the owner at the S5 T4 review on 2026-11-19. The retained
> native wrapper keeps Owlcat's base-plus-two ladder for this creature; no native option exists for this exact creature
> and higher tier, and none is invented. This narrows the original ideal (the Core Rulebook allows 1d4+1 creatures from
> a lower-level list, https://legacy.aonprd.com/coreRulebook/spells/summonMonster.html); it is not a gameplay repair and
> not fulfilment of this placement

- **Removed from:** the pending-decision set (now empty) and the recorded POL-05 dispositions (now empty).
- **Approved exclusions:** exactly 15, the three POL-04 Brachiosaurus Summon Monster cells and these twelve.
- **Not restricted:** generated creatures and every other wrapper row. The base-plus-two restriction applies only to
  these twelve cells.
- **Preserved:**
  - the fifteen existing canonical wrapper rows of these creatures, their 52 adapters, their identities and behaviour;
  - every lower-tier and foreign choice;
  - every withdrawn-content disposition, including the nine Stirge withholdings;
  - POL-04's three exclusions.

### Derived accounting (from unchanged source)

| | Before (`a3a55dac69`) | After |
|---|---|---|
| Admitted | 1,208 | 1,208 |
| Withheld | 9 | 9 |
| Missing | 12 | 0 |
| Approved out of scope | 3 | 15 |
| Original ideal placements | 1,232 | 1,232 |

- **Admitted coverage:** approval never raises it (controlled).
- **Original ideal:** still reported as **incomplete**; the report never calls it fully published.
- **Creature classification:** the five creatures move from "Some ideal placements published, some not" to "Every ideal
  placement in the approved scope published; the others approved out of scope, not published", beside Brachiosaurus.
- **Family status:** for example "In scope complete 3/9 (6 approved out of scope)" for Mite Summon Nature's Ally.
  - The previous label was "Partial … (n missing)".
  - The new label applies only when every non-excluded cell of the family is admitted.
- **Menus:** every live menu is unchanged; the largest is still Summon Monster IX at 116.
- **Behaviour:** unchanged; native template, quantity, duration and debit stay as shipped.

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
- **Canonical execution:** the publisher places the row under the parent of its own tier, so the canonical spell
  converts from that parent and spends one source of that parent.
- **Alternate families:** an adapter's duration and resource contract (class resource, Bottled Ally extract and so on)
  come from that family's host, not from the cloned native option.
- **Template:** none added. The wrapper keeps the native option's own after-spawn behaviour. The S5 T1 inherited-template
  policy applies to generated placements, not to these wrappers.
- **Module off:** the original options are restored.
- **Spawn replacement:** the builder has one spawn-replacement mode (`ReplacesSpawnUnit`). It requires a single direct
  spawn and is used today only for the Frost Giant at Summon Nature's Ally VII–IX, carved from the native Mastodon
  options.

## Audit of the twelve cells

Audited at `a3a55dac69` (before the decision). Every cell shared these facts, and all but the disposition still hold:
- **Original ideal:** yes.
- **Disposition at the audit:** `missing`, with the recorded POL-05 disposition, pending the owner decision.
- **Disposition now:** approved out of scope (POL-05 / S5 T4, Alternative A).
- **Exact identity:** none declared.
- **Registered / declared / admitted:** no / no / no, each read separately; no ledger identity exists for any of them.
- **Native graph, menu, cast:** none.
- **Alternate routes:** none.

| Cell | Same creature's highest existing wrapper (identity; native source; branch; unit) | Native 1d4+1 option at the cell's own parent (not selected; B2 donor candidate) |
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

**Reporter output after the decision** (`python tools/expanded_summoning_ideal_roster.py --cells
redcap,axiomite,soul-eater,manticore,mite`, excerpt: the twelve cells and each creature's highest existing 1d4+1 row):

| Creature | Family | Parent | Quantity | Original ideal | Scope | Exact identity | Registered | Declared | Admitted | Native graph | Native menu | Native cast |
|---|---|---|---|---|---|---|---|---|---|---|---|---|
| redcap | Summon Monster | VII | 1d4+1 | yes | in scope | `KMG.Summoning.NativeOption.SM.Tier7.Redcap.OneD4PlusOne` | yes | yes | yes | none | PASS:20261116T181244Z-owner-0.33.0-summoning-publication-boundary@c09ca6255fee | none |
| redcap | Summon Monster | VIII | 1d4+1 | yes | approved out of scope | none declared | no | no | no | none | none | none |
| redcap | Summon Monster | IX | 1d4+1 | yes | approved out of scope | none declared | no | no | no | none | none | none |
| axiomite | Summon Monster | VIII | 1d4+1 | yes | in scope | `KMG.Summoning.NativeOption.SM.Tier8.Axiomite.OneD4PlusOne` | yes | yes | yes | none | PASS:20261116T181244Z-owner-0.33.0-summoning-publication-boundary@c09ca6255fee | none |
| axiomite | Summon Monster | IX | 1d4+1 | yes | approved out of scope | none declared | no | no | no | none | none | none |
| soul-eater | Summon Monster | VIII | 1d4+1 | yes | in scope | `KMG.Summoning.NativeOption.SM.Tier8.SoulEater.OneD4PlusOne` | yes | yes | yes | none | PASS:20261116T181244Z-owner-0.33.0-summoning-publication-boundary@c09ca6255fee | none |
| soul-eater | Summon Monster | IX | 1d4+1 | yes | approved out of scope | none declared | no | no | no | none | none | none |
| manticore | Summon Nature's Ally | VII | 1d4+1 | yes | in scope | `KMG.Summoning.NativeOption.SNA.Tier7.Manticore.OneD4PlusOne` | yes | yes | yes | none | PASS:20261116T181244Z-owner-0.33.0-summoning-publication-boundary@c09ca6255fee | none |
| manticore | Summon Nature's Ally | VIII | 1d4+1 | yes | approved out of scope | none declared | no | no | no | none | none | none |
| manticore | Summon Nature's Ally | IX | 1d4+1 | yes | approved out of scope | none declared | no | no | no | none | none | none |
| mite | Summon Nature's Ally | III | 1d4+1 | yes | in scope | `KMG.Summoning.NativeOption.SNA.Tier3.Mite.OneD4PlusOne` | yes | yes | yes | none | PASS:20261116T181244Z-owner-0.33.0-summoning-publication-boundary@c09ca6255fee | none |
| mite | Summon Nature's Ally | IV | 1d4+1 | yes | approved out of scope | none declared | no | no | no | none | none | none |
| mite | Summon Nature's Ally | V | 1d4+1 | yes | approved out of scope | none declared | no | no | no | none | none | none |
| mite | Summon Nature's Ally | VI | 1d4+1 | yes | approved out of scope | none declared | no | no | no | none | none | none |
| mite | Summon Nature's Ally | VII | 1d4+1 | yes | approved out of scope | none declared | no | no | no | none | none | none |
| mite | Summon Nature's Ally | VIII | 1d4+1 | yes | approved out of scope | none declared | no | no | no | none | none | none |
| mite | Summon Nature's Ally | IX | 1d4+1 | yes | approved out of scope | none declared | no | no | no | none | none | none |

### Bounded reporter correction for these cells (at `a3a55dac69`)

**Defect.** For an undeclared cell, `--cells` filled the identity column from the project-creature convention, so a
missing wrapper cell would show `KMG.Summoning.Ability.SM.Tier8.Redcap.OneD4PlusOne`. That is not a wrapper identity,
and it was inferred.

**Fix:**
- *Identity shown:* a declared identity; else an active ledger identity of the exact placement (reported as registered
  but undeclared); else, for a project creature only, its deterministic generated identity (the accepted Brachiosaurus
  behaviour). An undeclared wrapper cell with no ledger identity shows "none declared" and probes no guessed symbol.
- *Pending set:* held the twelve POL-05 cells until the decision; it is now empty.
- *Evidence ledger:* now holds the v0.0.191 menu rows for these five creatures' 15 wrapper identities, each under its own
  session and package.

### Scope-contract controls after the decision

The reporter suite (`tools/test_expanded_summoning_ideal_roster.py`, 40 tests) controls:
- **Exact set:** exactly the twelve new exclusions plus the three POL-04 ones; no generated creature or other wrapper is
  restricted; the pending set and recorded dispositions are empty.
- **Coverage:** approval does not raise admitted coverage (1,208 with or without it), and the summary still reports the
  original ideal as incomplete.
- **Partial approval:** approving only Mite SNA IV and Redcap SM IX leaves the other ten POL-05 cells missing, and the
  families stay partial.
- **Consistent display:** the summary, generated document (accounting line, approved-exclusion table, creature
  classification, family status) and `--cells` show the same exclusions.
- **Scope conflicts in both families:** a coherently declared excluded wrapper (Redcap SM VIII and Mite SNA IV, with
  catalog rows, the source's wrapper count and active ledger identities moved to match) keeps its actual
  registration, declaration and admission facts.
  - It produces visible **SCOPE CONFLICT** output in the summary, the document and `--cells`.
  - `--validate-scope` fails.
  - The POL-04 Brachiosaurus conflict control still holds.
- **Registered-only identities:** a ledger identity of an excluded cell without a declaration is reported as registered
  only. It stays undeclared and unadmitted and raises no conflict.
- **Exact matching:** an observation of another tier or quantity never fills a cell, and the existing rows keep their
  exact identity and their v0.0.191 menu row (original session and package).
- **Stirge:** the nine Stirge withholdings are unchanged.
- **Freshness:** `--check` stays freshness-only.

## The comparison with Alternative B (not selected; corrected record)

Kept as the record of what A was chosen over. The corrections below come from the owner review of the packet. They do
not implement B, change the builder or the publication policy, or launch any native donor inspection.

- **Native options versus donors.** No native option exists for any of the twelve exact creature-and-higher-tier
  combinations. Donors do exist: each cell's parent has a native 1d4+1 option.
- **B2 and the Frost Giant precedent.** The existing direct-spawn replacement mechanism (`ReplacesSpawnUnit`) already
  publishes the Frost Giant at Summon Nature's Ally VII–IX from the Mastodon options.
  - *Coverage:* under the packet's donor-shape assumptions (not verified natively), it structurally covers the eight
    Summon Nature's Ally cells and the three Summon Monster IX cells, whose own-tier donors are single direct spawns.
  - *Redcap SM VIII:* the distinct case; its own-tier donor `12815be3…` has two branches, which the mechanism refuses.
  - *Plumbing:* the Summon Monster catalog helper (`N(...)` in `SummonNativeExpansionCatalog`) has no
    replacement-flag parameter today; only the Summon Nature's Ally helper `A(...)` does.
- **B1 (re-hosting a creature's own lower-tier option) had unresolved requirements:**
  - how the re-hosted option links to the higher parent;
  - how the alternate-family adapters publish against a source reused across tiers.
  - New menu membership alone would not establish the intended execution context, spell level or debit.
- **Execution contracts.** Canonical execution would follow the cloned native option and the new parent. Alternate-family
  duration and resource contracts come from each family's host.
- **Identity count.** 49 (12 canonical wrapper rows plus 37 adapters) was the proposed identity allocation, not proof
  that those alternate routes would publish correctly.
- **Effects B would have had:**
  - reporter: 1,220 admitted + 9 withheld + 0 missing + 3 approved out of scope = 1,232;
  - menus: Summon Monster VIII 111→112 and IX 116→119; Summon Nature's Ally IV–VII +1 each, VIII and IX +2 each;
  - validation: a new candidate with native qualification across both families and every distinct wrapper and quantity
    path.

## Why A

- Every shipped wrapper is an exact Owlcat option, and these twelve combinations are exactly where Owlcat authored none.
- A keeps the published surface, every identity and the largest menu (116) unchanged and needs no candidate or native
  qualification.
- The affected menus already offer many larger-group choices from generated placements. At the Sprint 41 measurement
  (released `b2be3ebe`), Summon Monster IX listed 100 1d4+1 choices and Summon Nature's Ally IX listed 98.

## Unchanged

- POL-04's three approved exclusions.
- The nine Stirge withholdings.
- S5 T2's acceptance and both aggregate-FAIL sessions.
- The unclassified console-learning issue.
- The release-validator obligations, including DEF-05.
- Earlier checkpoints and executions, under their original identities.

## Focused checks

| Check | Result | Time |
|---|---|---|
| `python -B tools/test_expanded_summoning_ideal_roster.py` | 40 of 40 | 1–2 s |
| `python -B tools/expanded_summoning_ideal_roster.py` (regenerate) | documents rewritten from unchanged source | under 1 s |
| `python -B tools/expanded_summoning_ideal_roster.py --check` | documents current (freshness only) | under 1 s |
| `python -B tools/expanded_summoning_ideal_roster.py --validate-scope` | valid, 15 approved exclusions | under 1 s |
| `git diff --check` | clean | — |
