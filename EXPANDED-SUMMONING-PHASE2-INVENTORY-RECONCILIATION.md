# Expanded Summoning publication-inventory reconciliation

Opened 2026-10-01 as the first task of the Claude Phase 2 continuation order.
This record supersedes every earlier prose claim that the 0.0.141 published
catalog contains 882 generated choices or 911 visible choices.

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
