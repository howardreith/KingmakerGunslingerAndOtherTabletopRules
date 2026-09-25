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

Candidate `5636d940b`: package `e7828df6...`, DLL `197ba4e5...`, MVID `76aef19e-962c-427d-b10e-864274b981b0`. Every run used the guarded
Steam App ID 640820 launcher against that one deployment.

| Scenario | Run | Result |
| --- | --- | --- |
| observe-favored-class-contract | 20260925T2042120333525Z-observe-favored-class-contract | PASS |
| observe-favored-class-host-state | 20260925T2042519693304Z-observe-favored-class-host-state | PASS |
| disposable-favored-class-elemental-core | 20260925T2043317409823Z-disposable-favored-class-elemental-core | PASS |
| disposable-favored-class-elemental-advanced | 20260925T2044150219285Z-disposable-favored-class-elemental-advanced | PASS |
| disposable-favored-class-oracle-revelations | 20260925T2044567973816Z-disposable-favored-class-oracle-revelations | PASS |
| disposable-favored-class-performance-range | 20260925T2045379719640Z-disposable-favored-class-performance-range | PASS |
| working-save-favored-class-lifecycle@KMG_AUTOMATION_WORKING | 20260925T2046176703078Z-working-save-favored-class-lifecycle | PASS |
| disposable-favored-class-respec | 20260925T2047194098043Z-disposable-favored-class-respec | PASS |
| working-save-favored-class-visual-census@KMG_AUTOMATION_WORKING | 20260925T2048326457443Z-working-save-favored-class-visual-census | PASS |
| disposable-favored-class-grit | 20260925T2050254669887Z-disposable-favored-class-grit | PASS |
| disposable-favored-class-gunslinger-menus | 20260925T2051123820204Z-disposable-favored-class-gunslinger-menus | PASS |
| disposable-favored-class-gunslinger-mechanics | 20260925T2052034733338Z-disposable-favored-class-gunslinger-mechanics | PASS |
| disposable-favored-class-initiative-timing | 20260925T2052448363589Z-disposable-favored-class-initiative-timing | PASS |
| disposable-favored-class-mostly-human | 20260925T2053254896577Z-disposable-favored-class-mostly-human | PASS |
| observe-favored-class-performance-visuals | 20260925T2054217536420Z-observe-favored-class-performance-visuals | PASS |
| disposable-favored-class-multiclass | 20260925T2055150395487Z-disposable-favored-class-multiclass | PASS |
| working-save-elemental-character-creation-regression@KMG_AUTOMATION_WORKING#class=Fighter;allocation=point-buy;nativeActionCase=racial-actions;race=Ifrit | 20260925T2056001020492Z-working-save-elemental-character-creation-regression | PASS |
| working-save-elemental-character-creation-regression@KMG_AUTOMATION_WORKING#class=Fighter;allocation=point-buy;nativeActionCase=racial-actions;race=Oread | 20260925T2101226212743Z-working-save-elemental-character-creation-regression | PASS |
| working-save-elemental-character-creation-regression@KMG_AUTOMATION_WORKING#class=Fighter;allocation=point-buy;nativeActionCase=racial-actions;race=Sylph | 20260925T2106287918109Z-working-save-elemental-character-creation-regression | PASS |
| working-save-elemental-character-creation-regression@KMG_AUTOMATION_WORKING#class=Fighter;allocation=point-buy;nativeActionCase=racial-actions;race=Undine | 20260925T2111512826181Z-working-save-elemental-character-creation-regression | PASS |
| working-save-elemental-native-respec@KMG_AUTOMATION_WORKING#class=Fighter;allocation=point-buy;race=Sylph | 20260925T2116212873781Z-working-save-elemental-native-respec | PASS |
| working-save-elemental-native-respec@KMG_AUTOMATION_WORKING#class=Fighter;allocation=point-buy;race=Oread | 20260925T2123088572792Z-working-save-elemental-native-respec | PASS |
| working-save-smoke@KMG_AUTOMATION_WORKING | 20260925T2133053163044Z-working-save-smoke | PASS |
| observe-favored-class-contract | 20260925T2134547060584Z-observe-favored-class-contract | PASS |
| disposable-favored-class-gunslinger-menus | 20260925T2135365440510Z-disposable-favored-class-gunslinger-menus | PASS |
| observe-favored-class-contract | 20260925T2136346225147Z-observe-favored-class-contract | PASS |
| observe-favored-class-host-state | 20260925T2137164575164Z-observe-favored-class-host-state | PASS |
| observe-favored-class-contract | 20260925T2137571099119Z-observe-favored-class-contract | PASS |
| L01 persistence prepare | 20260925T2130294793388Z-disposable-word-of-recall-favored-class-persistence | PASS |
| L01 persistence verify | 20260925T2131319946515Z-disposable-word-of-recall-favored-class-persistence | PASS |
| profile gunslinger-only | 20260925T2141418629762Z-observe-favored-class-host-state | PASS |
| profile gunslinger-call-of-the-wild | 20260925T2144578093484Z-observe-favored-class-host-state | PASS |
| profile gunslinger-call-of-the-wild-favored-class | 20260925T2148287677920Z-observe-favored-class-host-state | PASS |

