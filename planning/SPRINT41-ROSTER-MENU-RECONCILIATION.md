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

## Per-menu scale, and the real scalability gate

The charter is explicit that 1,232 "is an aggregate across eighteen parent
spells, never one menu", and that "the scalability gate is the largest single
parent spell". So the performance-relevant figure is not the total but the
largest single menu. Measured on the shipped surface:

| Family | Parent | Ideal | Generated | Wrapper | Offered | Shortfall |
|---|---|---|---|---|---|---|
| Summon Monster | I | 7 | 7 | 0 | **7** | — |
| Summon Monster | II | 20 | 20 | 0 | **20** | — |
| Summon Monster | III | 33 | 33 | 0 | **33** | — |
| Summon Monster | IV | 61 | 61 | 0 | **61** | — |
| Summon Monster | V | 75 | 74 | 1 | **75** | — |
| Summon Monster | VI | 91 | 88 | 3 | **91** | — |
| Summon Monster | VII | 104 | 99 | 4 | **103** | 1 |
| Summon Monster | VIII | 113 | 106 | 5 | **111** | 2 |
| Summon Monster | IX | 120 | 112 | 4 | **116** | 4 |
| Nature's Ally | I | 10 | 9 | 1 | **10** | — |
| Nature's Ally | II | 21 | 20 | 1 | **21** | — |
| Nature's Ally | III | 33 | 32 | 1 | **33** | — |
| Nature's Ally | IV | 64 | 63 | 0 | **63** | 1 |
| Nature's Ally | V | 75 | 73 | 1 | **74** | 1 |
| Nature's Ally | VI | 88 | 86 | 1 | **87** | 1 |
| Nature's Ally | VII | 100 | 97 | 2 | **99** | 1 |
| Nature's Ally | VIII | 107 | 103 | 2 | **105** | 2 |
| Nature's Ally | IX | 110 | 105 | 3 | **108** | 2 |

**The scalability gate is Summon Monster IX at 116 offered choices in one
menu**, against an ideal of 120. Nature's Ally IX is second at 108 of 110.

These eighteen rows sum to **1,217**, independently reproducing the visible
total above from a different direction, and the shortfall column sums to the
same **15**. Every shortfall falls at parent VII and above except four in
Nature's Ally IV-VII, which is the Mite and Manticore truncation reaching down
the ladder.

Performance has **not** been measured here; this establishes the figure that a
performance run has to be sized against.

## Confirmed against the live menus, not only against source

Everything above is derived from reading catalogs. It has now been checked
against the **running game**: the `expanded-summoning-menu-counts` assertion in
`observe-expanded-summoning-inventory` reports, per parent spell, the live menu
size and its One / 1d3 / 1d4+1 breakdown.

Measured on the released artifact `b2be3ebe`, whose roster catalogs are
byte-identical to the audit source:

| Menu | native before | live | source-derived | One | 1d3 | 1d4+1 | foreign |
|---|---|---|---|---|---|---|---|
| SM I | 1 | 7 | 7 | 7 | 0 | 0 | 0 |
| SM II | 2 | 20 | 20 | 13 | 7 | 0 | 0 |
| SM III | 3 | 33 | 33 | 13 | 13 | 7 | 0 |
| SM IV | 3 | 61 | 61 | 28 | 13 | 20 | 0 |
| SM V | 3 | 75 | 75 | 14 | 28 | 33 | 0 |
| SM VI | 3 | 91 | 91 | 16 | 14 | 61 | 0 |
| SM VII | 3 | 103 | 103 | 12 | 16 | 75 | 0 |
| SM VIII | 3 | 111 | 111 | 9 | 12 | 90 | 0 |
| **SM IX** | 3 | **116** | **116** | 7 | 9 | 100 | 0 |
| SNA I | 1 | 10 | 10 | 10 | 0 | 0 | 0 |
| SNA II | 2 | 21 | 21 | 11 | 10 | 0 | 0 |
| SNA III | 3 | 33 | 33 | 12 | 11 | 10 | 0 |
| SNA IV | 3 | 63 | 63 | 31 | 12 | 20 | 0 |
| SNA V | 3 | 74 | 74 | 11 | 31 | 32 | 0 |
| SNA VI | 3 | 87 | 87 | 13 | 11 | 63 | 0 |
| SNA VII | 3 | 99 | 99 | 12 | 13 | 74 | 0 |
| SNA VIII | 3 | 105 | 105 | 7 | 12 | 86 | 0 |
| SNA IX | 3 | 108 | 108 | 3 | 7 | 98 | 0 |
| **Total** | **48** | **1,217** | **1,217** | | | | **0** |

**Every menu matches exactly**, the breakdown sums to each menu's size, and
`foreign` is 0 everywhere. So the 1,217 total, the 15-placement shortfall and
the Summon Monster IX scalability gate at 116 are **observations from the
running game**, not inferences from reading catalogs — which is what turns A-5
and A-7 from source analysis into measured findings. The per-menu shortfalls
line up one for one: SM VII 103 against an ideal 104, SM VIII 111 against 113,
SM IX 116 against 120, SNA IV 63 against 64, and so on.

Three supporting assertions passed in the same run:
`expanded-summoning-menu-reconciliation` with `kmgMissingOrDuplicate=0`,
`uniqueMissing=0` and `registeredMissing=0`;
`expanded-summoning-menu-order-and-icons` with `misordered=0`,
`missingIcons=0`, `badQuantityNames=0` and `categoryDistinct=True`; and
`expanded-summoning-menu-contact-sheets` with `sheets=9`.

For scale, the native pre-KMG surface is **48** choices across the same 18
menus. Performance was **not** measured; this establishes that a performance run
has to be sized against 1,217 total choices with a worst single menu of 116.

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

A related point, stated carefully because the tool does **not** deserve blame for
it. The generated document opens with a "Charter reconciliation" table whose
second column is headed *Derived here*, and it closes with "Derived
independently from the manifest; the delta against every charter figure is
zero." Both are literally true: that column is re-derived from the frozen
manifest, so it is a manifest self-consistency check and by construction cannot
show a shipped-surface gap. It is not a false claim about the live menu.

The one row worth relabelling is `Core visible quantity placements | 1,232 |
1,232`, because the word *visible* invites exactly the reading the table does not
support — the visible surface is 1,217. The defect to repair is `coverage()`,
which really does read the shipped catalogs and really does mislabel
Brachiosaurus; the reconciliation table only needs its row renamed to say
*ideal* rather than *visible*.

## Scope of this document

Covered: the roster and menu surface, at placement level, derived from source.

Not covered here: gameplay, AI, lifecycle, sanitization and persistence
behaviour; compatibility profiles; visual acceptance; performance. Cleric
alignment restrictions and alphabetical summon-list ordering remain deferred
prerelease work, recorded and not implemented. Variant elementals remain
excluded by owner decision; the classic Air, Earth, Fire and Water elementals
are in scope and present.
