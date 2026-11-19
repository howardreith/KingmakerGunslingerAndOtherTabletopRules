# Brachiosaurus spell-family scope (POL-04 / S5 T3): decision packet

Status: **owner-decision checkpoint.** No gameplay has been implemented. This packet presents the two alternatives and
recommends one; principal review selects A or B.

Branch `codex/pol04-brachiosaurus-scope`, draft PR #188, stacked on PR #187 (accepted S5 T2 records head
`bbc8174f05`). Not merged, not released.

## The question

The shipped product offers Brachiosaurus through Summon Nature's Ally only, by a recorded Sprint 25 decision. The
ideal roster promises three Summon Monster cells:

| Cell | Parent | Quantity |
|---|---|---|
| SM-1 | Summon Monster VII | single |
| SM-2 | Summon Monster VIII | 1d3 |
| SM-3 | Summon Monster IX | 1d4+1 |

## 1. Current Brachiosaurus catalog entry

- **Creature key:** `brachiosaurus`.
- **Shipped catalog** (`ExpandedSummoningCatalog.cs`): `C("brachiosaurus","Brachiosaurus",null,false,7,"Mastodon")`.
  That is no Summon Monster tier, not Monster-templated, Summon Nature's Ally tier 7, with the Mastodon visual/body
  donor.
- **Template:** untemplated, "Caster alignment; never celestial/fiendish" (`planning/EXPANDED-SUMMONING-ROSTER.md`). The
  v0.0.159 notes say it "is untemplated under caster alignment and is not on the Summon Monster table, so it has no
  celestial or fiendish execution children".