## Scheduled rows (30)

The row evidence names the run that exercised the row in detail. The final
candidate re-ran every scenario listed above.

| Row | Ancestry → class | Disp. | Canonical effect | Rate / cap | Status | Evidence |
| --- | --- | --- | --- | --- | --- | --- |
| G01 | Dwarf → Gunslinger | I | gunslinger.misfire-by-firearm-type (per type) | 1/4, floor 1 | NATIVE TESTED; SAVE TESTED (pistol target) | menus: three independent per-type counters; mechanics: effective threshold reduced last with floor 1, other types unchanged; L01 families: a Dwarf's pistol misfire step reloads, the production path still reads Pistol 1, Musket 0 and Blunderbuss 0, and seeded native shots after the reload misfire only as that step allows (control, floor and musket unchanged) |
| G02 | Elf → Gunslinger | I | gunslinger.firearm-confirmation | 1/3, cap +5, nonstacking CF | SAVE TESTED (shared counter) | menus: capped at (5,10) then closed; mechanics: better of Critical Focus; L01 `20260924T0352059750938Z`/`20260924T0353099892035Z` |
| G04 | Half-elf → Gunslinger | I | gunslinger.grit (shared with G07) | 1/4 | SAVE TESTED | L01 transaction `word-of-recall-fcb-persistence-20260924T0352021320346Z_ba16f881ae444ef8985ad6eb6d178671` |
| G05 | Half-orc → Gunslinger | I | gunslinger.pistol-whip-attack | 1/3 | NATIVE TESTED | menus (6,14); mechanics: only the Pistol-Whip surrogate attack |
| G06 | Halfling → Gunslinger | I | nimble-halfling + dodge-halfling (two counters) | 1/4 cap +2; 1/4 | NATIVE TESTED; SAVE TESTED (Nimble) | menus: Nimble capped then Dodge; Mysterious Stranger and Musket Master exclusions; mechanics: genuine Nimble and deed buff only; L01 families: a Halfling Nimble step reloads its dodge modifier |
| G07 | Human → Gunslinger | I | gunslinger.grit | 1/4 | NATIVE TESTED; SAVE TESTED (shared counter) | grit: 20 native level-ups, no refill, cancel leaves no leak |
| G08 | Goblin → Gunslinger | I | gunslinger.firearm-confirmation | 1/3, cap +5 | CODE COMPLETE; route EXPECTED PROVIDER ABSENCE | the host goblin race is not in the playable catalog; the counter is G02's |
| G10 | Hobgoblin → Gunslinger | I | gunslinger.grit | 1/4 | NATIVE TESTED (menu route) | race matrix (Races Unleashed Hobgoblin) |
| G11 | Ifrit → Gunslinger | I | gunslinger.initiative-deed | 1/2 | NATIVE TESTED | initiative timing in real-time and turn-based entry; D3 fixed in `732cddae3` |
| G14 | Fetchling → Gunslinger | I | gunslinger.grit | 1/4 | NATIVE TESTED (menu route) | race matrix (Races Unleashed Fetchling) |
| G16 | Dhampir → Gunslinger (JBE) | O | gunslinger.firearm-confirmation | 1/3, cap +5 | NATIVE TESTED (profile ON) | third-party menus `20260924T0538189215967Z` (`9b3db78bf`); withheld by default; final candidate: third-party ON menus `20260924T2313502108487Z` (the profile ON publishes 166 leaves) |
| G17 | Drow → Gunslinger (JBE) | O | gunslinger.nimble-drow | 1/6, cap +2 | NATIVE TESTED | profile-ON menus; mechanics under the default profile (L05); final candidate: third-party ON menus `20260924T2313502108487Z` (the profile ON publishes 166 leaves) |
| G18 | Duergar → Gunslinger (JBE) | O | gunslinger.misfire-by-firearm-type | 1/4, floor 1 | NATIVE TESTED (profile ON) | Duergar offered the per-type misfire counters; final candidate: third-party ON menus `20260924T2313502108487Z` (the profile ON publishes 166 leaves) |
| G20 | Orc → Gunslinger (JBE) | O | gunslinger.pistol-whip-attack (alias route) | 1/3 | CODE COMPLETE; route EXPECTED PROVIDER ABSENCE | no playable Orc race; Half-orcs hold the same G05 counter |
| G21 | Tiefling → Gunslinger (JBE) | O | gunslinger.cmb-dirty-trick-trip | 1/2 | NATIVE TESTED | profile-ON menus; mechanics: trip and dirty tricks only; final candidate: third-party ON menus `20260924T2313502108487Z` (the profile ON publishes 166 leaves) |
| I01 | Ifrit → Alchemist | I | alchemist.bomb-damage | 1/2 | NATIVE TESTED | elemental core: flat bonus on a 5d1 bomb, above the damage floor; Vivisectionist excluded |
| I05 | Ifrit → Inquisitor | A | inquisitor.intimidate-fire-subtype (planes knowledge omitted) | 1/2 | NATIVE TESTED | native Demoralize against a fire-subtype target only |
| I06 | Ifrit → Oracle | I | oracle.selected-revelation (52 revelation targets; 51 published, Spirit of the Warrior excluded) | 1/6 per revelation | NATIVE TESTED; SAVE TESTED (Fire Breath) | oracle run: all 52 scoped with every audited family; the Ifrit menu offers exactly the owned revelation; Fire Breath at level 9 with two steps: CL 11, DC +1, dice 11, uses 3; neighbors and Fireball unchanged; excluded or withheld counters are inert (a held Spirit of the Warrior rank changes nothing); own level gates follow the effective level (83 moved natively); feature context refreshes on gain and removal; L01 families: the Ifrit Fire Breath counter reloads with its caster level, DC, dice and uses; the census renders every revelation leaf |
| I07 | Ifrit → Rogue | A | rogue.demoralize (jump omitted) | 1/2 | NATIVE TESTED | the native Demoralize check only; a failure inside a demoralize never reaches the next check (finally-closed envelope) |
| I08 | Ifrit → Sorcerer | I | sorcerer.selected-bloodline-power (fire) | 1/6, cap +2 | NATIVE TESTED; SAVE TESTED (Fire Ray); RESPEC TESTED | advanced run: Blast CL 9→11, DC 14→15, dice 9→11; Blast's extra uses at 17 and 20 follow the effective level; Ray damage bonus 5→6; neighbor and Fireball unchanged; L01 families: the Fire Ray arithmetic reloads; respec: an Ifrit Sorcerer 6 respecced to Sorcerer 1 keeps no counter and native level-1 arithmetic |
| O01 | Oread → Bard | I | bard.selected-performance-range (14 published performances) | +5 ft, cap +30 ft per performance | NATIVE TESTED; SAVE TESTED | performance run: own Inspire Courage area 50→60 ft, 80 ft at the cap; a 55-ft point inside only when widened; other performance, other performer and blueprint unchanged; Human and Archaeologist not offered; redesign: the area, its ring and the performance's description all show the owner's range, for that bard only; the descriptions follow the owner's live areas of the performance, which widen only as a whole (native while any live area is native, their actual range while all are widened, the configured range once none is live; no outcome is remembered); a widened area whose rollback cannot be verified is ended and the toggle whose own buff runs it is turned off (lifecycle `fcb-lifecycle-toggle-rollback`); Storm Call and Mockery excluded; lifecycle: membership, movement, interruption, death and area reload; L01 families: the widened area and its ring reload |
| O04 | Oread → Fighter | A | fighter.cmd-bull-rush-drag (drag omitted) | +1 | NATIVE TESTED | bull rush CMD only |
| O05 | Oread → Monk | I | monk.unarmed-confirmation | 1/3, cap +5 | NATIVE TESTED | unarmed strikes only (not claws or weapons); better of Critical Focus |
| O06 | Oread → Paladin | I | paladin.aura-ally-bonus (Courage, Resolve) | 1/4 | NATIVE TESTED; SAVE TESTED | advanced run: fear +4/+5/+6 and charm +4/+6 by each paladin's own steps; other saves unchanged; Divine Hunter not offered; lifecycle: two overlapping paladins follow the native Replace and Morale rules (never a sum, no stacking with Remove Fear); L01 families: the reloaded aura still gives +6 |
| O07 | Oread → Ranger | I | ranger.companion-natural-armor | 1/4 | NATIVE TESTED; SAVE TESTED; RESPEC TESTED | +2 then +3 on the current companion; stacks with Barkskin; touch and master AC unchanged; replacement leaves no orphan; removal clears it; lifecycle: death, polymorph, reload; respec: the old companion is destroyed without an orphaned projection and the new one receives it once; transitions: unlink, relink, qualification loss, dismissal and resummoning keep exactly one projection on the qualified pet; L01 families: reload, replacement and unlink after reload |
| O08 | Oread → Summoner | I | summoner.eidolon-natural-armor | 1/4 | NATIVE TESTED | +2 on Call of the Wild's eidolon |
| S04 | Sylph → Oracle | I | oracle.selected-revelation (shared with I06) | 1/6 | NATIVE TESTED | oracle run: a Sylph Wind Oracle is offered exactly Lightning Breath |
| S06 | Sylph → Sorcerer | I | sorcerer.selected-bloodline-power (air) | 1/6, cap +2 | NATIVE TESTED (menus; values through the shared adapter) | the Sylph air menu offers AirRay only; Ifrit with air gets none |
| U02 | Undine → Cleric | I | cleric.sr-penetration-aquatic-water | +1 | NATIVE TESTED | water and aquatic targets only |
| U04 | Undine → Monk | I | monk.grapple-cmd-and-stunning | mixed | NATIVE TESTED; SAVE TESTED | +N grapple CMD and floor(N/3) Stunning Fist uses; lifecycle; L01 families: the mixed-rate grapple CMD and Stunning Fist maximum reload |

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
| Optional host adapter (exact binary + readiness) | NATIVE TESTED (host present; host absent) | contract and host-state on the final candidate; the isolated `gunslinger-only` (host absent), `gunslinger-call-of-the-wild` and `gunslinger-call-of-the-wild-favored-class` profiles on the final candidate (report section 4) |
| Native host progression | NATIVE TESTED | one Gunslinger progression and bonus selection, 20 levels |
| Atomic, idempotent publication with owned rollback | NATIVE TESTED | contract: owned leaves appended as suffixes; repeat is a no-op; an injected fault is restored exactly |
| Fresh-process persistence (L01) | SAVE TESTED (one subject per state/mechanic family) | Word of Recall FCB transaction on the final candidate: partial and full grit with spent grit and its raised maximum, firearm confirmation, the selected firearm (a Dwarf's pistol misfire step, with native shots after the reload), performance range and ring, paladin aura (+6 after the reload), companion armor and its replacement after the reload, Undine Monk grapple CMD and Stunning Fist, Fire Breath and Fire Ray arithmetic, Halfling Nimble and Mostly Human identity with human access; the rows G05, G06 (Dodge), G11, G17, G21, I01, I05, I07, O04, O05, O08, U02, S04 and S06 have no persistence case of their own (NOT RUN) |
| Configuration profiles (restart-required settings file) | NATIVE TESTED | final candidate: third-party ON (contract and menus), Mostly Human OFF, integration OFF (mechanics suppressed, identities registered) and an invalid file (defaults, reported); the file was removed and verified absent after each profile (report section 4) |
| Scoped ancestry bridge (host human prerequisites) | NATIVE TESTED | Mostly Human run: 57 tracked prerequisites, open for Mostly Human and closed for Standard; Human, Half-elf and Aasimar true; Dwarf false; other race checks unchanged |
| Mostly Human companion racial trait (four parents) | NATIVE TESTED; SAVE TESTED; RESPEC TESTED | a genuine dual identity: human (humanoid) for race-related rules and the host's human prerequisites, while the native outsider identity, race, RaceId, scores and geniekin routes are kept; the identity is a class feature granted only by the trait, so a committed native respec to the standard ancestry removes it; L01 families reload |
| Selected-power target manifests | NATIVE TESTED | `docs/FAVORED-CLASS-TARGET-MANIFEST.md`; oracle and advanced runs |
| Lifecycle policy (L02–L10) | PARTIAL | L02 and L03 NATIVE TESTED on the final candidate; L06 and L07 remain partial (scenario families below) |
| Diagnostics | NATIVE TESTED | `[favored-class]` log events; host-state observer |
| Migration/recovery guidance | CODE COMPLETE (documented) | `docs/FAVORED-CLASS-COMPATIBILITY.md` |
| Icon dispositions | DOMAIN TESTED; NATIVE TESTED (visual census) | 184 consumers recorded (171 favored-class, 13 Mostly Human): 109 provider donors, 39 native semantic donors, 18 KMG family shares, 12 Mostly Human ancestry and parent-race shares and 6 hidden internal helpers; no null placeholder remains; the native visual census rendered all 162 leaves published under the default profile with their exact icons, and the four Mostly Human selectors in the actual creator |

## Scenario families (64)

`FAVORED-CLASS-COVERAGE.json` holds each family's status and evidence. Ladder
columns: CODE, DOMAIN, NATIVE, SAVE (QUALIFIED awaits the owner's review).

| ID | Family | Status | CODE | DOMAIN | NATIVE | SAVE | Evidence / note |
| --- | --- | --- | --- | --- | --- | --- | --- |
| H01 | Host DLL physically absent | NATIVE TESTED | yes | yes | yes | n/a | gunslinger-only isolated profile: host absent, KMG class/firearms/races load, no FCB UI |
| H02 | Host installed but disabled, unsupported or partially initialized | DOMAIN TESTED; NATIVE BLOCKED | yes | yes | no | n/a | AbsentDisabledAndUnloadedHostsAreInactive, ReadinessRequiresCompleteInitializationAndGunslinger; a disabled or partial host cannot be staged without altering the host or UMM settings |
| H03 | Class-before-host / registration-after-host order | NATIVE TESTED | yes | yes | yes | n/a | contract and host-state: one Gunslinger favored progression and one reward per qualifying level |
| H04 | Gunslinger absent from host map | DOMAIN TESTED; NATIVE BLOCKED | yes | yes | no | n/a | ReadinessRequiresCompleteInitializationAndGunslinger; removing the Gunslinger from the host map would alter the host |
| H05 | Same version string, changed hash/MVID/shape | DOMAIN TESTED; NATIVE BLOCKED | yes | yes | no | n/a | SameVersionDifferentBinaryIsRejected; a changed host binary is never staged (charter: never modify host binaries) |
| H06 | Repeat registration or readiness twice | NATIVE TESTED | yes | yes | yes | n/a | contract: repeat publication is a no-op; stable GUID sets, order and component counts |
| H07 | Missing optional race or power | NATIVE TESTED | yes | yes | yes | n/a | AbsentProvidersRemoveOnlyTheirRoute; G08/G20 routes absent natively (no playable Goblin/Orc) |
| H08 | Exception midway through publication | NATIVE TESTED | yes | yes | yes | n/a | contract: injected fault restores the owned arrays exactly; foreign entries unchanged |
| H09 | Preexisting custom JSON rewards | NATIVE TESTED | yes | yes | yes | n/a | contract/host-state: host JSON rewards preserved; no foreign file writes |
| H10 | Host generic HP/skill and prestige behavior | NATIVE TESTED | yes | yes | yes | n/a | grit/menus: host HP/skill rewards unchanged; no second automatic-HP patch |
| E01 | Each of the 21 source-addressable race identities | NATIVE TESTED | yes | yes | yes | n/a | menus race matrix; explicit provider absence for Goblin and Orc |
| E02 | Half-elf human/elf access | SAVE TESTED | yes | yes | yes | yes | menus; respec Half-elf source; L01 |
| E03 | Half-orc human/orc access | NATIVE TESTED | yes | yes | yes | n/a | menus: human grit and native Pistol-Whip |
| E04 | Aasimar/Tiefling | NATIVE TESTED | yes | yes | yes | n/a | menus; Mostly Human bridge (Aasimar keeps host policy) |
| E05 | Four geniekin, Mostly Human OFF/ON | SAVE TESTED | yes | yes | yes | yes | Mostly Human lane; settings profile OFF run; L01 families |
| E06 | All twelve elemental heritages | NATIVE TESTED | yes | yes | yes | n/a | Mostly Human heritages; elemental core/advanced |
| E07 | Mostly Human dual identity | SAVE TESTED | yes | yes | yes | yes | Mostly Human dual identity and bridge; lifecycle; L01 families |
| E08 | Visual race/body changes only | NATIVE TESTED | yes | yes | yes | n/a | ancestry from race GUID only (RaceIdentitiesAreExactAndComplete); the host bridge stays closed to a human-looking standard geniekin natively |
| E09 | Verified Racial/Planar Heritage provider | OUT OF SCOPE | n/a | n/a | n/a | n/a | OUT OF SCOPE: no qualified provider installed |
| E10 | Unknown provider, permission cycle, duplicate facts | PARTIAL (native) | yes | yes | partial | n/a | UnknownCyclicAndDuplicateEvidenceIsBounded; Races Unleashed Suli Mostly Human stays closed natively |
| E11 | Ordinary and Half-elf multiclass progressions | NATIVE TESTED | yes | n/a | yes | n/a | disposable-favored-class-multiclass |
| E12 | Future versus permanently replaced feature | NATIVE TESTED | yes | yes | yes | n/a | ImprovedFeatureRuleExcludesOnlyFullReplacement; advanced/menus archetype exclusions |
| E13 | Cancel/backtrack/reopen level-up or respec | NATIVE TESTED | yes | n/a | yes | n/a | respec cancel; lifecycle pet respec cancel; grit cancel |
| E14 | Empty child set / capped last target | NATIVE TESTED | yes | yes | yes | n/a | menus: capped counters close; host HP/skill fallback remains |
| E15 | Mercenary, companion, auto-level and fresh character | PARTIAL (native) | yes | n/a | partial | n/a | custom-companion respec and fresh characters natively; pets never get favored-class progressions; auto-level not exercised |
| E16 | Ancestry change with existing fractions | NATIVE TESTED | yes | n/a | yes | n/a | respec Half-elf -> Human -> Dwarf; Mostly Human -> Standard |
| M01 | Every supported divisor at N = 0..20 | NATIVE TESTED | yes | yes | yes | n/a | rank policy; grit 20 levels; menus |
| M02 | Capped and uncapped partial capacities | NATIVE TESTED | yes | yes | yes | n/a | UncappedDivisorSixDoesNotStopAtEighteen; menus caps |
| M03 | Switch targets between partial investments | NATIVE TESTED | yes | yes | yes | n/a | oracle/performance per-target ledgers |
| M04 | Duplicate ancestry source routes | NATIVE TESTED | yes | yes | yes | n/a | AliasRoutesNeverDuplicateOrMultiply; menus one counter |
| M05 | Grit with normal and attribute-replacing archetypes | NATIVE TESTED | yes | yes | yes | n/a | grit lane |
| M06 | Grit maximum increase/decrease while spent | SAVE TESTED | yes | yes | yes | yes | fcb-grit-no-refill; L01 spent grit |
| M07 | Misfire across types, ammo, conditions and reliability | PARTIAL (native) | yes | yes | partial | n/a | mechanics: per-type threshold and floor; not every ammo/condition combination |
| M08 | Confirmation below/equal/above Critical Focus | NATIVE TESTED | yes | yes | yes | n/a | mechanics; ConfirmationPreservesTheBetterBonus |
| M09 | Attack categories | NATIVE TESTED | yes | yes | yes | n/a | mechanics: firearm/unarmed/other categories |
| M10 | Pistol-Whip and deed interruption/custom path | PARTIAL (native) | yes | yes | partial | n/a | mechanics: Pistol-Whip surrogate attack only; interruption path not exercised |
| M11 | Nimble armor and loss-of-Dexterity | SAVE TESTED | yes | yes | yes | yes | mechanics; lifecycle; L01 Nimble |
| M12 | Dodge timing in both combat modes | PARTIAL (native) | yes | yes | partial | n/a | mechanics: deed buff; turn-based timing not separately exercised |
| M13 | Initiative at zero/nonzero grit and True Grit | PARTIAL (native) | yes | yes | partial | n/a | initiative timing in real-time and turn-based entry; True Grit not exercised |
| M14 | Bomb hit/splash/critical/converted damage | PARTIAL (native) | yes | yes | partial | n/a | elemental core: flat bonus on a direct hit; splash/critical not exercised |
| M15 | Two Bards with different range investments | SAVE TESTED | yes | yes | yes | yes | performance range; lifecycle membership; L01 bard |
| M16 | Performance selected target and cap | NATIVE TESTED | yes | yes | yes | n/a | performance range: per-target caps, owner text, ring; exclusions |
| M17 | Two paladins, aura overlap/unlock/replacement | SAVE TESTED | yes | yes | yes | yes | lifecycle aura overlap; L01 paladin |
| M18 | Pet replacement/resummon/death and Barkskin | SAVE TESTED | yes | yes | yes | yes | advanced; lifecycle; respec pet; L01 replacement |
| M19 | CMD versus CMB and maneuver subtype | NATIVE TESTED | yes | yes | yes | n/a | elemental core |
| M20 | Undine Monk at N = 1,2,3,20 | SAVE TESTED | yes | yes | yes | yes | UndineMonkMixedRateBundle; elemental core; L01 monk |
| M21 | Intimidate/demoralize scope | NATIVE TESTED | yes | yes | yes | n/a | elemental core |
| M22 | Actual subtype versus geniekin race | NATIVE TESTED | yes | yes | yes | n/a | elemental core |
| M23 | Undine SR checks | NATIVE TESTED | yes | yes | yes | n/a | elemental core |
| M24 | Each supported selected-power manifest entry | PARTIAL (native) | yes | yes | partial | yes | oracle: all 52 scoped, values for Fire Breath and neighbors; advanced: four bloodline powers; L01 revelation and bloodline arithmetic |
| M25 | Neighboring powers and spellbook unchanged | NATIVE TESTED | yes | yes | yes | n/a | oracle/advanced neighbors and Fireball unchanged |
| M26 | Power at unlock boundary and above 20 | NATIVE TESTED | yes | yes | yes | n/a | oracle thresholds; FCB virtual levels never unlock a power |
| M27 | No-scaling power or unsupported branch | DOMAIN TESTED (native n/a) | yes | yes | n/a | n/a | manifest dispositions (not offered) |
| M28 | Separate 1/3, 1/4, 1/6 racial variants | NATIVE TESTED | yes | yes | yes | n/a | DistinctVariantsKeepTheirOwnRatesAndProfiles; menus |
| L01 | Fresh-process save/reload | SAVE TESTED | yes | n/a | yes | yes | Word of Recall FCB transaction: one subject per state/mechanic family, including a selected firearm target with native shots after the reload; the own persistence cases of G05, G06 (Dodge), G11, G17, G21, I01, I05, I07, O04, O05, O08, U02, S04 and S06 are NOT RUN |
| L02 | Death/resurrection, area transition, polymorph/return | NATIVE TESTED | yes | n/a | yes | n/a | working-save-favored-class-lifecycle |
| L03 | Full respec commit and cancellation | NATIVE TESTED | yes | n/a | yes | n/a | respec lane (Gunslinger, Mostly Human, Sorcerer); lifecycle pet respec |
| L04 | Module OFF/ON across restarts | NATIVE TESTED | yes | yes | yes | n/a | settings profiles integration OFF/ON |
| L05 | Third-party profile OFF with earned choices | NATIVE TESTED | yes | yes | yes | n/a | third-party ON/OFF menus and mechanics |
| L06 | Missing external dependency in a saved build | CODE COMPLETE; NATIVE BLOCKED | yes | partial | no | no | documented recovery; identity retention domain tested; not staged (the mission forbids loading a host-dependent campaign with its dependency removed) |
| L07 | Real-time-with-pause and turn-based modes | PARTIAL (native) | yes | n/a | partial | n/a | initiative both modes; other families real-time |
| L08 | Unity serialization and release GUID manifest | SAVE TESTED | yes | yes | yes | yes | manifest tests; the L01 fresh-process reload deserializes the components of every reloaded family subject |
| L09 | Final package versus loaded DLL | NATIVE TESTED | yes | yes | yes | n/a | every guarded run checks commit, package, DLL and MVID |
| L10 | Unsupported level-cap/gestalt/respec/provider | DOMAIN TESTED (native n/a) | yes | yes | n/a | n/a | explicit scope statement; no broad compatibility claim |
