# Favored Class Integration — Coverage Ledger

Human-readable ledger of every charter source row (54 table appearances, 53
distinct options) and every required infrastructure item. The authoritative
row data (publisher, rate, cap, disposition, canonical effect) lives in
`src/KingmakerGunslinger/FavoredClass/FavoredClassCatalog.cs` and is pinned by
the `favored-class.catalog-*` domain tests. This ledger adds implementation
and qualification status. Status ladder: NOT STARTED → CODE COMPLETE →
DOMAIN TESTED → NATIVE TESTED → SAVE TESTED → QUALIFIED. Other dispositions:
BLOCKED, EXPECTED PROVIDER ABSENCE, NOT RUN, OUT OF SCOPE, DEFERRED, EXCLUDED,
ALIAS.

Legend for disposition: I faithful, A adaptation, O optional third-party
(profile OFF by default), P provider-deferred, D engineering-deferred,
X excluded, ALIAS repeated appearance.

## Scheduled rows (30)

| Row | Ancestry → class | Disp. | Canonical effect | Rate / cap | Status | Evidence |
| --- | --- | --- | --- | --- | --- | --- |
| G01 | Dwarf → Gunslinger | I | gunslinger.misfire-by-firearm-type (per type) | 1/4, floor 1 | NOT STARTED | — |
| G02 | Elf → Gunslinger | I | gunslinger.firearm-confirmation | 1/3, cap +5, nonstacking CF | NOT STARTED | — |
| G04 | Half-elf → Gunslinger | I | gunslinger.grit (shared with G07) | 1/4 | NATIVE TESTED (menu route; shared G07 counter) | native `20260924T0101128312625Z-disposable-favored-class-grit` (candidate `2cccec68b`, DLL `5e256ab2…`) race matrix: Half-elf offered grit once |
| G05 | Half-orc → Gunslinger | I | gunslinger.pistol-whip-attack | 1/3 | NOT STARTED | — |
| G06 | Halfling → Gunslinger | I | gunslinger.nimble-halfling + gunslinger.dodge-halfling (two counters) | 1/4 cap +2; 1/4 | NOT STARTED | — |
| G07 | Human → Gunslinger | I | gunslinger.grit | 1/4 | NATIVE TESTED (20 native level-ups; save/load pending) | native `20260924T0101128312625Z-disposable-favored-class-grit` (candidate `2cccec68b`, DLL `5e256ab2…`): ranks (floor(N/4), N-floor(N/4)), +floor(N/4) max grit, no refill, cancel no-leak, closed at 20, Mysterious Stranger +1 at N=4 |
| G08 | Goblin → Gunslinger | I | gunslinger.firearm-confirmation | 1/3, cap +5 | NOT STARTED | — |
| G10 | Hobgoblin → Gunslinger | I | gunslinger.grit | 1/4 | NATIVE TESTED (menu route) | native `20260924T0101128312625Z-disposable-favored-class-grit` (candidate `2cccec68b`, DLL `5e256ab2…`) race matrix (Races Unleashed Hobgoblin) |
| G11 | Ifrit → Gunslinger | I | gunslinger.initiative-deed | 1/2 | NOT STARTED | — |
| G14 | Fetchling → Gunslinger | I | gunslinger.grit | 1/4 | NATIVE TESTED (menu route) | native `20260924T0101128312625Z-disposable-favored-class-grit` (candidate `2cccec68b`, DLL `5e256ab2…`) race matrix (Races Unleashed Fetchling) |
| G16 | Dhampir → Gunslinger (JBE) | O | gunslinger.firearm-confirmation | 1/3, cap +5 | NOT STARTED | — |
| G17 | Drow → Gunslinger (JBE) | O | gunslinger.nimble-drow | 1/6, cap +2 | NOT STARTED | — |
| G18 | Duergar → Gunslinger (JBE) | O | gunslinger.misfire-by-firearm-type | 1/4, floor 1 | NOT STARTED | — |
| G20 | Orc → Gunslinger (JBE) | O | gunslinger.pistol-whip-attack (alias route) | 1/3 | NOT STARTED | — |
| G21 | Tiefling → Gunslinger (JBE) | O | gunslinger.cmb-dirty-trick-trip | 1/2 | NOT STARTED | — |
| I01 | Ifrit → Alchemist | I | alchemist.bomb-damage | 1/2 | NOT STARTED | — |
| I05 | Ifrit → Inquisitor | A | inquisitor.intimidate-fire-subtype (planes knowledge omitted) | 1/2 | NOT STARTED | — |
| I06 | Ifrit → Oracle | I | oracle.selected-revelation (per revelation) | 1/6 | NOT STARTED | — |
| I07 | Ifrit → Rogue | A | rogue.demoralize (jump omitted) | 1/2 | NOT STARTED | — |
| I08 | Ifrit → Sorcerer | I | sorcerer.selected-bloodline-power (fire elemental/efreeti) | 1/6, cap +2 | NOT STARTED | — |
| O01 | Oread → Bard | I | bard.selected-performance-range (per performance) | +5 ft, cap +30 ft | NOT STARTED | — |
| O04 | Oread → Fighter | A | fighter.cmd-bull-rush-drag (drag only if implemented) | +1 | NOT STARTED | — |
| O05 | Oread → Monk | I | monk.unarmed-confirmation | 1/3, cap +5 | NOT STARTED | — |
| O06 | Oread → Paladin | I | paladin.aura-ally-bonus (Courage, Resolve) | 1/4 | NOT STARTED | — |
| O07 | Oread → Ranger | I | ranger.companion-natural-armor | 1/4 | NOT STARTED | — |
| O08 | Oread → Summoner | I | summoner.eidolon-natural-armor | 1/4 | NOT STARTED | — |
| S04 | Sylph → Oracle | I | oracle.selected-revelation (shared with I06) | 1/6 | NOT STARTED | — |
| S06 | Sylph → Sorcerer | I | sorcerer.selected-bloodline-power (air elemental/djinni) | 1/6, cap +2 | NOT STARTED | — |
| U02 | Undine → Cleric | I | cleric.sr-penetration-aquatic-water | +1 | NOT STARTED | — |
| U04 | Undine → Monk | I | monk.grapple-cmd-and-stunning (N CMD, floor(N/3) stunning) | mixed | NOT STARTED | — |