- **Recorded decision:** the `ExpandedSummoningCatalog.Validate` comment ("Sprint 25 adds Brachiosaurus to Nature's Ally
  VII only ... The Monster surface is deliberately unchanged"), `docs/RELEASE-NOTES-0.0.159.md` and
  `docs/SPRINT25-COVERAGE.md` ("No Brach Monster row or template children were added").

**Identities.** The ledger (`blueprints/blueprints.json`) holds 19 active identities, none of them Summon Monster:

| Kind | Symbols |
|---|---|
| Unit | `KMG.Summoning.Unit.Brachiosaurus` `9093aef3…` |
| Canonical Nature's Ally abilities | `SNA.Tier7.Brachiosaurus.One` `5ff406f7…`, `SNA.Tier8.Brachiosaurus.OneD3` `7ab83d22…`, `SNA.Tier9.Brachiosaurus.OneD4PlusOne` `10a645ec…` |
| Natural attacks | `Natural.Brachiosaurus.Tail4d6`, `.TailType`, `.UnitType`, `.Chassis` |
| Specials | `Special.Brachiosaurus.Trample`, `.Overrun`, `.GreaterOverrun` |
| Alternate adapters (Nature's Ally families) | Naturalist Tier 7/8/9, Feral Hunter Tier 7/8/9, Preservationist Tier 7 and 9 (Bottled Ally has no Summon Nature's Ally VIII) |

**Routes.**
- *Canonical:* Summon Nature's Ally VII (single), VIII (1d3) and IX (1d4+1), through the canonical Nature's Ally donor
  carriers `6d8d59aa…`, `256739c1…` and `9bd8cb61…`.
- *Alternate:* the Naturalist, Feral Hunter and Preservationist adapters only. As
  `planning/SPRINT41-ROSTER-MENU-RECONCILIATION.md` records, the alternate catalog is keyed by casting source, so no
  adapter can supply a Summon Monster choice for a creature with no Summon Monster placement.

## 2. Current actual menu census

| Evidence | Brachiosaurus SNA VII / VIII / IX | Brachiosaurus SM VII / VIII / IX |
|---|---|---|
| Registered identity (ledger) | yes, yes, yes | none |
| Declared and admitted (source catalogs) | yes, yes, yes | none |
| Graph built in game | yes, all three canonical and all eight adapters, in the accepted S5 T1 census `20261119T201715Z-owner-0.33.0-inherited-template-census` (disposition `OutsideSummonMonster`) | none |
| Per-placement native menu observation | none on record | none |
| Native spell cast of this exact identity | none on record | none |

The live menu sizes were last measured per parent spell on the released artifact `b2be3ebe` (Sprint 41). Summon Monster
VII, VIII and IX held 103, 111 and 116 choices, against ideals of 104, 113 and 120. Summon Monster VII's single
shortfall is this Brachiosaurus cell. Those counts are aggregates per menu, not per-creature observations, and the
reporter does not credit them to any placement.

Sprint 25 natively qualified the Brachiosaurus *unit*: its profile, the +23 tail, Perception, Power Attack, its
commands and Trample (candidate `a243548b`). A later Cleanup2 creature review passed 36/36
(`20261024T0745057530976Z`). Both observe the creature's behaviour. Neither is recorded as a cast of a specific Summon
Nature's Ally identity, so they are not counted as cast evidence here.

## 3. Current ideal-roster contract

- **Where the promise comes from:** the frozen ideal manifest `ExpandedSummoningIdealRosterCatalog.cs` row
  `R("brachiosaurus","Brachiosaurus",7,7,25,Essential,High,false,"Sauropod dinosaur")`. It encodes the Expanded
  Summoning charter's Appendix A, Sprint 25 ("Giant Bodies - Brachiosaurus and Elephant"), at Summon Monster 7 and
  Nature's Ally 7.
- **No rules citation is recorded:** I did not verify the tabletop summon tables in this session.
- **The manifest is inert:** it registers and publishes nothing ("no publication path reads it"). Its product readers
  are `ExpandedSummoningCoveragePolicy`, which has no gameplay caller, and the runtime-test
  `ExpandedSummoningProjectedMenuFixture` (base-entry targets).

**Reporter output.** `python tools/expanded_summoning_ideal_roster.py --cells brachiosaurus,tyrannosaurus,stegosaurus`,
on this branch:

| Creature | Family | Parent | Quantity | Ideal | Scope | Exact identity | Registered | Declared | Admitted | Native graph | Native menu | Native cast |
|---|---|---|---|---|---|---|---|---|---|---|---|---|
| brachiosaurus | Summon Monster | VII | single | yes | pending owner decision | `KMG.Summoning.Ability.SM.Tier7.Brachiosaurus.One` | no | no | no | none | none | none |
| brachiosaurus | Summon Monster | VIII | 1d3 | yes | pending owner decision | `KMG.Summoning.Ability.SM.Tier8.Brachiosaurus.OneD3` | no | no | no | none | none | none |
| brachiosaurus | Summon Monster | IX | 1d4+1 | yes | pending owner decision | `KMG.Summoning.Ability.SM.Tier9.Brachiosaurus.OneD4PlusOne` | no | no | no | none | none | none |
| brachiosaurus | Summon Nature's Ally | VII | single | yes | in scope | `KMG.Summoning.Ability.SNA.Tier7.Brachiosaurus.One` | yes | yes | yes | PASS:20261119T201715Z-owner-0.33.0-inherited-template-census | none | none |
| brachiosaurus | Summon Nature's Ally | VIII | 1d3 | yes | in scope | `KMG.Summoning.Ability.SNA.Tier8.Brachiosaurus.OneD3` | yes | yes | yes | PASS:20261119T201715Z-owner-0.33.0-inherited-template-census | none | none |
| brachiosaurus | Summon Nature's Ally | IX | 1d4+1 | yes | in scope | `KMG.Summoning.Ability.SNA.Tier9.Brachiosaurus.OneD4PlusOne` | yes | yes | yes | PASS:20261119T201715Z-owner-0.33.0-inherited-template-census | none | none |
| tyrannosaurus | Summon Monster | VII | single | yes | in scope | `KMG.Summoning.Ability.SM.Tier7.Tyrannosaurus.One` | yes | yes | yes | PASS:20261119T201715Z-owner-0.33.0-inherited-template-census | none | none |
| tyrannosaurus | Summon Monster | VIII | 1d3 | yes | in scope | `KMG.Summoning.Ability.SM.Tier8.Tyrannosaurus.OneD3` | yes | yes | yes | PASS:20261119T201715Z-owner-0.33.0-inherited-template-census | none | none |
| tyrannosaurus | Summon Monster | IX | 1d4+1 | yes | in scope | `KMG.Summoning.Ability.SM.Tier9.Tyrannosaurus.OneD4PlusOne` | yes | yes | yes | PASS:20261119T201715Z-owner-0.33.0-inherited-template-census | none | none |
| tyrannosaurus | Summon Nature's Ally | VII | single | yes | in scope | `KMG.Summoning.Ability.SNA.Tier7.Tyrannosaurus.One` | yes | yes | yes | PASS:20261119T201715Z-owner-0.33.0-inherited-template-census | none | none |
| tyrannosaurus | Summon Nature's Ally | VIII | 1d3 | yes | in scope | `KMG.Summoning.Ability.SNA.Tier8.Tyrannosaurus.OneD3` | yes | yes | yes | PASS:20261119T201715Z-owner-0.33.0-inherited-template-census | none | none |
| tyrannosaurus | Summon Nature's Ally | IX | 1d4+1 | yes | in scope | `KMG.Summoning.Ability.SNA.Tier9.Tyrannosaurus.OneD4PlusOne` | yes | yes | yes | PASS:20261119T201715Z-owner-0.33.0-inherited-template-census | none | none |
| stegosaurus | Summon Nature's Ally | VI | single | yes | in scope | `KMG.Summoning.Ability.SNA.Tier6.Stegosaurus.One` | yes | yes | yes | PASS:20261119T201715Z-owner-0.33.0-inherited-template-census | none | none |
| stegosaurus | Summon Nature's Ally | VII | 1d3 | yes | in scope | `KMG.Summoning.Ability.SNA.Tier7.Stegosaurus.OneD3` | yes | yes | yes | PASS:20261119T201715Z-owner-0.33.0-inherited-template-census | none | none |
| stegosaurus | Summon Nature's Ally | VIII | 1d4+1 | yes | in scope | `KMG.Summoning.Ability.SNA.Tier8.Stegosaurus.OneD4PlusOne` | yes | yes | yes | PASS:20261119T201715Z-owner-0.33.0-inherited-template-census | none | none |
| stegosaurus | Summon Nature's Ally | IX | 1d4+1 | yes | in scope | `KMG.Summoning.Ability.SNA.Tier9.Stegosaurus.OneD4PlusOne` | yes | yes | yes | PASS:20261119T201715Z-owner-0.33.0-inherited-template-census | none | none |

The unrelated, correctly published control with native cast evidence is
`python tools/expanded_summoning_ideal_roster.py --cells air-mephit` (excerpt):

| Creature | Family | Parent | Quantity | Ideal | Scope | Exact identity | Registered | Declared | Admitted | Native graph | Native menu | Native cast |
|---|---|---|---|---|---|---|---|---|---|---|---|---|
| air-mephit | Summon Monster | IV | single | yes | in scope | `KMG.Summoning.Ability.SM.Tier4.AirMephit.One` | yes | yes | yes | PASS:20261119T201715Z-owner-0.33.0-inherited-template-census | none | PASS:20261119T201938Z-owner-0.33.0-inherited-template-casts |
| air-mephit | Summon Monster | VI | 1d4+1 | yes | in scope | `KMG.Summoning.Ability.SM.Tier6.AirMephit.OneD4PlusOne` | yes | yes | yes | PASS:20261119T201715Z-owner-0.33.0-inherited-template-census | none | PASS:20261119T201938Z-owner-0.33.0-inherited-template-casts |
| air-mephit | Summon Nature's Ally | IV | single | yes | in scope | `KMG.Summoning.Ability.SNA.Tier4.AirMephit.One` | yes | yes | yes | PASS:20261119T201715Z-owner-0.33.0-inherited-template-census | none | none |

The global source-derived summary (`--check`) is unchanged:

> Source-derived coverage is incomplete: 1,208 of 1,232 ideal placements admitted; 9 withheld; 15 missing; 0 extra;
> 0 duplicate; 0 unexplained.

The 15 missing are these 3 POL-04 cells plus the 12 POL-05 wrapper-ladder cells.

### The bounded reporter repair (tooling and records only)

Before this checkpoint the reporter could not express either alternative per cell:
- Alternative A would still have read as `missing`.
- Registered identity, admission and native evidence were not reported per cell.

`tools/expanded_summoning_ideal_roster.py` gains:
- `APPROVED_OUT_OF_SCOPE`: empty until an owner decision is recorded. An approved cell becomes `out-of-scope`, never
  `covered` and never `missing`. A declared choice for an approved-out-of-scope cell is reported as a scope conflict.
- `SCOPE_PENDING`: the three POL-04 cells.
- `--cells`: one decision row per ideal placement, with independent ideal/scope, exact family-specific ledger
  identity, declared, admitted and native graph/menu/cast columns.

Native evidence is read from `planning/expanded-summoning-native-placement-evidence.json`, extracted read-only from the
accepted S5 T1 census and cast sessions and keyed by exact identity.

With no decision recorded, every existing output and generated document is byte-identical.

**False-pass controls** (`tools/test_expanded_summoning_ideal_roster.py`, nine new):
- each SM cell is reported individually and unfilled;
- a registered Nature's Ally identity never fills a Summon Monster cell;
- declared and admitted without a ledger identity stays unregistered;
- cast evidence is exact to the identity (an SM cast does not count for SNA, and graph evidence is not a cast);
- a failed or misnamed evidence row cannot pass a cell, and an unknown evidence kind is refused;
- approved-out-of-scope moves exactly the approved cells (covered unchanged, missing less by exactly those);
- one approved cell leaves the others missing;
- a declared choice that contradicts the decision is a conflict;
- the current decision is still pending.

No global total is hardcoded in the new tests. The Nature's Ally controls (Brachiosaurus SNA, Stegosaurus), the
Tyrannosaurus both-family control and the unrelated Air Mephit control are all retained.

## 4. Alternative A: keep the shipped Nature's Ally-only scope

**What it is:** a scope decision, not a gameplay repair. Brachiosaurus stays available only through its current Nature's
Ally placements. Nothing is newly fixed or added. All 19 identities and every Nature's Ally behaviour stay unchanged.

**Two ways to record it** (owner's choice):
- **A-records (no product bytes):**
  - Record the decision in `APPROVED_OUT_OF_SCOPE` for the three cells, with its text.
  - Regenerate `planning/EXPANDED-SUMMONING-IDEAL-ROSTER.md`.
  - Update `docs/EXPANDED-SUMMONING-ROSTER-COVERAGE.md`, `KNOWN-ISSUES.md` and the status register.
  - The ideal stays 1,232. The reporter then reads 1,208 admitted, 9 withheld, 12 missing (POL-05 only) and 3 approved
    out of scope.
  - Validation: the reporter tests, `--check` freshness and `git diff --check`. No build.
- **A-manifest (inert compiled change):**
  - Also correct the manifest to the Stegosaurus pattern (Summon Monster tier `null`), restating the charter figures:
    120 to 119 Summon Monster entries and 1,232 to 1,229 ideal placements.
  - The reporter's `EXPECTED`/charter check, the `ideal-roster-*` domain tests and
    `ExpandedSummoningProjectedMenuFixture`'s base-entry target move with it.
  - The manifest is compiled into the assembly, so the DLL bytes change. Gameplay does not, because no publication path
    reads it.
  - Validation: the focused `ideal-roster`/`coverage` domain tests and the reporter tests. No native run, since nothing
    observable changes.

## 5. Alternative B: publish the three Summon Monster cells

**Catalog:** `C("brachiosaurus","Brachiosaurus",7,true,7,"Mastodon")`, published through the normal owned path
(Tyrannosaurus and Triceratops pattern). The existing Nature's Ally routes are unchanged.

**New append-only identities: 21.** The unit, natural, special and Nature's Ally identities are reused.
- 3 canonical abilities: `SM.Tier7.Brachiosaurus.One`, `SM.Tier8.Brachiosaurus.OneD3`,
  `SM.Tier9.Brachiosaurus.OneD4PlusOne`.
- 6 deliberate-template execution children: `.Celestial` and `.Fiendish` for each.
- 12 Summon Monster adapters: Monster Tactician, Summoner, Master Summoner and Occultist, Tier 7/8/9 each.

**Source and converted chain:** the native SummonMonsterVIIBase (`ab167fd8…`), VIII and IX parents convert to the new
child, exactly as the Tyrannosaurus children do. The donor carriers are the ones Tyrannosaurus uses: SM VII
`2920d485…`, SM VIII `ddc1d195…`, SM IX `22b9580c…`.

**Rules of the new placements:**
- **Quantities:** single at VII, 1d3 at VIII, 1d4+1 at IX (the `GenerateVariants` progression).
- **Duration:** native one round per caster level.
- **Spell level:** the caster's own Summon Monster VII/VIII/IX level.
- **Debit:** one source of that parent.

**Template policy.** Monster-templated: celestial or fiendish by caster alignment (`PreserveDeliberateTemplate` in the
S5 T1 per-placement policy), as for the project's other Summon Monster animals.
- This reverses the v0.0.159 statement "has no celestial or fiendish execution children" for the Summon Monster side
  only. Nature's Ally stays untemplated.
- An untemplated Summon Monster variant would be a further, separate decision.

**Counts that move (none hardcoded as passing):**
- `ValidateFamily(Monster)` 112/600 to 113/603, and `RegisteredLogicalPlacementCount` +3.
- Each of the four Summon Monster alternate families 600 to 603.
- The S5 T1 census 5,769 to 5,790 observations (+9 canonical, +12 alternate).
- Live menus: Summon Monster VII 103 to 104 (= ideal), VIII 111 to 112, IX 116 to 117. The worst single menu becomes 117.
- Reporter: missing 15 to 12.
- The development-candidate validator's `appended` 0 to 21. That also needs the ledger test counter, the adapter
  prefix tests and the icon-catalog registry hash repinned (the icon is reused).

**Native cases needed** (one candidate):
- Summon Monster VII single by a good and an evil caster (celestial and fiendish template, smite and exact debit).
- VIII 1d3 and IX 1d4+1 with controlled dice and exact counts.
- One alternate Summon Monster family cast (for example Master Summoner tier VII), with its own debit and duration.
- The census rerun (5,790; 46/2/0 donors).
- Per-menu counts for Summon Monster VII to IX.
- Nature's Ally Brachiosaurus and an unrelated row unchanged.

**Saves and compatibility:**
- Append-only identities are safe for existing saves, and the unit blueprint is unchanged.
- Once released, the 21 identities are permanent; withdrawing them later needs a load-time fixer.
- Alternate-provider (Call of the Wild) adapter counts change, and the next release validator must account for them
  beside the S5 T1 obligation.

## 6. Recommendation

**Alternative A, recorded as A-records**, unless the owner deliberately chooses the content expansion.
- The shipped Nature's Ally-only scope is coherent and usable. All three Nature's Ally placements and all eight
  Nature's Ally adapters are registered, admitted and built in game. The creature itself has been natively qualified.
  The Stegosaurus row already establishes Nature's Ally-only dinosaurs as a project pattern.
- Theme: a natural sauropod belongs with the nature-caster family. The project's Summon Monster animals carry
  celestial or fiendish templates, which the Sprint 25 decision deliberately did not give it.
- CRPG consequences of A: arcane Summon Monster casters (Wizard, Sorcerer and the Summon Monster alternate families)
  do not gain a Gargantuan tank at VII to IX. The largest single menu stays 116 choices. No identity becomes permanent.
- A-records keeps the frozen charter auditable: the promise and its waiver are both on record. A-manifest rewrites the
  promise itself.
- B is the larger, irreversible-once-released path: 21 permanent identities, a template reversal on the Summon Monster
  side, moved census, menu and validator counts, and its own candidate and native qualification.

## Unresolved questions for the owner

1. A or B.
2. If A: A-records (waiver on record, charter figures unchanged) or A-manifest (Stegosaurus-pattern manifest, charter
   figures restated to 119 entries and 1,229 placements).
3. If B: templated Summon Monster placements (recommended for consistency with the other Summon Monster animals) or
   untemplated.
4. Whether a tabletop-rules citation for the charter's Summon Monster VII placement must be checked before deciding. It
   was not verified in this session.

## Scope of this checkpoint

- No gameplay or product source changed, and no package was built.
- The console-learning failure recorded under S5 T2 was not investigated on this branch.

**Focused checks:**

| Check | Result | Time |
|---|---|---|
| `python -B tools/test_expanded_summoning_ideal_roster.py` | 31 of 31 (22 existing, 9 new) | 1 s |
| `python -B tools/expanded_summoning_ideal_roster.py --check` | documents current | under 1 s |
| `Invoke-KmgGate -Filter 'expanded-summoning.ideal-roster;expanded-summoning.coverage'` | 11 of 11 (3,135 registered) | 21 s |
| `git diff --check` | clean | — |
