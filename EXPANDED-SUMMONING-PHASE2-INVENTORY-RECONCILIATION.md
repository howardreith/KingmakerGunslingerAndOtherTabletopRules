# Expanded Summoning publication-inventory reconciliation

## OwnerAcceptedEngineLimitation: PASSIVE_CREATURE_SENSES_UNMODELED

**Accepted by the owner on 2026-10-03.** Kingmaker models none of **Scent**,
**Darkvision** or **Low-light Vision**, and this project omits them wherever a
bounded native audit proves no faithful carrier exists.

### The evidence the acceptance rests on

The audit searched every component on every blueprint the running game had
loaded - the whole library, not a filtered subset - and reported
`senseComponentCensus=Blindsensex74/OverrideVisionRangex23`. Scent has no
carrier of any kind. No enum reachable from `BlueprintUnit`, `UnitEntityData` or
`UnitDescriptor` holds a darkvision value: `visionEnum=<none>`.
`OverrideVisionRange` is a sight radius in metres, carried by 23 blueprints, of
which the sampled six are Vordakai and Horagnamon boss units setting 50 m
against a live unit's 8.5.

### What the label permits

A creature may publish or qualify without these three passive senses. The
omission is recorded here once and referenced from the affected creature rows.
**A creature is never kept hidden for one of these three alone.**

### What it forbids

- No substitution of `AddBlindsight`, `UnitPartBlindsense`, tremorsense or any
  materially different sense. Blindsight is a real and different rule, and this
  project implements it exactly for the Dire Bat; that implementation stays.
- No use of `OverrideVisionRange` as darkvision. It changes general detection
  range in all conditions and would hand a creature an advantage no stat block
  grants it.
- No scent, darkness, stealth-detection or perception subsystem in this phase.
- No claim anywhere that the omitted traits work.
- No use of this label to waive any other sense, combat ability, immunity,
  skill or signature mechanic.
- The limitation is never re-marked BLOCKED.

This is a conservative engine limitation, not a balance adaptation.

### Creatures currently governed by it

Giant Ant (Worker), Giant Ant (Soldier) and Giant Ant (Drone) for Scent and
Darkvision; the Fire Beetle and Giant Stag Beetle for Low-light Vision and
Darkvision respectively; and already-published creatures carrying the same
engine gap, the wolves among them. It applies to later Phase 2 creatures whose
stat blocks list these passive senses.


## Sprint 14 closeout, 2026-10-03: COMPLETE WITH OWNER-ACCEPTED ENGINE LIMITATION

All three Sprint 14 insects are published. The published surface is **952
generated placements plus 29 retained native wrappers, for 981 visible
choices**, derived from source. The Fire Beetle's 18 placements went out on its
own qualification; the Giant Ant Worker's 16 and Soldier's 14 followed once the
owner accepted `PASSIVE_CREATURE_SENSES_UNMODELED`, which was the only thing
holding them - their mechanics had already qualified 174/174 across six guarded
scenarios on candidate `cda8d72c`.

Nothing was implemented to earn that publication. The ruling records an engine
gap rather than closing one: no substitute sense was added, no vision range was
touched, and no record claims the omitted traits work.

What remains withheld is Sprint 15's pair - the Giant Ant Drone's 12 placements
and the Giant Stag Beetle's 6 - registered ahead of their own qualification the
way every sprint before them did. The Drone carries the same unmodelled senses
and is no longer held for them; it waits on its own gates alone.

`HumanReview: NOT_PERFORMED_NONBLOCKING`.

## Sprint 14 publication, 2026-10-03: the Fire Beetle only

The published surface is **922 generated placements plus 29 retained native
wrappers, for 951 visible choices**, derived from source. Sprint 14 registered
48 placements across three insects and publishes 18 of them - the Fire Beetle,
reachable from both parents and splitting nine each way. The 30 placements of
Giant Ant (Worker) and Giant Ant (Soldier) stay withheld.