## Alias

| Row | Ancestry → class | Disp. | Note |
| --- | --- | --- | --- |
| I04 | Ifrit → Gunslinger | ALIAS | Same published option as G11; never a second reward. |

## Not scheduled (23) — never published as a placeholder

| Row | Ancestry → class | Disp. | Reason (charter) |
| --- | --- | --- | --- |
| G03 | Gnome → Gunslinger | D | Maintenance runs on completed rests, not repair minutes. |
| G09 | Grippli → Gunslinger | P | No qualified Grippli provider. |
| G12 | Kobold → Gunslinger | P | No qualified Kobold provider. |
| G13 | Ratfolk → Gunslinger | P | No qualified Ratfolk provider. |
| G15 | Kitsune (universal) | D | Needs a Magical Tail provider integration. |
| G19 | Fetchling → Gunslinger (JBE) | D | Lighting-specific concealment path not qualified. |
| G22 | Wayang → Gunslinger (JBE) | P | Provider deferred; distinct 1/4 rate. |
| I02 | Ifrit → Bard | X | No finite-target Fascinate model. |
| I03 | Ifrit → Cleric | X | Narrow planes knowledge context unavailable. |
| O02 | Oread → Cleric | X | Narrow planes knowledge context unavailable. |
| O03 | Oread → Druid | X | Subject-limited knowledge unavailable. |
| S01 | Sylph → Cleric | X | Narrow planes knowledge context unavailable. |
| S02 | Sylph → Druid | X | Subject-limited knowledge unavailable. |
| S03 | Sylph → Inquisitor | D | Motionless/opposed-check contexts not qualified. |
| S05 | Sylph → Rogue | X | Jump and Sense Motive have no faithful mapping. |
| S07 | Sylph → Witch | X | No autonomous familiar skill actor. |
| S08 | Sylph → Wizard | D | Air/Wood school provider unqualified. |
| U01 | Undine → Bard | X | Perform-check Countersong absent. |
| U03 | Undine → Druid | X | No Wild Empathy system. |
| U05 | Undine → Sorcerer | X | No underwater casting check. |
| U06 | Undine → Summoner | D | Range-limited Life Link / aquatic base form unproven. |
| U07 | Undine → Wizard | D | Off-list spell-level design unsettled. |
| U08 | Undine → Shifter | X | No Wild Empathy system. |

## Infrastructure

| Item | Status | Evidence |
| --- | --- | --- |
| Pure rank policy (N = full + partial, ceilings, mixed rates) | DOMAIN TESTED | `favored-class.rank-*` (7 cases) |
| Source catalog (54 appearances, 53 options, 22 effects) | DOMAIN TESTED | `favored-class.catalog-*` (6 cases) |
| Ancestry permission graph (FAQ, host policy, Mostly Human, bounded) | DOMAIN TESTED | `favored-class.eligibility-*` (5 cases) |
| Optional host adapter (exact binary + readiness) | NATIVE TESTED (host present); host-absent profile pending | native `20260924T0102047921088Z-observe-favored-class-contract`: exact SHA-256/MVID/six IL fingerprints, `Core.load` completion marker, readiness |
| Native host progression (Gunslinger in host scan) | NATIVE TESTED | same run: one progression `cbe4e194…` and one bonus selection `5ffbec50…` (MergeIds identities), 20 levels, generic HP/skill rewards unchanged |
| Atomic, idempotent publication with owned rollback | NATIVE TESTED | same run: exact foreign prefix, repeat publication no-op (H06), injected mid-transaction fault rolls back exactly (H08) |
| Scoped ancestry bridge for the 20 host human families | NOT STARTED | — |
| Mostly Human companion racial trait (four parents) | NOT STARTED | — |
| Configuration profiles and lifecycle policy | NOT STARTED | — |
| Diagnostics | NOT STARTED | — |
| Selected-power target manifests | NOT STARTED | — |
| Migration/recovery guidance | NOT STARTED | — |
