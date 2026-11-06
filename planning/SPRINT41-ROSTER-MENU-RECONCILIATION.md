# Sprint 41 audit — roster and menu reconciliation at placement level

Audit source `710ac76b7e4bedbc2a0af4d1a4a38758c82fbbca` (the PR #98 merge).
Read-only derivation from source; no product code, manifest or catalog was
changed to produce it.

This supersedes the earlier identity-level reconciliation in
`runtime-evidence/sprint41-audit-preliminary/ROSTER-MENU-RECONCILIATION.md`,
which reported every charter figure as met. At placement level two of those
figures do not hold. The correction is stated in full below rather than quietly
replaced.

## Method

The reconciliation key is **(family, parent spell level, creature identity,
quantity)** — one menu choice.

Both sides are projected by the *production* rule, `GenerateVariants` in
`ExpandedSummoningCatalog.cs`: for each parent spell level 1-9, a creature with
a family tier `t <= parent` is offered as `One` when `t == parent`, `1d3` when
`t == parent - 1`, and `1d4+1` otherwise. The ideal side applies it to the
frozen `ExpandedSummoningIdealRosterCatalog.cs` tiers; the project-generated
side applies the same rule to the shipped `ExpandedSummoningCatalog.cs` tiers.
Nothing is reimplemented differently for the two sides, so a difference can only
come from a tier difference.

Retained native wrappers are **not** projected. Each row in
`SummonNativeExpansionCatalog.cs` is one authored `(parent, creature, quantity)`
choice, so they are counted row by row. This matters: a first pass of this
reconciliation collapsed wrapper rows to one tier per creature and undercounted
them 12 against 29. Wrapper ladders are *truncated* relative to generated
ladders, which is exactly the kind of gap placement-level reconciliation exists
to find, so collapsing them would have hidden finding A-7 below.

## The arithmetic closes exactly

| Quantity | Count |
|---|---|
| Ideal placements (charter) | **1,232** |
| Project-generated placements | 1,188 |
| Retained native wrapper choices | 29 |
| **Total visible menu choices** | **1,217** |
| Ideal placements with no menu choice at all | **15** |

`1,188 + 29 + 15 = 1,232`, exactly. There are also **zero** placements offered
that the manifest does not place, and **zero** placements offered twice by both
a generated ladder and a wrapper. The two surfaces are disjoint and contained.

Per family:

| | Summon Monster | Nature's Ally |
|---|---|---|
| Ideal creatures | 120 | 110 |
| Ideal placements | 624 | 608 |
| Project-generated creatures | 112 | 105 |
| Project-generated placements | 600 | 588 |
| Wrapper choices | 17 | 12 |
| Offered placements | 617 | 600 |
| **Unoffered ideal placements** | **7** | **8** |

The generated per-family placement counts match the hard invariants the product
asserts about itself in `ExpandedSummoningCatalog.Validate` —
`ValidateFamily(Monster, 112, 600)` and `ValidateFamily(NaturesAlly, 105, 588)` —
so the actual side of this reconciliation agrees with the product's own
self-check.

## Where the earlier reconciliation was wrong

The previous pass concluded that the whole 1,232-versus-1,217 difference was
structural, attributable to the eleven wrapper-covered creatures surfacing as
wrapper choices instead of full ladders. That is true of **12** of the 15
placements. The remaining **3** are an isolated gap in a project-owned
creature that has nothing to do with wrappers (A-5).

It also reported **Summon Monster 120/120 Published — met**. That figure comes
from a tool that decides family publication from identity presence, not from a
real menu choice (A-6). Measured from actual choices the figure is **119/120**.

| Charter figure | Target | Identity-level (old) | Placement-level (correct) |
|---|---|---|---|
| Summon Monster entries Published | 120 | 120 — "met" | **119**, one with no surface |
| Nature's Ally entries Published | 110 | 110 — met | 110 — met |
| Unique creatures published somewhere | 145 | 145 — met | 145 — met |
| Entries still Planned | 0 | 0 — met | 0 — met |
| Entries Registered but suppressed | 0 | 0 — met | 0 — met |
| Creatures neither owned nor wrapped | 0 | 0 — met | 0 — met |
| Visible quantity placements | 1,232 | not assessed | **1,217**, 15 unoffered |

The four "met" rows are confirmed at placement level, not merely carried over.

## Every unoffered ideal placement, attributed

### A-5 — Brachiosaurus has no Summon Monster surface (3 placements)

| | |
|---|---|
| Ideal manifest | `R("brachiosaurus","Brachiosaurus",7,7,25,…)` — Summon Monster VII |
| Shipped catalog | `C("brachiosaurus","Brachiosaurus",null,false,7,"Mastodon")` — no Monster tier |
| Unoffered | SM VII `One`, SM VIII `1d3`, SM IX `1d4+1` |
| Other surfaces | none: no Summon Monster wrapper row exists for it, and the alternate-summoning adapters cannot supply one |

On that last point: `AlternateSummoningSourceCatalog` is keyed by alternate
*casting source* — Monster Tactician, Summoner, Master Summoner, Occultist and
so on — not by creature. An adapter re-projects whatever its family roster
already contains, so a creature contributing no Summon Monster placements cannot
appear in Summon Monster through any adapter. That is the same thing
`planning/ALTERNATE-SUMMONING-STATE.md` records when it says Brachiosaurus
"gains SNA adapter identities only".

It is the only creature the manifest places in a family where no choice exists
in that family.

The implementation side is deliberate and recorded in two places — the Sprint 25
comment in `ExpandedSummoningCatalog.Validate` ("The Monster surface is
deliberately unchanged at 95/543") and
`planning/ALTERNATE-SUMMONING-STATE.md` ("Brachiosaurus, SNA-only and withheld,
gains SNA adapter identities only"). So this is **not** a silent regression. It
is a standing inconsistency: the frozen manifest and the shipped surface
disagree, and they cannot both be right.

The sibling rows settle that it is isolated rather than a family-wide rule:

| Creature | Ideal SM / SNA | Shipped SM / SNA | Agree? |
|---|---|---|---|
| Ankylosaurus | 5 / 5 | 5 / 5 | yes |
| Deinonychus | 4 / 4 | 4 / 4 | yes |
| Triceratops | 6 / 6 | 6 / 6 | yes |
| Elephant | 6 / 6 | 6 / 6 | yes |
| Tyrannosaurus | 7 / 7 | 7 / 7 | yes |
| Stegosaurus | — / 6 | — / 6 | yes, both omit Summon Monster |
| **Brachiosaurus** | **7** / 7 | **—** / 7 | **no** |

Tyrannosaurus shows a tier-7 dinosaur in both families is intended and shipped;
Stegosaurus shows a Nature's-Ally-only dinosaur is expressed by setting the
manifest tier to null. Brachiosaurus matches neither pattern.

**Owner disposition required, and this audit does not choose it:** either ship
the Summon Monster side (the Tyrannosaurus pattern) or set the manifest Monster
tier to null (the Stegosaurus pattern). The charter's 1,232 total and its 120/120
Summon Monster figure both move depending on which way it goes.

### A-7 — wrapper ladders stop at the highest authored native row (12 placements)

| Family | Creature | Ideal tier | Unoffered |
|---|---|---|---|
| SM | Redcap | 5 | VIII `1d4+1`, IX `1d4+1` |
| SM | Axiomite | 6 | IX `1d4+1` |
| SM | Soul Eater | 6 | IX `1d4+1` |
| SNA | Manticore | 5 | VIII `1d4+1`, IX `1d4+1` |
| SNA | Mite | 1 | IV-IX `1d4+1` (six) |

Every one is a `1d4+1` choice at a parent level above *tier + 2*. A wrapper
authors at most the three native rows — `One`, `1d3`, `1d4+1` at tier, tier+1
and tier+2 — whereas a generated ladder continues to level 9. The lower a
wrapper creature's tier, the more of its ladder is missing, which is why Mite at
tier 1 loses six placements and Thanadaemon at tier 9 loses none.

The other seven wrapper family-slots are **complete**, because their ideal
ladders end at or before tier+2: Bogeyman (SM 7), Movanic Deva (SM 8), Frost
Giant (SM 8 and SNA 7), Thanadaemon (SM 9), Nereid (SNA 8), Hamadryad (SNA 9).

This is a structural consequence of the recorded decision to reuse native
wrappers rather than regenerate those eleven creatures. Recorded as a
charter-figure discrepancy, not as a code defect. **Owner disposition:** author
the missing wrapper rows, or restate the 1,232 figure as 1,217 met by native
reuse with 15 placements consciously unoffered.

## The eleven wrapper-covered creatures, validated at their required placements

134 project-owned identities + 11 wrapper-covered identities = 145, and no
creature is unrepresented. All eleven are present in the family the charter
places them in, at the charter's tier; the only shortfall is the upper-ladder
truncation above.

| Key | Name | SM tier | SNA tier | Wrapper rows | Ladder complete? |
|---|---|---|---|---|---|
| axiomite | Axiomite | 6 | — | 6/7/8 | no, IX missing |
| bogeyman | Bogeyman | 7 | — | 7/8/9 | yes |
| frost-giant | Frost Giant | 8 | 7 | SM 8/9, SNA 7/8/9 | yes, both |
| hamadryad | Hamadryad | — | 9 | 9 | yes |
| manticore | Manticore | — | 5 | 5/6/7 | no, VIII-IX missing |
| mite | Mite | — | 1 | 1/2/3 | no, IV-IX missing |
| movanic-deva | Movanic Deva | 8 | — | 8/9 | yes |
| nereid | Nereid | — | 8 | 8/9 | yes |
| redcap | Redcap | 5 | — | 5/6/7 | no, VIII-IX missing |
| soul-eater | Soul Eater | 6 | — | 6/7/8 | no, IX missing |
| thanadaemon | Thanadaemon | 9 | — | 9 | yes |

Subtracting 134 from 145 and calling the difference missing content would have
been wrong; so would treating the wrapper count of 29 as eleven complete
ladders.

## Related finding carried into the repair queue

**A-6 — the roster tool overstates family coverage.**
`coverage()` in `tools/expanded_summoning_ideal_roster.py` sets a family state of
`Published` whenever the creature key appears in the shipped catalog, without
checking that the shipped row carries a tier for that family. Consequences: the
regenerated `planning/EXPANDED-SUMMONING-IDEAL-ROSTER.md` lists Brachiosaurus
Summon Monster as `Published`, indistinguishable from Tyrannosaurus; and the
headline 120/120 is identity-level. It is why A-5 survived until a
placement-level pass ran, and it is why the earlier reconciliation reported a
clean sheet.

Not repaired here, and deliberately so: the correct post-repair number depends on
the A-5 disposition, so changing the tool now would bake in one of the two
answers.

## Scope of this document

Covered: the roster and menu surface, at placement level, derived from source.

Not covered here: gameplay, AI, lifecycle, sanitization and persistence
behaviour; compatibility profiles; visual acceptance; performance. Cleric
alignment restrictions and alphabetical summon-list ordering remain deferred
prerelease work, recorded and not implemented. Variant elementals remain
excluded by owner decision; the classic Air, Earth, Fire and Water elementals
are in scope and present.