The ants are not withheld for want of qualification. Their mechanics passed on
the closing candidate: printed Perception +5 exactly with zero class ranks, the
printed trip defence, a sting that is its own weapon type, a grab on the bite
alone carrying exactly +4 to the grapple and nothing to the trip, an injury
poison that a wound delivers and a hit reduced to zero damage does not, vermin
mind-affecting immunity proved by a native mind-affecting buff they refuse and
the caster accepts, and both combat modes driven through the command a player's
click produces. What they cannot have is their printed scent.

Kingmaker has no scent mechanic. The bounded native audit searched the whole
loaded blueprint library, not a filtered subset, and found no component that
could express it; the engine's only sense plumbing is `AddBlindsight` with
`UnitPartBlindsense`, which this project already uses for the Dire Bat and which
is a different rule. Their printed darkvision 60 ft is equally unrepresentable:
no enum reachable from `BlueprintUnit`, `UnitEntityData` or `UnitDescriptor`
carries a darkvision value, and `OverrideVisionRange` is a sight radius in
metres - six Vordakai and Horagnamon boss units set 50 m against a live default
of 8.5 m - so using it for darkvision would hand the ants more than double
their detection range in all conditions, an advantage no stat block grants
them. In an engine with no darkness that is inventing a mechanic rather than
implementing one.

The Fire Beetle's own senses requirement was the opposite case and is met
exactly: its stat block prints **no** darkvision, nothing was inherited from the
Vermin racial class, the project unit type or the Giant Spider donor, and the
live unit reads `darkvision=False` with no carrier. Its printed low-light vision
has no representation either, and no mechanical consequence in an engine with no
light model, so nothing about the creature is softened by its absence.

**This is not recorded as an accepted engine limitation.** No
`OwnerAcceptedEngineLimitation:` label is created here, because only the owner
converts a proven barrier into one. The two castes are held pending a single
ruling, recorded as a blocker with the engine evidence. The same ruling governs
Sprint 15's Giant Ant Drone, which prints scent too; the Giant Stag Beetle does
not and is unaffected.

Opened 2026-10-01 as the first task of the Claude Phase 2 continuation order.
This record supersedes every earlier prose claim that the 0.0.141 published
catalog contains 882 generated choices or 911 visible choices.

## 0. Current source, after the Sprint 12 publication

Sprint 12 qualified later the same day and its four creatures published, so the
current source no longer withholds anything it has registered. Sections 2 to 4
below describe the frozen v0.0.141 build and keep its own numbers; this section
describes the branch as it stands now.

| Quantity | v0.0.141 (frozen) | Current source |
| --- | --- | --- |
| Registered generated placements | 900 | **904** (457 SM + 447 SNA) |
| Suppressed generated placements | 68 | **0** |
| Published generated placements | 832 | **904** (457 SM + 447 SNA) |
| Retained native wrappers | 29 | 29 (17 SM + 12 SNA) |
| Total visible player choices | 861 | **933** (474 SM + 459 SNA) |

The published equation is now **904 + 29 = 933**. Dog, Hyena and Goblin Dog,
which v0.0.141 withdrew, are castable again on their original registered
identities; Dire Rat is published for the first time; and Sprint 13 added the
Shadow Mastiff, whose four Summon Monster placements took the registered
generated total from 900 to 904 and are now published alongside the rest. The
withdrawal described in section 4 is therefore resolved on this branch; it
remains true of the released v0.0.141 artifact, which is immutable.

## 1. Why this record exists

The 0.0.141 release notes, the Phase 2 implementation report, the Phase 2
journal, the fidelity matrix, the changelog, the resume handoff and the state
header all described the published surface as "882 generated choices plus 29
retained native wrappers, for 911 visible choices". The production source at
the released commit does not produce those numbers.

The discrepancy is a stale report, not a source defect. Production code, the
generated roster ledger (`planning/EXPANDED-SUMMONING-ROSTER.md`) and the
domain suite already carry the correct derived values; only hand-written prose
was left behind.

## 2. Derived inventory at the released commit

