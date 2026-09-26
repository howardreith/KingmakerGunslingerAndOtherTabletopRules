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

Candidate `12651613c`: package `95d57d55...`, DLL `ee0a67e4...`, MVID `62badef3-342b-409a-9733-20dc6a664578`. Every run used the guarded
Steam App ID 640820 launcher against that one deployment.

| Scenario | Run | Result |
| --- | --- | --- |
| observe-favored-class-contract | 20260926T0424285521034Z-observe-favored-class-contract | PASS |
| observe-favored-class-host-state | 20260926T0425098817176Z-observe-favored-class-host-state | PASS |
| observe-favored-class-host-defects | 20260926T0425504713383Z-observe-favored-class-host-defects | PASS |
| disposable-favored-class-elemental-core | 20260926T0426304190835Z-disposable-favored-class-elemental-core | PASS |
| disposable-favored-class-elemental-advanced | 20260926T0427143130346Z-disposable-favored-class-elemental-advanced | PASS |
| disposable-favored-class-oracle-revelations | 20260926T0427587131736Z-disposable-favored-class-oracle-revelations | PASS |
| disposable-favored-class-performance-range | 20260926T0429246463836Z-disposable-favored-class-performance-range | PASS |
| working-save-favored-class-lifecycle@KMG_AUTOMATION_WORKING | 20260926T0430061268709Z-working-save-favored-class-lifecycle | PASS |
| disposable-favored-class-respec | 20260926T0431104013973Z-disposable-favored-class-respec | PASS |
| working-save-favored-class-visual-census@KMG_AUTOMATION_WORKING | 20260926T0431532259029Z-working-save-favored-class-visual-census | PASS |
| disposable-favored-class-grit | 20260926T0433443953898Z-disposable-favored-class-grit | PASS |
| disposable-favored-class-gunslinger-menus | 20260926T0434326926830Z-disposable-favored-class-gunslinger-menus | PASS |
| disposable-favored-class-gunslinger-mechanics | 20260926T0435266732898Z-disposable-favored-class-gunslinger-mechanics | PASS |
| disposable-favored-class-initiative-timing | 20260926T0436075638631Z-disposable-favored-class-initiative-timing | PASS |
| disposable-favored-class-turn-modes | 20260926T0436472719055Z-disposable-favored-class-turn-modes | PASS |
| disposable-favored-class-bombs | 20260926T0437279321541Z-disposable-favored-class-bombs | PASS |
| disposable-favored-class-auto-level | 20260926T0438084070076Z-disposable-favored-class-auto-level | PASS |
| disposable-favored-class-mostly-human | 20260926T0438487161591Z-disposable-favored-class-mostly-human | PASS |
| observe-favored-class-performance-visuals | 20260926T0439381382013Z-observe-favored-class-performance-visuals | PASS |
| disposable-favored-class-multiclass | 20260926T0440198691989Z-disposable-favored-class-multiclass | PASS |
| working-save-elemental-character-creation-regression@KMG_AUTOMATION_WORKING#class=Fighter;allocation=point-buy;nativeActionCase=racial-actions;race=Ifrit | 20260926T0441000207225Z-working-save-elemental-character-creation-regression | PASS |
| working-save-elemental-character-creation-regression@KMG_AUTOMATION_WORKING#class=Fighter;allocation=point-buy;nativeActionCase=racial-actions;race=Oread | 20260926T0446152587603Z-working-save-elemental-character-creation-regression | PASS |
| working-save-elemental-character-creation-regression@KMG_AUTOMATION_WORKING#class=Fighter;allocation=point-buy;nativeActionCase=racial-actions;race=Sylph | 20260926T0451067541490Z-working-save-elemental-character-creation-regression | PASS |
| working-save-elemental-character-creation-regression@KMG_AUTOMATION_WORKING#class=Fighter;allocation=point-buy;nativeActionCase=racial-actions;race=Undine | 20260926T0456110533371Z-working-save-elemental-character-creation-regression | PASS |
| working-save-elemental-native-respec@KMG_AUTOMATION_WORKING#class=Fighter;allocation=point-buy;race=Sylph | 20260926T0500236878668Z-working-save-elemental-native-respec | PASS |
| working-save-elemental-native-respec@KMG_AUTOMATION_WORKING#class=Fighter;allocation=point-buy;race=Oread | 20260926T0507046815679Z-working-save-elemental-native-respec | PASS |
| working-save-smoke@KMG_AUTOMATION_WORKING | 20260926T0514062685374Z-working-save-smoke | PASS |
| L01 fresh-process persistence (prepare) | 20260926T0515338811043Z-disposable-word-of-recall-favored-class-persistence | PASS |
| L01 fresh-process persistence (verify) | 20260926T0516336684454Z-disposable-word-of-recall-favored-class-persistence | PASS |
| observe-favored-class-contract | 20260926T0518031115585Z-observe-favored-class-contract | PASS |
| disposable-favored-class-gunslinger-menus | 20260926T0518432350673Z-disposable-favored-class-gunslinger-menus | PASS |
| observe-favored-class-contract | 20260926T0519366390443Z-observe-favored-class-contract | PASS |
| observe-favored-class-host-state | 20260926T0520171147411Z-observe-favored-class-host-state | PASS |
| observe-favored-class-contract | 20260926T0520572501435Z-observe-favored-class-contract | PASS |
| observe-favored-class-host-state | 20260926T0522081723639Z-observe-favored-class-host-state | PASS |
| L06 missing dependency (prepare) | 20260926T0523196350139Z-disposable-word-of-recall-favored-class-persistence | PASS |
| L06 missing dependency (absent host) | 20260926T0524188474485Z-observe-favored-class-missing-dependency | PASS |
| profile gunslinger-only / observe-favored-class-host-state | 20260926T0528022419214Z-observe-favored-class-host-state | PASS |
| profile gunslinger-call-of-the-wild / observe-favored-class-host-state | 20260926T0531183716191Z-observe-favored-class-host-state | PASS |
| profile gunslinger-call-of-the-wild-favored-class / observe-favored-class-host-state | 20260926T0534505046553Z-observe-favored-class-host-state | PASS |

