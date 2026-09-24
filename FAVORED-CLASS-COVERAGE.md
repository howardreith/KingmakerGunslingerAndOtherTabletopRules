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

Candidate for the native evidence below unless stated: `f2db844c2` (DLL `5a278b75…`, MVID `04b7fb78-5fd9-4080-ae7c-36ae87b727fe`).
Evidence paths are under `C:\Dev\KingmakerGunslingerLab\runtime-evidence\`
(machine-local, not committed).

| Row | Ancestry → class | Disp. | Canonical effect | Rate / cap | Status | Evidence |
| --- | --- | --- | --- | --- | --- | --- |
| G01 | Dwarf → Gunslinger | I | gunslinger.misfire-by-firearm-type (per type) | 1/4, floor 1 | NATIVE TESTED | menus `20260924T0129112635200Z` (`fba7516d3`): three independent per-type counters (6/4/2/8 investments); mechanics `20260924T0349220382365Z`: effective misfire threshold reduced last with floor 1, scatter aggregate uses the effective threshold, other types unchanged |
| G02 | Elf → Gunslinger | I | gunslinger.firearm-confirmation | 1/3, cap +5, nonstacking CF | SAVE TESTED (shared counter) | menus: capped at (5,10) then closed; mechanics: better of Critical Focus, melee and Pistol-Whip surrogate excluded; L01 `20260924T0352059750938Z`/`20260924T0353099892035Z`: Half-elf (elf route) partial rank survives a fresh-process reload |
| G04 | Half-elf → Gunslinger | I | gunslinger.grit (shared with G07) | 1/4 | SAVE TESTED | L01 transaction `word-of-recall-fcb-persistence-20260924T0352021320346Z_ba16f881ae444ef8985ad6eb6d178671`: level-5 Half-elf Pistolero, grit P,P,P,F (full 1 / partial 3, +1 max grit), current grit 1 after spending 1 from 2; ranks, current/max grit and earned steps identical after the fresh-process reload |
| G05 | Half-orc → Gunslinger | I | gunslinger.pistol-whip-attack | 1/3 | NATIVE TESTED | menus: (6,14) over 20 levels; mechanics: attack bonus only on the Pistol-Whip surrogate attack (no shot, damage or CMB leakage) |
| G06 | Halfling → Gunslinger | I | gunslinger.nimble-halfling + gunslinger.dodge-halfling (two counters) | 1/4 cap +2; 1/4 | NATIVE TESTED | menus: Nimble capped at 8 investments then Dodge (3,9); Mysterious Stranger not offered Nimble, Musket Master not offered Dodge; mechanics: Nimble bonus only while Nimble is active in eligible armor; Dodge only inside the deed buff |
| G07 | Human → Gunslinger | I | gunslinger.grit | 1/4 | NATIVE TESTED; SAVE TESTED (shared counter) | grit `20260924T0351081263683Z`: 20 native level-ups, ranks (floor(N/4), N-floor(N/4)), +floor(N/4) max grit, no refill, cancel no-leak, closed at 20, Mysterious Stranger +1 at N=4; persistence through the shared counter (G04 row) |
| G08 | Goblin → Gunslinger | I | gunslinger.firearm-confirmation | 1/3, cap +5 | CODE COMPLETE; route EXPECTED PROVIDER ABSENCE | host goblin race exists but is not in the playable catalog (race matrix in the grit and menus runs); the counter is the G02 counter |
| G10 | Hobgoblin → Gunslinger | I | gunslinger.grit | 1/4 | NATIVE TESTED (menu route) | race matrix (Races Unleashed Hobgoblin) |
| G11 | Ifrit → Gunslinger | I | gunslinger.initiative-deed | 1/2 | NATIVE TESTED | menus: (10,10); initiative timing `20260924T0154493048526Z` (`732cddae3`): the stored combat initiative includes the deed and the earned steps in real-time and turn-based entry (D3 fixed); Sprint 38 regression `20260924T0155347348333Z` PASS |
| G14 | Fetchling → Gunslinger | I | gunslinger.grit | 1/4 | NATIVE TESTED (menu route) | race matrix (Races Unleashed Fetchling) |
| G16 | Dhampir → Gunslinger (JBE) | O | gunslinger.firearm-confirmation | 1/3, cap +5 | CODE COMPLETE (profile OFF) | withheld by default (contract `20260924T0348360182600Z`); profile-ON publication NOT RUN |
| G17 | Drow → Gunslinger (JBE) | O | gunslinger.nimble-drow | 1/6, cap +2 | NATIVE TESTED (mechanics, profile OFF) | withheld by default; Drow Nimble mechanics in the mechanics run; profile-ON menus NOT RUN |
| G18 | Duergar → Gunslinger (JBE) | O | gunslinger.misfire-by-firearm-type | 1/4, floor 1 | CODE COMPLETE (profile OFF) | shared G01 counters; profile-ON route NOT RUN |
| G20 | Orc → Gunslinger (JBE) | O | gunslinger.pistol-whip-attack (alias route) | 1/3 | CODE COMPLETE (profile OFF) | shared G05 counter; no playable Orc race in the installed stack |
| G21 | Tiefling → Gunslinger (JBE) | O | gunslinger.cmb-dirty-trick-trip | 1/2 | NATIVE TESTED (mechanics, profile OFF) | withheld by default; trip and the three dirty tricks only, CMD and other maneuvers unchanged; profile-ON menus NOT RUN |
| I01 | Ifrit → Alchemist | I | alchemist.bomb-damage | 1/2 | NATIVE TESTED (menus, progression); mechanics re-run pending | elemental core `20260924T0347450150348Z`: offered to Ifrit Alchemists only, not to Humans or Vivisectionists; flat damage observed (+3 at three steps) on the bomb context; the zero-base probe hid +1 under the native damage floor (probe fixed in `4c9409ba4`) |
| I05 | Ifrit → Inquisitor | A | inquisitor.intimidate-fire-subtype (planes knowledge omitted) | 1/2 | NATIVE TESTED | +2 on native Demoralize against a fire-subtype creature, none against others, none for a control |
| I06 | Ifrit → Oracle | I | oracle.selected-revelation (per revelation) | 1/6 | NOT STARTED | — |
| I07 | Ifrit → Rogue | A | rogue.demoralize (jump omitted) | 1/2 | NATIVE TESTED | +2 only on Intimidate made by the native Demoralize action; other Intimidate checks unchanged |
| I08 | Ifrit → Sorcerer | I | sorcerer.selected-bloodline-power (fire elemental/efreeti) | 1/6, cap +2 | NOT STARTED | — |
| O01 | Oread → Bard | I | bard.selected-performance-range (per performance) | +5 ft, cap +30 ft | NOT STARTED | — |
| O04 | Oread → Fighter | A | fighter.cmd-bull-rush-drag (drag only if implemented) | +1 | NATIVE TESTED | lockstep native progression: +3 bull rush CMD at three investments, trip CMD unchanged |
| O05 | Oread → Monk | I | monk.unarmed-confirmation | 1/3, cap +5 | NATIVE TESTED | +5 on native unarmed strikes (Kingmaker marks them IsUnarmed and IsNatural; fixed in `d9d09b8b8`), none on a longsword or a natural claw; with Critical Focus the better of the two (5 over 4) |
| O06 | Oread → Paladin | I | paladin.aura-ally-bonus (Courage, Resolve) | 1/4 | NOT STARTED | — |
| O07 | Oread → Ranger | I | ranger.companion-natural-armor | 1/4 | NOT STARTED | — |
| O08 | Oread → Summoner | I | summoner.eidolon-natural-armor | 1/4 | NOT STARTED | — |
| S04 | Sylph → Oracle | I | oracle.selected-revelation (shared with I06) | 1/6 | NOT STARTED | — |
| S06 | Sylph → Sorcerer | I | sorcerer.selected-bloodline-power (air elemental/djinni) | 1/6, cap +2 | NOT STARTED | — |
| U02 | Undine → Cleric | I | cleric.sr-penetration-aquatic-water | +1 | NATIVE TESTED | +3 spell penetration at three ranks against water and aquatic subtypes, none against other targets or for a control caster |
| U04 | Undine → Monk | I | monk.grapple-cmd-and-stunning (N CMD, floor(N/3) stunning) | mixed | NATIVE TESTED | six lockstep native level-ups: +6 grapple CMD, +2 Stunning Fist uses, ranks 2/4, bull rush CMD unchanged |

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
| Ancestry permission graph (FAQ, host policy, Mostly Human, bounded) | DOMAIN TESTED; native routes for FAQ and host policy | `favored-class.eligibility-*` (5 cases); menus race matrix (Half-elf grit + confirmation, Half-orc grit + Pistol-Whip, Aasimar/Tiefling grit) |
| Optional host adapter (exact binary + readiness) | NATIVE TESTED (host present and host absent) | contract `20260924T0348360182600Z`: exact SHA-256/MVID/six IL fingerprints, completion marker; H01 isolated gunslinger-only profile `20260924T0140349022134Z` (`cf8a66d21`, profile `compat-20260924T014026Z-1da92b038b7e`, restoration verified): HostAbsent, owned leaves registered, no FCB publication, class offered; CotW-only and CotW+FCB profiles BLOCKED (B2) |
| Native host progression (Gunslinger in host scan) | NATIVE TESTED | one progression `cbe4e194…` and one bonus selection `5ffbec50…`, 20 levels, generic HP/skill rewards unchanged |
| Atomic, idempotent publication with owned rollback (all host selections) | NATIVE TESTED | contract `20260924T0348360182600Z`: every host class selection holds exactly its owned suffix after untouched foreign entries; profile-withheld leaves absent; repeat publication no-op (H06); injected fault rolls back exactly (H08) |
| Fresh-process persistence (L01) | SAVE TESTED | Word of Recall FCB transaction `word-of-recall-fcb-persistence-20260924T0352021320346Z_ba16f881ae444ef8985ad6eb6d178671` (prepare `20260924T0352059750938Z`, verify `20260924T0353099892035Z`): Gunslinger ranks, target identities, current/max grit and effects identical; disposable `KMG_FCB_PERSISTENCE_*` save only, cleanup restored |
| Configuration profiles (restart-required settings file) | DOMAIN TESTED; native defaults observed | `favored-class.settings-*` (3 cases); contract `20260924T0348360182600Z` `fcb-settings-profile`: file absent, charter defaults, effective profile equals the resolved one; third-party-ON and integration-OFF runs NOT RUN |
| Scoped ancestry bridge for the 20 host human families | NOT STARTED | — |
| Mostly Human companion racial trait (four parents) | NOT STARTED | — |
| Lifecycle policy (L02–L10) | NOT STARTED | — |
| Diagnostics | CODE COMPLETE | `[favored-class]` log events (settings, host decision, publication, rollback); host-state observer |
| Selected-power target manifests | NOT STARTED | — |
| Migration/recovery guidance | NOT STARTED | — |