Source of truth: `src/KingmakerGunslinger/Summoning/ExpandedSummoningCatalog.cs`,
`SummonVisibilityCatalog.cs` and `SummonNativeExpansionCatalog.cs` at
`97f0a966b3219ce0122626a492529b509e1db880` (tag `v0.0.141`, PR #25 head).
Every number below is derived from those files by the catalog's own generation
rule - a creature with a family tier `t` occupies one placement under each
parent spell level `t..9` - and agrees with
`ExpandedSummoningBaselineInventory.Census`.

| Quantity | Value |
| --- | --- |
| Project creature identities | 88 |
| Summon Monster roster entries | 80 |
| Summon Nature's Ally roster entries | 78 |
| Registered generated placements | 900 (453 SM + 447 SNA) |
| Suppressed generated placements | 68 (34 SM + 34 SNA) |
| **Published generated placements** | **832 (419 SM + 413 SNA)** |
| Retained native wrappers | 29 (17 SM + 12 SNA) |
| **Total visible player choices** | **861 (436 SM + 425 SNA)** |

The published equation is therefore **832 + 29 = 861**, not 882 + 29 = 911.

### 2.1 Suppressed placements by creature

| Creature | SM tier | SM placements | SNA tier | SNA placements | Total |
| --- | --- | --- | --- | --- | --- |
| Dire Rat | 1 | 9 | 1 | 9 | 18 |
| Dog | 1 | 9 | 1 | 9 | 18 |
| Goblin Dog | 2 | 8 | 2 | 8 | 16 |
| Hyena | 2 | 8 | 2 | 8 | 16 |
| **Total** | | **34** | | **34** | **68** |

### 2.2 Per-parent census

| Parent | Registered | Suppressed | Published generated | Native wrappers | Visible choices |
| --- | --- | --- | --- | --- | --- |
| SM I | 5 | 2 | 3 | 0 | 3 |
| SM II | 16 | 4 | 12 | 0 | 12 |
| SM III | 25 | 4 | 21 | 0 | 21 |
| SM IV | 48 | 4 | 44 | 0 | 44 |
| SM V | 57 | 4 | 53 | 1 | 54 |
| SM VI | 68 | 4 | 64 | 3 | 67 |
| SM VII | 75 | 4 | 71 | 4 | 75 |
| SM VIII | 79 | 4 | 75 | 5 | 80 |
| SM IX | 80 | 4 | 76 | 4 | 80 |
| **SM total** | **453** | **34** | **419** | **17** | **436** |
| SNA I | 7 | 2 | 5 | 1 | 6 |
| SNA II | 17 | 4 | 13 | 1 | 14 |
| SNA III | 25 | 4 | 21 | 1 | 22 |
| SNA IV | 49 | 4 | 45 | 0 | 45 |
| SNA V | 57 | 4 | 53 | 1 | 54 |
| SNA VI | 65 | 4 | 61 | 1 | 62 |
| SNA VII | 72 | 4 | 68 | 2 | 70 |
| SNA VIII | 77 | 4 | 73 | 2 | 75 |
| SNA IX | 78 | 4 | 74 | 3 | 77 |
| **SNA total** | **447** | **34** | **413** | **12** | **425** |
| **Combined** | **900** | **68** | **832** | **29** | **861** |

## 3. Cause of the stale figures

`882` was the correct **registered and published** generated total at commit
`881db758` ("Publish qualified Sprint 11 ungulates"). At that commit
`SummonVisibilityCatalog` held `RegisteredLogicalPlacementCount = 882`,
`SuppressedLogicalPlacementCount = 0`, so registered and published agreed and
`882 + 29 = 911` was a true statement about that revision.

The next commit, `d7822297` ("Add hidden Sprint 12 summon foundation"),
changed both ends of the equation at once:

- it registered Dire Rat at SM 1 / SNA 1, adding 18 placements (882 -> 900);
- it suppressed `dire-rat`, `dog`, `hyena` and `goblin-dog`, withdrawing 68
  placements (900 -> 832 published).

The constants, the generated roster ledger and the domain tests were all
updated to 900/68/832 in that commit. The prose was not re-derived, so it kept
describing the previous revision. The 0.0.141 release was cut two commits
later and inherited the stale sentence.

This is why the gap is 50 rather than 18: the suppression removed Dog, Hyena
and Goblin Dog, which existed and were published before Sprint 12 began, as
well as the new Dire Rat.

## 4. Disclosure: the 0.0.141 release withdraws three previously published creatures

Derived the same way from `v0.0.140`, whose only suppression was `dire-bat`:

| Release | Registered | Suppressed | Published generated | Wrappers | Visible |
| --- | --- | --- | --- | --- | --- |
| v0.0.140 | 813 | 14 (Dire Bat) | 799 (407 SM + 392 SNA) | 29 | 828 |
| v0.0.141 | 900 | 68 (Sprint 12) | 832 (419 SM + 413 SNA) | 29 | 861 |

Net published generated placements still rise by 33, and Dire Bat's 14
placements become visible for the first time. However, **Dog, Hyena and Goblin
Dog were visible in v0.0.140 and are hidden in v0.0.141**, removing 50
placements a player could previously cast. The release notes did not disclose
that withdrawal, and claimed an increase to 911 visible choices that the build
does not contain.

Save safety is unaffected: the four creatures keep their registered unit,
ability and placement identities, so a save written against v0.0.140 that
contains an active Dog, Hyena or Goblin Dog summon still deserializes under
v0.0.141. Only the menu entries are withheld.

### Disposition

- The published tag `v0.0.141`, its ZIP and its GitHub release metadata are
  immutable under the current order and are **not** modified. This record and
  the repository's own documents carry the correction.
- The withdrawal is resolved by completing Sprint 12 rather than by reverting
  the suppression against unqualified behavior: Sprint 12 changed Goblin Dog's
  mechanics and all three creatures' intended presentation, so republishing
  them before qualification would publish unqualified work.
- When Sprint 12 qualifies, all four creatures publish together on their
  existing registered identities and the totals are re-derived from source, not
  copied from any earlier document.

## 5. Guard against recurrence

`ExpandedSummoningBaselineInventoryTests.VisibleChoicesDecomposeExactly` and
`PublishedInventoryRecordsMatchTheDerivedEquation` assert the derived equation
and that the mutable current records state the same numbers, so a future
suppression or registration change fails the domain suite until every record is
re-derived.

## 6. Superseded statements

The following claims are superseded by section 2. Historical run descriptions
that report what a specific guarded run actually observed at its own source
revision are left intact and annotated, because those observations were true
when made.

| Document | Stale claim | Status |
| --- | --- | --- |
| `docs/RELEASE-NOTES-0.0.141.md` | "882 generated choices and 29 retained native wrappers, for 911 visible choices" | Corrected |
| `EXPANDED-SUMMONING-PHASE2-IMPLEMENTATION-REPORT.md` (header, Sprint 11 section) | 882 generated / 911 visible | Corrected |
| `EXPANDED-SUMMONING-PHASE2-JOURNAL.md` (0.0.141 header) | 882 visible generated choices | Corrected |
| `EXPANDED-SUMMONING-PHASE2-AUTONOMOUS-STATE.md` (release boundary, Sprint 11 checkpoint) | 882 generated / 911 visible | Corrected |
| `planning/EXPANDED-SUMMONING-FIDELITY-MATRIX.md` (header, 0.0.141 boundary) | 882 generated / 911 total | Corrected |
| `CHANGELOG.md` 0.0.141 entry | "882-root live inventory" as the release surface | Corrected with the run's own observed value retained |
| `AUTONOMOUS-RESUME.md`, `AUTONOMOUS-GUNSLINGER-JOURNAL.md` | 882 visible generated choices | Corrected |
| Evidence index rows for `20260930T0457205706691Z` and `20260930T0508037596496Z` | 882 roots observed | Retained as historical observation; annotated as pre-`d7822297` |