## Scheduled rows (30)

The row evidence names the run that exercised the row in detail. The final
candidate re-ran every scenario listed above.

| Row | Ancestry → class | Disp. | Canonical effect | Rate / cap | Status | Evidence |
| --- | --- | --- | --- | --- | --- | --- |
| G01 | Dwarf → Gunslinger | I | gunslinger.misfire-by-firearm-type (per type) | 1/4, floor 1 | NATIVE TESTED; SAVE TESTED (pistol target) | menus: three independent per-type counters; mechanics: effective threshold reduced last with floor 1, other types unchanged; L01 families: a Dwarf's pistol misfire step reloads, the production path still reads Pistol 1, Musket 0 and Blunderbuss 0, and seeded native shots after the reload misfire only as that step allows (control, floor and musket unchanged) |
| G02 | Elf → Gunslinger | I | gunslinger.firearm-confirmation | 1/3, cap +5, nonstacking CF | SAVE TESTED (shared counter) | menus: capped at (5,10) then closed; mechanics: better of Critical Focus; L01 `20260924T0352059750938Z`/`20260924T0353099892035Z` |
| G04 | Half-elf → Gunslinger | I | gunslinger.grit (shared with G07) | 1/4 | SAVE TESTED | L01 transaction `word-of-recall-fcb-persistence-20260924T0352021320346Z_ba16f881ae444ef8985ad6eb6d178671` |
| G05 | Half-orc → Gunslinger | I | gunslinger.pistol-whip-attack | 1/3 | NATIVE TESTED; SAVE TESTED | menus (6,14); mechanics: only the Pistol-Whip surrogate attack; L01 rows: after the fresh-process reload the counter moves the Pistol-Whip deed attack's attack bonus (neighbor: an ordinary pistol shot) by exactly its recorded steps |
| G06 | Halfling → Gunslinger | I | nimble-halfling + dodge-halfling (two counters) | 1/4 cap +2; 1/4 | NATIVE TESTED; SAVE TESTED (Nimble, Dodge) | menus: Nimble capped then Dodge; Mysterious Stranger and Musket Master exclusions; mechanics: genuine Nimble and deed buff only; L01 families: a Halfling Nimble step reloads its dodge modifier; L01 rows: after the fresh-process reload the counter moves AC while the Gunslinger's Dodge buff is active (neighbor: AC without the buff) by exactly its recorded steps |
| G07 | Human → Gunslinger | I | gunslinger.grit | 1/4 | NATIVE TESTED; SAVE TESTED (shared counter) | grit: 20 native level-ups, no refill, cancel leaves no leak |
| G08 | Goblin → Gunslinger | I | gunslinger.firearm-confirmation | 1/3, cap +5 | CODE COMPLETE; route EXPECTED PROVIDER ABSENCE | the host goblin race is not in the playable catalog; the counter is G02's |
| G10 | Hobgoblin → Gunslinger | I | gunslinger.grit | 1/4 | NATIVE TESTED (menu route) | race matrix (Races Unleashed Hobgoblin) |
| G11 | Ifrit → Gunslinger | I | gunslinger.initiative-deed | 1/2 | NATIVE TESTED; SAVE TESTED | initiative timing in real-time and turn-based entry; D3 fixed in `732cddae3`; L01 rows: after the fresh-process reload the counter moves the Initiative deed's bonus on a native initiative roll with grit (neighbor: the Initiative stat) by exactly its recorded steps |
| G14 | Fetchling → Gunslinger | I | gunslinger.grit | 1/4 | NATIVE TESTED (menu route) | race matrix (Races Unleashed Fetchling) |
| G16 | Dhampir → Gunslinger (JBE) | O | gunslinger.firearm-confirmation | 1/3, cap +5 | NATIVE TESTED (profile ON) | third-party menus `20260924T0538189215967Z` (`9b3db78bf`); withheld by default; final candidate: third-party ON menus `20260924T2313502108487Z` (the profile ON publishes 166 leaves) |
| G17 | Drow → Gunslinger (JBE) | O | gunslinger.nimble-drow | 1/6, cap +2 | NATIVE TESTED; SAVE TESTED | profile-ON menus; mechanics under the default profile (L05); final candidate: third-party ON menus `20260924T2313502108487Z` (the profile ON publishes 166 leaves); L01 rows: after the fresh-process reload the counter moves Nimble AC (neighbor: flat-footed AC) by exactly its recorded steps |
| G18 | Duergar → Gunslinger (JBE) | O | gunslinger.misfire-by-firearm-type | 1/4, floor 1 | NATIVE TESTED (profile ON) | Duergar offered the per-type misfire counters; final candidate: third-party ON menus `20260924T2313502108487Z` (the profile ON publishes 166 leaves) |
| G20 | Orc → Gunslinger (JBE) | O | gunslinger.pistol-whip-attack (alias route) | 1/3 | CODE COMPLETE; route EXPECTED PROVIDER ABSENCE | no playable Orc race; Half-orcs hold the same G05 counter |
| G21 | Tiefling → Gunslinger (JBE) | O | gunslinger.cmb-dirty-trick-trip | 1/2 | NATIVE TESTED; SAVE TESTED | profile-ON menus; mechanics: trip and dirty tricks only; final candidate: third-party ON menus `20260924T2313502108487Z` (the profile ON publishes 166 leaves); L01 rows: after the fresh-process reload the counter moves trip CMB (neighbor: bull rush CMB) by exactly its recorded steps |
| I01 | Ifrit → Alchemist | I | alchemist.bomb-damage | 1/2 | NATIVE TESTED; SAVE TESTED | elemental core: flat bonus on a 5d1 bomb, above the damage floor; Vivisectionist excluded; L01 rows: after the fresh-process reload the counter moves a bomb's flat damage bonus (neighbor: Fireball) by exactly its recorded steps |
| I05 | Ifrit → Inquisitor | A | inquisitor.intimidate-fire-subtype (planes knowledge omitted) | 1/2 | NATIVE TESTED; SAVE TESTED | native Demoralize against a fire-subtype target only; L01 rows: after the fresh-process reload the counter moves a demoralize check against a fire creature (neighbor: a creature without the fire subtype) by exactly its recorded steps |
| I06 | Ifrit → Oracle | I | oracle.selected-revelation (66 revelation targets; 65 published, Spirit of the Warrior excluded) | 1/6 per revelation | NATIVE TESTED; SAVE TESTED (Fire Breath) | oracle run: all 66 scoped with every audited family; every published revelation's values equal a native oracle at the effective level; the Ifrit menu offers exactly the owned revelation; Fire Breath at level 9 with two steps: CL 11, DC +1, dice 11, uses 3; neighbors and Fireball unchanged; excluded or withheld counters are inert (a held Spirit of the Warrior rank changes nothing); own level gates follow the effective level (83 moved natively); feature context refreshes on gain and removal; L01 families: the Ifrit Fire Breath counter reloads with its caster level, DC, dice and uses; the census renders every revelation leaf |
| I07 | Ifrit → Rogue | A | rogue.demoralize (jump omitted) | 1/2 | NATIVE TESTED; SAVE TESTED | the native Demoralize check only; a failure inside a demoralize never reaches the next check (finally-closed envelope); L01 rows: after the fresh-process reload the counter moves a demoralize check (neighbor: an Intimidate check that is not a demoralize) by exactly its recorded steps |
| I08 | Ifrit → Sorcerer | I | sorcerer.selected-bloodline-power (fire: Ray, Blast, Resistance) | 1/6, cap +2 | NATIVE TESTED; SAVE TESTED (Fire Ray); RESPEC TESTED | advanced run: Blast CL 9→11, DC 14→15, dice 9→11; Blast's extra uses at 17 and 20 follow the effective level; Ray damage bonus 5→6; neighbor and Fireball unchanged; Elemental Resistance's gates equal a native sorcerer at the capped effective level (resistance 20 two levels early); L01 families: the Fire Ray arithmetic reloads; respec: an Ifrit Sorcerer 6 respecced to Sorcerer 1 keeps no counter and native level-1 arithmetic |
| O01 | Oread → Bard | I | bard.selected-performance-range (14 published performances) | +5 ft, cap +30 ft per performance | NATIVE TESTED; SAVE TESTED | performance run: own Inspire Courage area 50→60 ft, 80 ft at the cap; a 55-ft point inside only when widened; other performance, other performer and blueprint unchanged; Human and Archaeologist not offered; redesign: the area, its ring and the performance's description all show the owner's range, for that bard only; the descriptions follow the owner's live areas of the performance, which widen only as a whole (native while any live area is native, their actual range while all are widened, the configured range once none is live; no outcome is remembered); a widened area whose rollback cannot be verified is ended and the toggle whose own buff runs it is turned off (lifecycle `fcb-lifecycle-toggle-rollback`); Storm Call and Mockery excluded; lifecycle: membership, movement, interruption, death and area reload; L01 families: the widened area and its ring reload |
| O04 | Oread → Fighter | A | fighter.cmd-bull-rush-drag (drag omitted) | +1 | NATIVE TESTED; SAVE TESTED | bull rush CMD only; L01 rows: after the fresh-process reload the counter moves CMD against bull rush (neighbor: CMD against trip) by exactly its recorded steps |
| O05 | Oread → Monk | I | monk.unarmed-confirmation | 1/3, cap +5 | NATIVE TESTED; SAVE TESTED | unarmed strikes only (not claws or weapons); better of Critical Focus; L01 rows: after the fresh-process reload the counter moves an unarmed strike's critical confirmation bonus (neighbor: a longsword) by exactly its recorded steps |
| O06 | Oread → Paladin | I | paladin.aura-ally-bonus (Courage, Resolve) | 1/4 | NATIVE TESTED; SAVE TESTED | advanced run: fear +4/+5/+6 and charm +4/+6 by each paladin's own steps; other saves unchanged; Divine Hunter not offered; lifecycle: two overlapping paladins follow the native Replace and Morale rules (never a sum, no stacking with Remove Fear); L01 families: the reloaded aura still gives +6 |
| O07 | Oread → Ranger | I | ranger.companion-natural-armor | 1/4 | NATIVE TESTED; SAVE TESTED; RESPEC TESTED | +2 then +3 on the current companion; stacks with Barkskin; touch and master AC unchanged; replacement leaves no orphan; removal clears it; lifecycle: death, polymorph, reload; respec: the old companion is destroyed without an orphaned projection and the new one receives it once; transitions: unlink, relink, qualification loss, dismissal and resummoning keep exactly one projection on the qualified pet; L01 families: reload, replacement and unlink after reload |
| O08 | Oread → Summoner | I | summoner.eidolon-natural-armor | 1/4 | NATIVE TESTED; SAVE TESTED | +2 on Call of the Wild's eidolon; L01 rows: after the fresh-process reload the counter moves the eidolon's AC (neighbor: its touch AC) by exactly its recorded steps |
| S04 | Sylph → Oracle | I | oracle.selected-revelation (shared with I06) | 1/6 | NATIVE TESTED; SAVE TESTED | oracle run: a Sylph Wind Oracle is offered exactly Lightning Breath; L01 rows: after the fresh-process reload the counter moves Lightning Breath's caster level (neighbor: the Air Ray) by exactly its recorded steps |
| S06 | Sylph → Sorcerer | I | sorcerer.selected-bloodline-power (air: Ray, Blast, Resistance) | 1/6, cap +2 | NATIVE TESTED; SAVE TESTED | the Sylph air menu offers AirRay only; Ifrit with air gets none; advanced run: the Air Blast and Air Ray values, Blast's extra uses at 17 and 20 and Elemental Resistance's gates at the effective level natively; L01 rows: after the fresh-process reload the counter moves the Air Ray's caster level (neighbor: Lightning Breath) by exactly its recorded steps |
| U02 | Undine → Cleric | I | cleric.sr-penetration-aquatic-water | +1 | NATIVE TESTED; SAVE TESTED | water and aquatic targets only; L01 rows: after the fresh-process reload the counter moves spell penetration against a water creature (neighbor: another creature) by exactly its recorded steps |
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
| Optional host adapter (exact binary + readiness) | NATIVE TESTED (present; absent; disabled; simulated defects) | contract and host-state on the final candidate; the isolated `gunslinger-only` (host absent), `gunslinger-call-of-the-wild` and `gunslinger-call-of-the-wild-favored-class` profiles on the final candidate (report section 4) |
| Native host progression | NATIVE TESTED | one Gunslinger progression and bonus selection, 20 levels |
| Atomic, idempotent publication with owned rollback | NATIVE TESTED | contract: owned leaves appended as suffixes; repeat is a no-op; an injected fault is restored exactly |
| Fresh-process persistence (L01) | SAVE TESTED (one subject per state/mechanic family and every remaining row) | Word of Recall FCB transaction on the final candidate: the families subjects and the own case of G05, G06 (Dodge), G11, G17, G21, I01, I05, I07, O04, O05, O08, U02, S04 and S06 (report section 4) |
| Configuration profiles (restart-required settings file) | NATIVE TESTED | final candidate: third-party ON (contract and menus), Mostly Human OFF, integration OFF (mechanics suppressed, identities registered) and an invalid file (defaults, reported); the file was removed and verified absent after each profile (report section 4) |
| Scoped ancestry bridge (host human prerequisites) | NATIVE TESTED | Mostly Human run: 57 tracked prerequisites, open for Mostly Human and closed for Standard; Human, Half-elf and Aasimar true; Dwarf false; other race checks unchanged |
| Mostly Human companion racial trait (four parents) | NATIVE TESTED; SAVE TESTED; RESPEC TESTED | a genuine dual identity: human (humanoid) for race-related rules and the host's human prerequisites, while the native outsider identity, race, RaceId, scores and geniekin routes are kept; the identity is a class feature granted only by the trait, so a committed native respec to the standard ancestry removes it; L01 families reload |
| Selected-power target manifests | NATIVE TESTED | `docs/FAVORED-CLASS-TARGET-MANIFEST.md`; oracle and advanced runs |
| Lifecycle policy (L02–L10) | NATIVE TESTED | L02, L03 and L07 native, L06 save tested (missing-dependency transaction), L04/L05 profiles, L08/L09 manifests and identities (scenario families below) |
| Diagnostics | NATIVE TESTED | `[favored-class]` log events; host-state observer |
| Migration/recovery guidance | NATIVE TESTED | `docs/FAVORED-CLASS-COMPATIBILITY.md`; the L06 warning names the missing host and its state before the native load error (missing-dependency transaction) |
| Icon dispositions | DOMAIN TESTED; NATIVE TESTED (visual census) | 216 consumers recorded (203 favored-class, 13 Mostly Human); no null placeholder remains; the native visual census rendered all 192 leaves published under the default profile with their exact icons, and the four Mostly Human selectors in the actual creator |

