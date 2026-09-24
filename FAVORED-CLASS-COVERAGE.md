# Favored Class Integration — Coverage Ledger

Human-readable ledger of every charter source row (54 table appearances, 53
distinct options), every required infrastructure item and the 64 charter
scenario families. The authoritative row data (publisher, rate, cap,
disposition, canonical effect) lives in
`src/KingmakerGunslinger/FavoredClass/FavoredClassCatalog.cs` and is pinned by
the `favored-class.catalog-*` domain tests. The machine-readable form of this
ledger is `FAVORED-CLASS-COVERAGE.json`. Status ladder: NOT STARTED → CODE
COMPLETE → DOMAIN TESTED → NATIVE TESTED → SAVE TESTED → QUALIFIED. Other
dispositions: BLOCKED, EXPECTED PROVIDER ABSENCE, NOT RUN, OUT OF SCOPE,
DEFERRED, EXCLUDED, ALIAS. No row is QUALIFIED; qualification needs the
owner's review of this local candidate.

Legend for disposition: I faithful, A adaptation, O optional third-party
(profile OFF by default), P provider-deferred, D engineering-deferred,
X excluded, ALIAS repeated appearance.

Evidence entries are run identifiers under
`C:\Dev\KingmakerGunslingerLab\runtime-evidence\` (machine-local, not
committed). Target manifests are in `docs/FAVORED-CLASS-TARGET-MANIFEST.md`.

## Final candidate regression

Candidate `ef250f877`: DLL `fe451f35…`, MVID `2c9d8c5f-c424-4179-91cd-72ca85438c54`. Every run used the
guarded Steam App ID 640820 launcher.

| Scenario | Run (final candidate `ef250f877`) |
| --- | --- |
| `observe-favored-class-contract` | 20260924T0603184328859Z PASS |
| `observe-favored-class-host-state` | 20260924T0604015343603Z PASS |
| `disposable-favored-class-grit` | 20260924T0604457833407Z PASS |
| `disposable-favored-class-gunslinger-menus` | 20260924T0605443253994Z PASS |
| `disposable-favored-class-gunslinger-mechanics` | 20260924T0606461797306Z PASS |
| `disposable-favored-class-initiative-timing` | 20260924T0607291357025Z PASS |
| `disposable-favored-class-elemental-core` | 20260924T0608135881312Z PASS |
| `disposable-favored-class-mostly-human` | 20260924T0609003356634Z PASS |
| `disposable-favored-class-elemental-advanced` | 20260924T0609525254026Z PASS |
| `disposable-favored-class-oracle-revelations` | 20260924T0601519001416Z PASS |
| `disposable-favored-class-performance-range` | 20260924T0602359332310Z PASS |
| L01 fresh-process persistence (`Invoke-WordOfRecallFavoredClassPersistence.ps1`) | 20260924T0612102884372Z (prepare) + 20260924T0613092420246Z (verify) PASS; transaction word-of-recall-fcb-persistence-20260924T0612076008434Z_556ce961a4974babba75ab6c3145bb78 (settings and complete Mods tree restored) |
| `working-save-smoke` (KMG_AUTOMATION_WORKING, 11/11) | 20260924T0615397121565Z PASS |

## Scheduled rows (30)

The row evidence names the run that exercised the row in detail. The final
candidate re-ran every scenario listed above.

| Row | Ancestry → class | Disp. | Canonical effect | Rate / cap | Status | Evidence |
| --- | --- | --- | --- | --- | --- | --- |
| G01 | Dwarf → Gunslinger | I | gunslinger.misfire-by-firearm-type (per type) | 1/4, floor 1 | NATIVE TESTED | menus: three independent per-type counters; mechanics: effective threshold reduced last with floor 1, other types unchanged |
| G02 | Elf → Gunslinger | I | gunslinger.firearm-confirmation | 1/3, cap +5, nonstacking CF | SAVE TESTED (shared counter) | menus: capped at (5,10) then closed; mechanics: better of Critical Focus; L01 `20260924T0352059750938Z`/`20260924T0353099892035Z` |
| G04 | Half-elf → Gunslinger | I | gunslinger.grit (shared with G07) | 1/4 | SAVE TESTED | L01 transaction `word-of-recall-fcb-persistence-20260924T0352021320346Z_ba16f881ae444ef8985ad6eb6d178671` |
| G05 | Half-orc → Gunslinger | I | gunslinger.pistol-whip-attack | 1/3 | NATIVE TESTED | menus (6,14); mechanics: only the Pistol-Whip surrogate attack |
| G06 | Halfling → Gunslinger | I | nimble-halfling + dodge-halfling (two counters) | 1/4 cap +2; 1/4 | NATIVE TESTED | menus: Nimble capped then Dodge; Mysterious Stranger and Musket Master exclusions; mechanics: genuine Nimble and deed buff only |
| G07 | Human → Gunslinger | I | gunslinger.grit | 1/4 | NATIVE TESTED; SAVE TESTED (shared counter) | grit: 20 native level-ups, no refill, cancel leaves no leak |
| G08 | Goblin → Gunslinger | I | gunslinger.firearm-confirmation | 1/3, cap +5 | CODE COMPLETE; route EXPECTED PROVIDER ABSENCE | the host goblin race is not in the playable catalog; the counter is G02's |
| G10 | Hobgoblin → Gunslinger | I | gunslinger.grit | 1/4 | NATIVE TESTED (menu route) | race matrix (Races Unleashed Hobgoblin) |
| G11 | Ifrit → Gunslinger | I | gunslinger.initiative-deed | 1/2 | NATIVE TESTED | initiative timing in real-time and turn-based entry; D3 fixed in `732cddae3` |
| G14 | Fetchling → Gunslinger | I | gunslinger.grit | 1/4 | NATIVE TESTED (menu route) | race matrix (Races Unleashed Fetchling) |
| G16 | Dhampir → Gunslinger (JBE) | O | gunslinger.firearm-confirmation | 1/3, cap +5 | NATIVE TESTED (profile ON) | third-party menus `20260924T0538189215967Z` (`9b3db78bf`); withheld by default |
| G17 | Drow → Gunslinger (JBE) | O | gunslinger.nimble-drow | 1/6, cap +2 | NATIVE TESTED | profile-ON menus; mechanics under the default profile (L05) |
| G18 | Duergar → Gunslinger (JBE) | O | gunslinger.misfire-by-firearm-type | 1/4, floor 1 | NATIVE TESTED (profile ON) | Duergar offered the per-type misfire counters |
| G20 | Orc → Gunslinger (JBE) | O | gunslinger.pistol-whip-attack (alias route) | 1/3 | CODE COMPLETE; route EXPECTED PROVIDER ABSENCE | no playable Orc race; Half-orcs hold the same G05 counter |
| G21 | Tiefling → Gunslinger (JBE) | O | gunslinger.cmb-dirty-trick-trip | 1/2 | NATIVE TESTED | profile-ON menus; mechanics: trip and dirty tricks only |
| I01 | Ifrit → Alchemist | I | alchemist.bomb-damage | 1/2 | NATIVE TESTED | elemental core: flat bonus on a 5d1 bomb, above the damage floor; Vivisectionist excluded |
| I05 | Ifrit → Inquisitor | A | inquisitor.intimidate-fire-subtype (planes knowledge omitted) | 1/2 | NATIVE TESTED | native Demoralize against a fire-subtype target only |
| I06 | Ifrit → Oracle | I | oracle.selected-revelation (52 revelation targets) | 1/6 per revelation | NATIVE TESTED (PARTIAL values) | oracle run: all 52 scoped with every audited family; the Ifrit menu offers exactly the owned revelation; Fire Breath at level 9 with two steps: CL 11, DC +1, dice 11, uses 3; neighbors and Fireball unchanged; held tiers unmoved; feature context refreshes on gain and removal |
| I07 | Ifrit → Rogue | A | rogue.demoralize (jump omitted) | 1/2 | NATIVE TESTED | the native Demoralize check only |
| I08 | Ifrit → Sorcerer | I | sorcerer.selected-bloodline-power (fire) | 1/6, cap +2 | NATIVE TESTED | advanced run: Blast CL 9→11, DC 14→15, dice 9→11; Ray damage bonus 5→6; neighbor and Fireball unchanged |
| O01 | Oread → Bard | I | bard.selected-performance-range (15 performances) | +5 ft, cap +30 ft per performance | NATIVE TESTED | performance run: own Inspire Courage area 50→60 ft, 80 ft at the cap; a 55-ft point inside only when widened; other performance, other performer and blueprint unchanged; Human and Archaeologist not offered |
| O04 | Oread → Fighter | A | fighter.cmd-bull-rush-drag (drag omitted) | +1 | NATIVE TESTED | bull rush CMD only |
| O05 | Oread → Monk | I | monk.unarmed-confirmation | 1/3, cap +5 | NATIVE TESTED | unarmed strikes only (not claws or weapons); better of Critical Focus |
| O06 | Oread → Paladin | I | paladin.aura-ally-bonus (Courage, Resolve) | 1/4 | NATIVE TESTED | advanced run: fear +4/+5/+6 and charm +4/+6 by each paladin's own steps; other saves unchanged; Divine Hunter not offered |
| O07 | Oread → Ranger | I | ranger.companion-natural-armor | 1/4 | NATIVE TESTED | +2 then +3 on the current companion; stacks with Barkskin; touch and master AC unchanged; replacement leaves no orphan; removal clears it |
| O08 | Oread → Summoner | I | summoner.eidolon-natural-armor | 1/4 | NATIVE TESTED | +2 on Call of the Wild's eidolon |
| S04 | Sylph → Oracle | I | oracle.selected-revelation (shared with I06) | 1/6 | NATIVE TESTED | oracle run: a Sylph Wind Oracle is offered exactly Lightning Breath |
| S06 | Sylph → Sorcerer | I | sorcerer.selected-bloodline-power (air) | 1/6, cap +2 | NATIVE TESTED (menus; values through the shared adapter) | the Sylph air menu offers AirRay only; Ifrit with air gets none |
| U02 | Undine → Cleric | I | cleric.sr-penetration-aquatic-water | +1 | NATIVE TESTED | water and aquatic targets only |
| U04 | Undine → Monk | I | monk.grapple-cmd-and-stunning | mixed | NATIVE TESTED | +N grapple CMD and floor(N/3) Stunning Fist uses |

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
| Pure rank policy | DOMAIN TESTED; NATIVE TESTED (d = 2, 3, 4, 6; capped and uncapped) | `favored-class.rank-*`; grit and menus runs |
| Source catalog (54 appearances, 53 options, 22 effects) | DOMAIN TESTED | `favored-class.catalog-*` |
| Ancestry permission graph | DOMAIN TESTED; NATIVE TESTED | `favored-class.eligibility-*`; menus race matrix; Mostly Human run |
| Optional host adapter (exact binary + readiness) | NATIVE TESTED (host present; host absent) | contract on the final candidate; H01 isolated profile `20260924T0140349022134Z` |
| Native host progression | NATIVE TESTED | one Gunslinger progression and bonus selection, 20 levels |
| Atomic, idempotent publication with owned rollback | NATIVE TESTED | contract: owned leaves appended as suffixes; repeat is a no-op; an injected fault is restored exactly |
| Fresh-process persistence (L01) | SAVE TESTED (Gunslinger counters) | Word of Recall FCB transaction; other counters NOT RUN (OD-10) |
| Configuration profiles (restart-required settings file) | NATIVE TESTED | `07c21f29b`: third-party ON `20260924T0455537582112Z`, Mostly Human OFF `20260924T0456353591331Z`, integration OFF `20260924T0457176732605Z` (mechanics suppressed, identities registered), invalid file `20260924T0458003051455Z` (defaults, reported); the file was restored absent after each |
| Scoped ancestry bridge (host human prerequisites) | NATIVE TESTED | Mostly Human run: 57 tracked prerequisites, open for Mostly Human and closed for Standard; Human, Half-elf and Aasimar true; Dwarf false; other race checks unchanged |
| Mostly Human companion racial trait (four parents) | NATIVE TESTED | fact diff {Trait, Identity} added and {Standard} removed; race, RaceId and scores unchanged; no Outsider or Human race facts |
| Selected-power target manifests | NATIVE TESTED | `docs/FAVORED-CLASS-TARGET-MANIFEST.md`; oracle and advanced runs |
| Lifecycle policy (L02–L10) | PARTIAL | scenario families below |
| Diagnostics | NATIVE TESTED | `[favored-class]` log events; host-state observer |
| Migration/recovery guidance | CODE COMPLETE (documented) | `docs/FAVORED-CLASS-COMPATIBILITY.md` |
| Icon dispositions | DOMAIN TESTED (catalog validator); art pending (OD-8) | 183 consumers recorded; 127 visible choices show the null-icon placeholder |

## Scenario families (64)

`FAVORED-CLASS-COVERAGE.json` holds each family's status, evidence and notes.

| Family | NATIVE/SAVE TESTED | DOMAIN/CODE only | PARTIAL | NOT RUN / OUT OF SCOPE |
| --- | --- | --- | --- | --- |
| H01–H10 | H01 H03 H06 H07 H08 H09 H10 | H02 H04 H05 | — | — |
| E01–E16 | E01 E02 E03 E04 E05 E06 E07 E12 E14 | E08 E10 | E13 E15 | E09 (out of scope), E11, E16 |
| M01–M28 | M01 M02 M03 M04 M05 M06 M08 M09 M11 M15 M17 M18 M19 M20 M21 M22 M23 M25 M28 | M27 | M07 M10 M12 M13 M14 M16 M24 M26 | — |
| L01–L10 | L01 L04 L05 L09 | L06 L08 L10 | L07 | L02 L03 |