## Scenario families (64)

`FAVORED-CLASS-COVERAGE.json` holds each family's status and evidence. Ladder
columns: CODE, DOMAIN, NATIVE, SAVE (QUALIFIED awaits the owner's review).
NATIVE TESTED (simulated): the production gates and planner ran in the game on
the live host's own observations, cloned with one fact changed, because the
defective host itself is not staged (owner-authorized for H04 and H05).

| ID | Family | Status | CODE | DOMAIN | NATIVE | SAVE | Evidence / note |
| --- | --- | --- | --- | --- | --- | --- | --- |
| H01 | Host DLL physically absent | NATIVE TESTED | yes | yes | yes | n/a | gunslinger-only isolated profile: host absent, KMG class/firearms/races load, no FCB UI |
| H02 | Host installed but disabled, unsupported or partially initialized | NATIVE TESTED | yes | yes | yes | n/a | disabled host: the host-state lane in a byte-exact Params.xml stage (HostDisabled, every owned identity registered, nothing published); unsupported and partially initialized: the host-defects lane on cloned live observations (UnsupportedBinary, HostIncomplete, nothing planned) |
| H03 | Class-before-host / registration-after-host order | NATIVE TESTED | yes | yes | yes | n/a | contract and host-state: one Gunslinger favored progression and one reward per qualifying level |
| H04 | Gunslinger absent from host map | NATIVE TESTED (simulated) | yes | yes | yes | n/a | host-defects lane: a host without a Gunslinger entry (not scanned, not offered, wrong shape) blocks exactly the Gunslinger rows, touches no Gunslinger selection and plans every other family as live; the host's global state is unchanged (no second Core.load or rebuild) |
| H05 | Same version string, changed hash/MVID/shape | NATIVE TESTED (simulated) | yes | yes | yes | n/a | host-defects lane: the same version label with a changed SHA-256, MVID, Core.load body or required member, another version or a changed dependency is UnsupportedBinary with its own reason and cannot plan; the label alone decides nothing |
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
| E10 | Unknown provider, permission cycle, duplicate facts | NATIVE TESTED | yes | yes | yes | n/a | gunslinger-menus permission bounds on real level-1 visits: an unconditional cycle, the depth bound of four, an unverified fact, a duplicated Mostly Human identity and a stray racial fact; Races Unleashed Suli Mostly Human stays closed; UnknownCyclicAndDuplicateEvidenceIsBounded |
| E11 | Ordinary and Half-elf multiclass progressions | NATIVE TESTED | yes | n/a | yes | n/a | disposable-favored-class-multiclass |
| E12 | Future versus permanently replaced feature | NATIVE TESTED | yes | yes | yes | n/a | ImprovedFeatureRuleExcludesOnlyFullReplacement; advanced/menus archetype exclusions |
| E13 | Cancel/backtrack/reopen level-up or respec | NATIVE TESTED | yes | n/a | yes | n/a | respec cancel; lifecycle pet respec cancel; grit cancel |
| E14 | Empty child set / capped last target | NATIVE TESTED | yes | yes | yes | n/a | menus: capped counters close; host HP/skill fallback remains |
| E15 | Mercenary, companion, auto-level and fresh character | NATIVE TESTED | yes | yes | yes | n/a | auto-level lane: a recorded level applied by the native constructor keeps its same-level reward (D5 fixed) while the unscoped native calls drop it; a host-only plan and Linzi's stored plan are identical either way; the Gunslinger has no default build; an NPC summon has no plan or favored class; custom-companion respec, fresh characters and pets as before |
| E16 | Ancestry change with existing fractions | NATIVE TESTED | yes | n/a | yes | n/a | respec Half-elf -> Human -> Dwarf; Mostly Human -> Standard |
| M01 | Every supported divisor at N = 0..20 | NATIVE TESTED | yes | yes | yes | n/a | rank policy; grit 20 levels; menus |
| M02 | Capped and uncapped partial capacities | NATIVE TESTED | yes | yes | yes | n/a | UncappedDivisorSixDoesNotStopAtEighteen; menus caps |
| M03 | Switch targets between partial investments | NATIVE TESTED | yes | yes | yes | n/a | oracle/performance per-target ledgers |
| M04 | Duplicate ancestry source routes | NATIVE TESTED | yes | yes | yes | n/a | AliasRoutesNeverDuplicateOrMultiply; menus one counter |
| M05 | Grit with normal and attribute-replacing archetypes | NATIVE TESTED | yes | yes | yes | n/a | grit lane |
| M06 | Grit maximum increase/decrease while spent | SAVE TESTED | yes | yes | yes | yes | fcb-grit-no-refill; L01 spent grit |
| M07 | Misfire across types, ammo, conditions and reliability | NATIVE TESTED | yes | yes | yes | n/a | mechanics: a 31-case misfire matrix (types, both hands, ammunition, Broken with and without Gun Training, Reliable, Dead Shot and Scatter Shot) |
| M08 | Confirmation below/equal/above Critical Focus | NATIVE TESTED | yes | yes | yes | n/a | mechanics; ConfirmationPreservesTheBetterBonus |
| M09 | Attack categories | NATIVE TESTED | yes | yes | yes | n/a | mechanics: firearm/unarmed/other categories |
| M10 | Pistol-Whip and deed interruption/custom path | NATIVE TESTED | yes | yes | yes | n/a | mechanics: the surrogate attack and its refusals; turn-modes: an interrupted Pistol-Whip spends and attacks nothing, and the counter moves the attack bonus by exactly its steps and never the trip's CMB (scoped to the deed's own attack) |
| M11 | Nimble armor and loss-of-Dexterity | SAVE TESTED | yes | yes | yes | yes | mechanics; lifecycle; L01 Nimble |
| M12 | Dodge timing in both combat modes | NATIVE TESTED | yes | yes | yes | n/a | turn-modes: the real Dodge command in real time and turn-based: swift, one grit, +2 plus the Halfling steps, no movement, no second use while active, active through its round and gone by the next |
| M13 | Initiative at zero/nonzero grit and True Grit | NATIVE TESTED | yes | yes | yes | n/a | initiative timing and turn-modes: the deed with and without grit and with True Grit; True Grit Dodge refused at 0 grit and free at 1 |
| M14 | Bomb hit/splash/critical/converted damage | NATIVE TESTED | yes | yes | yes | n/a | bombs: production bombs thrown natively; the steps once per damaged creature on a direct hit, splash with failed and passed saves, the miss splash, a critical and converted variants; nothing for Breath Weapon Bomb or the acid bomb's lingering damage |
| M15 | Two Bards with different range investments | SAVE TESTED | yes | yes | yes | yes | performance range; lifecycle membership; L01 bard |
| M16 | Performance selected target and cap | NATIVE TESTED | yes | yes | yes | n/a | performance range: per-target caps, owner text, ring; exclusions |
| M17 | Two paladins, aura overlap/unlock/replacement | SAVE TESTED | yes | yes | yes | yes | lifecycle aura overlap; L01 paladin |
| M18 | Pet replacement/resummon/death and Barkskin | SAVE TESTED | yes | yes | yes | yes | advanced; lifecycle; respec pet; L01 replacement |
| M19 | CMD versus CMB and maneuver subtype | NATIVE TESTED | yes | yes | yes | n/a | elemental core |
| M20 | Undine Monk at N = 1,2,3,20 | SAVE TESTED | yes | yes | yes | yes | UndineMonkMixedRateBundle; elemental core; L01 monk |
| M21 | Intimidate/demoralize scope | NATIVE TESTED | yes | yes | yes | n/a | elemental core |
| M22 | Actual subtype versus geniekin race | NATIVE TESTED | yes | yes | yes | n/a | elemental core |
| M23 | Undine SR checks | NATIVE TESTED | yes | yes | yes | n/a | elemental core |
| M24 | Each supported selected-power manifest entry | SAVE TESTED | yes | yes | yes | yes | oracle: every published revelation (65) equals a native oracle at the effective level at every read point and gate; advanced: six bloodline powers including Elemental Resistance's own gates; L01 revelation and bloodline arithmetic |
| M25 | Neighboring powers and spellbook unchanged | NATIVE TESTED | yes | yes | yes | n/a | oracle/advanced neighbors and Fireball unchanged |
| M26 | Power at unlock boundary and above 20 | NATIVE TESTED | yes | yes | yes | n/a | oracle thresholds; FCB virtual levels never unlock a power |
| M27 | No-scaling power or unsupported branch | DOMAIN TESTED (native n/a) | yes | yes | n/a | n/a | manifest dispositions (not offered) |
| M28 | Separate 1/3, 1/4, 1/6 racial variants | NATIVE TESTED | yes | yes | yes | n/a | DistinctVariantsKeepTheirOwnRatesAndProfiles; menus |
| L01 | Fresh-process save/reload | SAVE TESTED | yes | n/a | yes | yes | Word of Recall FCB transaction: one subject per state/mechanic family, including a selected firearm target with native shots after the reload, and the own persistence case of every remaining scheduled row (G05, G06 Dodge, G11, G17, G21, I01, I05, I07, O04, O05, O08, U02, S04, S06) |
| L02 | Death/resurrection, area transition, polymorph/return | NATIVE TESTED | yes | n/a | yes | n/a | working-save-favored-class-lifecycle |
| L03 | Full respec commit and cancellation | NATIVE TESTED | yes | n/a | yes | n/a | respec lane (Gunslinger, Mostly Human, Sorcerer); lifecycle pet respec |
| L04 | Module OFF/ON across restarts | NATIVE TESTED | yes | yes | yes | n/a | settings profiles integration OFF/ON |
| L05 | Third-party profile OFF with earned choices | NATIVE TESTED | yes | yes | yes | n/a | third-party ON/OFF menus and mechanics |
| L06 | Missing external dependency in a saved build | SAVE TESTED | yes | yes | yes | yes | missing-dependency transaction: a fixture save made with the host is read in the disabled-host profile; the native converter throws on every missing reference (nothing substituted), the fixture and saves folder are unchanged, KMG's identities resolve and the precise warning precedes the native message; the fixture is deleted with hash proof |
| L07 | Real-time-with-pause and turn-based modes | NATIVE TESTED | yes | n/a | yes | n/a | turn-modes: initiative, Dodge, Pistol-Whip, demoralize and grit in real time and in a fresh turn-based combat |
| L08 | Unity serialization and release GUID manifest | SAVE TESTED | yes | yes | yes | yes | manifest tests; the L01 fresh-process reload deserializes the components of every reloaded family subject |
| L09 | Final package versus loaded DLL | NATIVE TESTED | yes | yes | yes | n/a | every guarded run checks commit, package, DLL and MVID |
| L10 | Unsupported level-cap/gestalt/respec/provider | DOMAIN TESTED (native n/a) | yes | yes | n/a | n/a | explicit scope statement; no broad compatibility claim |
