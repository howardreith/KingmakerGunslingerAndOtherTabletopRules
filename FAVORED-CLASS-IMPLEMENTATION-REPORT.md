# Favored Class Integration - Implementation Report

## PARTIAL - NOT RELEASE QUALIFIED

This pass addressed the six findings of the PR #24 review (section 10) on the
same branch. The final candidate `aa298650e` passed all 33 guarded native
runs of its qualification: the eight lanes the findings changed (contract and
bootstrap aggregate, host state, elemental core with the Demoralize envelope,
elemental advanced with the Blast use thresholds and the replay scope, Oracle
level gates, performance range transaction, lifecycle pet transitions and
respec); every other favored-class lane; the four elemental creator and two
elemental respec lanes; the fresh-process persistence transaction (prepare and
verify), which now also unlinks a reloaded companion; the guarded working-save
smoke; the four temporary settings profiles (five runs); and the three
isolated compatibility profiles. Every run loaded the same DLL (SHA-256 and
MVID in section 1) from the same deterministic package, and the owner's
install was restored and verified byte for byte afterwards (section 8).

The candidate remains PARTIAL - NOT RELEASE QUALIFIED. Nine charter scenario
families are only partly observed natively (E10, E15, M07, M10, M12, M13,
M14, M24, L07). Fourteen scheduled rows have no persistence case of their
own (G05, G06 Dodge, G11, G17, G21, I01, I05, I07, O04, O05, O08, U02, S04,
S06). Four host or dependency states cannot be staged under the mission's
rules: H02, H04 and H05 would require altering the host, its binary or UMM
settings, and L06 would require loading a host-dependent campaign with its
dependency removed. Fifteen selected-power targets whose level thresholds are
implementable are explicitly deferred because they need owned identities the
ledger does not contain (section 6). Sections 3, 6 and 9 name each of these
gaps exactly.

The branch was pushed to PR #24 (https://github.com/howardreith/KingmakerGunslingerAndOtherTabletopRules/pull/24) on the owner's instruction; nothing was
merged, tagged or published.

## 1. Identity

| Item | Value |
| --- | --- |
| Worktree | `C:\Dev\KingmakerGunslingerLab\worktrees\favored-class-integration` |
| Branch | `claude/favored-class-integration` (pushed to PR #24 on the owner's instruction; not merged) |
| Baseline | `996105ed9e72259a220be56e2a74e0cc18e5ef47` (master, 0.0.138 release record) |
| Final source commit | `aa298650ecbc7003ce4c4d0ff992984793362462` |
| Evidence and records commit | the commit that adds this report (on top of the source commit) |
| Package | `artifacts/local-runtime/0.0.139/KingmakerGunslinger-0.0.139-local-runtime.zip`, SHA-256 `9abc94549073461e966a9c6cc0a8e4093e337666eb4cf586269dbe5e6a2fc526` (identical to the favored-class-integration package; not committed) |
| Loaded DLL | `KingmakerGunslinger.dll` 0.0.139, SHA-256 `5edb5e424df14b58cfc460782f8b3d7c8f181950a0784b04cc21d77717a064cb`, MVID `ecc3e660-1295-4132-8019-c01b703561d2` |
| Deployment | `C:\Dev\KingmakerGunslingerLab\runtime-evidence\deployments\20260925T0441598369963Z\deployment.json` |
| Deterministic build | two clean Release builds of `aa298650e` each ran the 1,824-case domain suite (0 failures) and produced the identical package and DLL; strict standalone UMM package validation PASS |
| Domain suite | 1,824 deterministic cases, all PASS; repository and icon-catalog validation PASS |

Continuation commits (oldest first), after the first pass's records commit
`fee3f45ec` on the same branch (47 commits since the baseline up to the
continuation's candidate `15c37695c`, whose records commit `eed717046`
follows; 57 up to the final source commit `aa298650e`):

- `8a46506df72ce79903a1bff94a0968441d9adc3c` Repair Mostly Human identity, power eligibility, thresholds and icons
- `0138d70b65348d0d2f5ddf2755b70723a95d31d1` Add the O01 performance visual observation
- `e972713925a0d1cb2e54a6b41cc029e7e6b47446` Make O01 range, ring and text agree for each owner
- `08ab4d17c2f270e17bf9a6d7d53315d56de2b68e` Add E11 and respec lanes; fix eidolon and Mostly Human donors
- `b2cf0cf815be129cfaf7562a331b2db7a3fbe621` Add the working-save lifecycle lane; fix E11 and respec subjects
- `ff434c829bb5d35a95f858e7be6bcc725450a231` Add persistence families; custom-companion respec; lifecycle waits
- `1b2d4174dfe6f14cc4a5171afe0aa870c93e2ede` Keep the Mostly Human identity out of respec; fix lifecycle lanes
- `94a00352b3f5b31a3931176c713d7c15f5c8a7f2` Add the native visual census and L03 pet and power respec lanes
- `487dd28ef91ccfe64bd6c81b95be8f9969647781` Pin the registration of every favored-class runtime lane
- `aef882103c23894bbdf942d417240818cf32b279` Regenerate the O01 target manifest from the mechanics classification
- `85d305d53b1aac014ad05984759f3fe7dfe1ea14` Cover a cancelled respec of a Ranger with a projected companion (E13)
- `c6ad460298f7fbe892ea74891337e79b8918ee36` Census only the routes the active profile offers
- `7247451534313b65574eaab4a4d2f0d4803f9991` Hold KMG rows to exact icons and record host rows in the census
- `1ebd83fa90147fb1ca2272bc762c5f620d9d46c2` State the census row rule exactly
- `e3617e224881176d854e346812f122f4737ee2d6` Fix the lane mismatches the verification batch exposed
- `bd8f7c60c28ab6d62870e117756f5a77529f372b` Open the census where the screen routes the reward; fix the L03 subjects
- `77afdde6284cfd2a1af7c7acd573c2b41d1c5b9d` Resolve the bloodline as a progression; re-show the census reward
- `675b117b0934c354d892283136c4b17f36ab75f7` Track the rebuilt level-up state; take class choices before the reward
- `b1b93acad6f72705db6077da0c6504fae0ca05a3` Let a counter target what the same level-up gains
- `cacf3a002cf570ab130a1a898abd4d0d62d378d0` Pin the usable-power wiring as an owned target
- `15c37695c267522ce35cbc021168514d6fb058fc` Reload a selected firearm target in a fresh process

Review commits (PR #24), oldest first, after the records commit `eed717046`:

- `ae0f1dafd436b94ee5e63985f03223e82982f904` Gate owned effects on a committed host publication
- `f9a8e6cfbecd1c80d323e29a98c6b5ea61fe7600` Advance owned powers' own thresholds; exclude the possession BAB
- `078978a4c8e53710ef36de084027f324554bb1ba` Keep pet armor on one qualified desired pet
- `da07c4e07bbc2ea47b63310f33131301e2637654` Widen a performance only together with its ring
- `f4f5b9931b693e4429a3324a92d2ff960e73f627` Close the replay and demoralize scopes in finally blocks
- `719085d518d15c0590e38ba8b122be516d70876b` Count every bootstrap registry in the aggregate
- `7bb4a704085b143225a8067e39bd8f6b5263237b` Pin the live aggregate to its catalog-derived expectation
- `27176f424f4d4f6e794669367a26a2d01e8cdff2` Follow Blast's extra uses through Call of the Wild's gates
- `aa298650ecbc7003ce4c4d0ff992984793362462` Wait only for the pet-transition step's own ranger

## 2. Rows, targets and counts

- Catalog: 54 table appearances, 53 distinct options (46 Paizo, 7 Jon Brazer
  Enterprises), 22 canonical effects.
- Scheduled rows: 30. First-party faithful 22 (G01 G02 G04 G05 G06 G07 G08 G10
  G11 G14, I01 I06 I08, O01 O05 O06 O07 O08, S04 S06, U02 U04); adaptations 3
  (I05 I07 O04); optional third-party 5 (G16 G17 G18 G20 G21, profile OFF by
  default). Alias: I04 = G11 (never a second reward).
- Provider absences: G08 (Goblin) and G20 (Orc) are CODE COMPLETE with their
  routes EXPECTED PROVIDER ABSENCE. No playable Goblin or Orc race exists in
  the qualified profile; G08's counter is G02's firearm confirmation and G20's
  is G05's Pistol-Whip counter, both published and natively tested through
  their other routes. Neither route is ever registered as an empty
  (unrestricted) race array.
- Not published: 23 (provider-deferred G09 G12 G13 G22; engineering-deferred
  G03 G15 G19 S03 S08 U06 U07; excluded I02 I03 O02 O03 S01 S02 S05 S07 U01
  U03 U05 U08); none appears as a placeholder.
- Owned identities: 171 favored-class leaves and helpers registered on every
  load (host present or absent), plus 13 Mostly Human identities; identity
  ledger 2,140 (2,138 active, 2 reserved). The bootstrap's registered
  aggregate counts every registry including Mostly Human's: 2,085 registered =
  2,085 expected in the qualified profile.
- Targets (docs/FAVORED-CLASS-TARGET-MANIFEST.md): I06/S04 52 revelation
  targets, 51 published and scoped with every audited family and their own
  level gates, Spirit of the Warrior excluded (its possession sets base attack
  bonus from oracle level, which a counter must never raise); I08/S06 4 bloodline powers (Fire Ray,
  Fire Blast, Air Ray, Air Blast) through the fire/air elemental bloodlines
  and their proven Seeker and Crossblooded aliases; O01 16 registered
  performance counters: 14 published (9 Kingmaker, 5 Call of the Wild), 2
  excluded (Storm Call, Mockery) and 14 classified non-targets.

## 3. Charter scenario families (64)

Ladder: CODE COMPLETE, DOMAIN TESTED, NATIVE TESTED (guarded Steam App ID
640820 runs), SAVE TESTED (fresh-process reload). QUALIFIED needs the owner's
review of this local candidate, so it is `no` for every family. `n/a` means
the gate does not apply to that family; `partial` names what was not observed;
BLOCKED means the state cannot be staged under the mission's rules. Status is
the single highest step fully reached.

| ID | Family | Status | CODE COMPLETE | DOMAIN TESTED | NATIVE TESTED | SAVE TESTED | QUALIFIED | Evidence / note |
| --- | --- | --- | --- | --- | --- | --- | --- | --- |
| H01 | Host DLL physically absent | NATIVE TESTED | yes | yes | yes | n/a | no (owner review) | gunslinger-only isolated profile: host absent, KMG class/firearms/races load, no FCB UI |
| H02 | Host installed but disabled, unsupported or partially initialized | DOMAIN TESTED; NATIVE BLOCKED | yes | yes | no | n/a | no (owner review) | AbsentDisabledAndUnloadedHostsAreInactive, ReadinessRequiresCompleteInitializationAndGunslinger; a disabled or partial host cannot be staged without altering the host or UMM settings |
| H03 | Class-before-host / registration-after-host order | NATIVE TESTED | yes | yes | yes | n/a | no (owner review) | contract and host-state: one Gunslinger favored progression and one reward per qualifying level |
| H04 | Gunslinger absent from host map | DOMAIN TESTED; NATIVE BLOCKED | yes | yes | no | n/a | no (owner review) | ReadinessRequiresCompleteInitializationAndGunslinger; removing the Gunslinger from the host map would alter the host |
| H05 | Same version string, changed hash/MVID/shape | DOMAIN TESTED; NATIVE BLOCKED | yes | yes | no | n/a | no (owner review) | SameVersionDifferentBinaryIsRejected; a changed host binary is never staged (charter: never modify host binaries) |
| H06 | Repeat registration or readiness twice | NATIVE TESTED | yes | yes | yes | n/a | no (owner review) | contract: repeat publication is a no-op; stable GUID sets, order and component counts |
| H07 | Missing optional race or power | NATIVE TESTED | yes | yes | yes | n/a | no (owner review) | AbsentProvidersRemoveOnlyTheirRoute; G08/G20 routes absent natively (no playable Goblin/Orc) |
| H08 | Exception midway through publication | NATIVE TESTED | yes | yes | yes | n/a | no (owner review) | contract: injected fault restores the owned arrays exactly; foreign entries unchanged |
| H09 | Preexisting custom JSON rewards | NATIVE TESTED | yes | yes | yes | n/a | no (owner review) | contract/host-state: host JSON rewards preserved; no foreign file writes |
| H10 | Host generic HP/skill and prestige behavior | NATIVE TESTED | yes | yes | yes | n/a | no (owner review) | grit/menus: host HP/skill rewards unchanged; no second automatic-HP patch |
| E01 | Each of the 21 source-addressable race identities | NATIVE TESTED | yes | yes | yes | n/a | no (owner review) | menus race matrix; explicit provider absence for Goblin and Orc |
| E02 | Half-elf human/elf access | SAVE TESTED | yes | yes | yes | yes | no (owner review) | menus; respec Half-elf source; L01 |
| E03 | Half-orc human/orc access | NATIVE TESTED | yes | yes | yes | n/a | no (owner review) | menus: human grit and native Pistol-Whip |
| E04 | Aasimar/Tiefling | NATIVE TESTED | yes | yes | yes | n/a | no (owner review) | menus; Mostly Human bridge (Aasimar keeps host policy) |
| E05 | Four geniekin, Mostly Human OFF/ON | SAVE TESTED | yes | yes | yes | yes | no (owner review) | Mostly Human lane; settings profile OFF run; L01 families |
| E06 | All twelve elemental heritages | NATIVE TESTED | yes | yes | yes | n/a | no (owner review) | Mostly Human heritages; elemental core/advanced |
| E07 | Mostly Human dual identity | SAVE TESTED | yes | yes | yes | yes | no (owner review) | Mostly Human dual identity and bridge; lifecycle; L01 families |
| E08 | Visual race/body changes only | NATIVE TESTED | yes | yes | yes | n/a | no (owner review) | ancestry from race GUID only (RaceIdentitiesAreExactAndComplete); the host bridge stays closed to a human-looking standard geniekin natively |
| E09 | Verified Racial/Planar Heritage provider | OUT OF SCOPE | n/a | n/a | n/a | n/a | no (owner review) | OUT OF SCOPE: no qualified provider installed |
| E10 | Unknown provider, permission cycle, duplicate facts | PARTIAL (native) | yes | yes | partial | n/a | no (owner review) | UnknownCyclicAndDuplicateEvidenceIsBounded; Races Unleashed Suli Mostly Human stays closed natively |
| E11 | Ordinary and Half-elf multiclass progressions | NATIVE TESTED | yes | n/a | yes | n/a | no (owner review) | disposable-favored-class-multiclass |
| E12 | Future versus permanently replaced feature | NATIVE TESTED | yes | yes | yes | n/a | no (owner review) | ImprovedFeatureRuleExcludesOnlyFullReplacement; advanced/menus archetype exclusions |
| E13 | Cancel/backtrack/reopen level-up or respec | NATIVE TESTED | yes | n/a | yes | n/a | no (owner review) | respec cancel; lifecycle pet respec cancel; grit cancel |
| E14 | Empty child set / capped last target | NATIVE TESTED | yes | yes | yes | n/a | no (owner review) | menus: capped counters close; host HP/skill fallback remains |
| E15 | Mercenary, companion, auto-level and fresh character | PARTIAL (native) | yes | n/a | partial | n/a | no (owner review) | custom-companion respec and fresh characters natively; pets never get favored-class progressions; auto-level not exercised |
| E16 | Ancestry change with existing fractions | NATIVE TESTED | yes | n/a | yes | n/a | no (owner review) | respec Half-elf -> Human -> Dwarf; Mostly Human -> Standard |
| M01 | Every supported divisor at N = 0..20 | NATIVE TESTED | yes | yes | yes | n/a | no (owner review) | rank policy; grit 20 levels; menus |
| M02 | Capped and uncapped partial capacities | NATIVE TESTED | yes | yes | yes | n/a | no (owner review) | UncappedDivisorSixDoesNotStopAtEighteen; menus caps |
| M03 | Switch targets between partial investments | NATIVE TESTED | yes | yes | yes | n/a | no (owner review) | oracle/performance per-target ledgers |
| M04 | Duplicate ancestry source routes | NATIVE TESTED | yes | yes | yes | n/a | no (owner review) | AliasRoutesNeverDuplicateOrMultiply; menus one counter |
| M05 | Grit with normal and attribute-replacing archetypes | NATIVE TESTED | yes | yes | yes | n/a | no (owner review) | grit lane |
| M06 | Grit maximum increase/decrease while spent | SAVE TESTED | yes | yes | yes | yes | no (owner review) | fcb-grit-no-refill; L01 spent grit |
| M07 | Misfire across types, ammo, conditions and reliability | PARTIAL (native) | yes | yes | partial | n/a | no (owner review) | mechanics: per-type threshold and floor; not every ammo/condition combination |
| M08 | Confirmation below/equal/above Critical Focus | NATIVE TESTED | yes | yes | yes | n/a | no (owner review) | mechanics; ConfirmationPreservesTheBetterBonus |
| M09 | Attack categories | NATIVE TESTED | yes | yes | yes | n/a | no (owner review) | mechanics: firearm/unarmed/other categories |
| M10 | Pistol-Whip and deed interruption/custom path | PARTIAL (native) | yes | yes | partial | n/a | no (owner review) | mechanics: Pistol-Whip surrogate attack only; interruption path not exercised |
| M11 | Nimble armor and loss-of-Dexterity | SAVE TESTED | yes | yes | yes | yes | no (owner review) | mechanics; lifecycle; L01 Nimble |
| M12 | Dodge timing in both combat modes | PARTIAL (native) | yes | yes | partial | n/a | no (owner review) | mechanics: deed buff; turn-based timing not separately exercised |
| M13 | Initiative at zero/nonzero grit and True Grit | PARTIAL (native) | yes | yes | partial | n/a | no (owner review) | initiative timing in real-time and turn-based entry; True Grit not exercised |
| M14 | Bomb hit/splash/critical/converted damage | PARTIAL (native) | yes | yes | partial | n/a | no (owner review) | elemental core: flat bonus on a direct hit; splash/critical not exercised |
| M15 | Two Bards with different range investments | SAVE TESTED | yes | yes | yes | yes | no (owner review) | performance range; lifecycle membership; L01 bard |
| M16 | Performance selected target and cap | NATIVE TESTED | yes | yes | yes | n/a | no (owner review) | performance range: per-target caps, owner text, ring; exclusions |
| M17 | Two paladins, aura overlap/unlock/replacement | SAVE TESTED | yes | yes | yes | yes | no (owner review) | lifecycle aura overlap; L01 paladin |
| M18 | Pet replacement/resummon/death and Barkskin | SAVE TESTED | yes | yes | yes | yes | no (owner review) | advanced; lifecycle; respec pet; L01 replacement |
| M19 | CMD versus CMB and maneuver subtype | NATIVE TESTED | yes | yes | yes | n/a | no (owner review) | elemental core |
| M20 | Undine Monk at N = 1,2,3,20 | SAVE TESTED | yes | yes | yes | yes | no (owner review) | UndineMonkMixedRateBundle; elemental core; L01 monk |
| M21 | Intimidate/demoralize scope | NATIVE TESTED | yes | yes | yes | n/a | no (owner review) | elemental core |
| M22 | Actual subtype versus geniekin race | NATIVE TESTED | yes | yes | yes | n/a | no (owner review) | elemental core |
| M23 | Undine SR checks | NATIVE TESTED | yes | yes | yes | n/a | no (owner review) | elemental core |
| M24 | Each supported selected-power manifest entry | PARTIAL (native) | yes | yes | partial | yes | no (owner review) | oracle: all 52 scoped, values for Fire Breath and neighbors; advanced: four bloodline powers; L01 revelation and bloodline arithmetic |
| M25 | Neighboring powers and spellbook unchanged | NATIVE TESTED | yes | yes | yes | n/a | no (owner review) | oracle/advanced neighbors and Fireball unchanged |
| M26 | Power at unlock boundary and above 20 | NATIVE TESTED | yes | yes | yes | n/a | no (owner review) | oracle thresholds; FCB virtual levels never unlock a power |
| M27 | No-scaling power or unsupported branch | DOMAIN TESTED (native n/a) | yes | yes | n/a | n/a | no (owner review) | manifest dispositions (not offered) |
| M28 | Separate 1/3, 1/4, 1/6 racial variants | NATIVE TESTED | yes | yes | yes | n/a | no (owner review) | DistinctVariantsKeepTheirOwnRatesAndProfiles; menus |
| L01 | Fresh-process save/reload | SAVE TESTED | yes | n/a | yes | yes | no (owner review) | Word of Recall FCB transaction: one subject per state/mechanic family, including a selected firearm target with native shots after the reload; the own persistence cases of G05, G06 (Dodge), G11, G17, G21, I01, I05, I07, O04, O05, O08, U02, S04 and S06 are NOT RUN |
| L02 | Death/resurrection, area transition, polymorph/return | NATIVE TESTED | yes | n/a | yes | n/a | no (owner review) | working-save-favored-class-lifecycle |
| L03 | Full respec commit and cancellation | NATIVE TESTED | yes | n/a | yes | n/a | no (owner review) | respec lane (Gunslinger, Mostly Human, Sorcerer); lifecycle pet respec |
| L04 | Module OFF/ON across restarts | NATIVE TESTED | yes | yes | yes | n/a | no (owner review) | settings profiles integration OFF/ON |
| L05 | Third-party profile OFF with earned choices | NATIVE TESTED | yes | yes | yes | n/a | no (owner review) | third-party ON/OFF menus and mechanics |
| L06 | Missing external dependency in a saved build | CODE COMPLETE; NATIVE BLOCKED | yes | partial | no | no | no (owner review) | documented recovery; identity retention domain tested; not staged (the mission forbids loading a host-dependent campaign with its dependency removed) |
| L07 | Real-time-with-pause and turn-based modes | PARTIAL (native) | yes | n/a | partial | n/a | no (owner review) | initiative both modes; other families real-time |
| L08 | Unity serialization and release GUID manifest | SAVE TESTED | yes | yes | yes | yes | no (owner review) | manifest tests; the L01 fresh-process reload deserializes the components of every reloaded family subject |
| L09 | Final package versus loaded DLL | NATIVE TESTED | yes | yes | yes | n/a | no (owner review) | every guarded run checks commit, package, DLL and MVID |
| L10 | Unsupported level-cap/gestalt/respec/provider | DOMAIN TESTED (native n/a) | yes | yes | n/a | n/a | no (owner review) | explicit scope statement; no broad compatibility claim |

| Status | Families |
| --- | ---: |
| SAVE TESTED | 11 |
| NATIVE TESTED | 37 |
| PARTIAL (native) | 9 |
| DOMAIN TESTED (native n/a) | 2 |
| DOMAIN TESTED; NATIVE BLOCKED | 3 |
| CODE COMPLETE; NATIVE BLOCKED | 1 |
| OUT OF SCOPE | 1 |
| Total | 64 |

## 4. Final-candidate runs

Every run used the guarded launcher (Steam App ID 640820), verified the
commit, package, DLL and MVID before launch, and used only
`KMG_AUTOMATION_WORKING` or request-local disposable fixtures.

| Scenario | Run | Result | Note |
| --- | --- | --- | --- |
| observe-favored-class-contract | 20260925T0442014336065Z-observe-favored-class-contract | PASS |  |
| observe-favored-class-host-state | 20260925T0442470377650Z-observe-favored-class-host-state | PASS |  |
| disposable-favored-class-elemental-core | 20260925T0443327592809Z-disposable-favored-class-elemental-core | PASS |  |
| disposable-favored-class-elemental-advanced | 20260925T0444268317587Z-disposable-favored-class-elemental-advanced | PASS |  |
| disposable-favored-class-oracle-revelations | 20260925T0445118827121Z-disposable-favored-class-oracle-revelations | PASS |  |
| disposable-favored-class-performance-range | 20260925T0445550085579Z-disposable-favored-class-performance-range | PASS |  |
| working-save-favored-class-lifecycle@KMG_AUTOMATION_WORKING | 20260925T0446375733122Z-working-save-favored-class-lifecycle | PASS |  |
| disposable-favored-class-respec | 20260925T0447410780186Z-disposable-favored-class-respec | PASS |  |
| working-save-favored-class-visual-census@KMG_AUTOMATION_WORKING | 20260925T0448564533908Z-working-save-favored-class-visual-census | PASS |  |
| disposable-favored-class-grit | 20260925T0451021662946Z-disposable-favored-class-grit | PASS |  |
| disposable-favored-class-gunslinger-menus | 20260925T0451520044915Z-disposable-favored-class-gunslinger-menus | PASS |  |
| disposable-favored-class-gunslinger-mechanics | 20260925T0452460362817Z-disposable-favored-class-gunslinger-mechanics | PASS |  |
| disposable-favored-class-initiative-timing | 20260925T0453281176124Z-disposable-favored-class-initiative-timing | PASS |  |
| disposable-favored-class-mostly-human | 20260925T0454100197487Z-disposable-favored-class-mostly-human | PASS |  |
| observe-favored-class-performance-visuals | 20260925T0455038884902Z-observe-favored-class-performance-visuals | PASS |  |
| disposable-favored-class-multiclass | 20260925T0455468192345Z-disposable-favored-class-multiclass | PASS |  |
| working-save-elemental-character-creation-regression@KMG_AUTOMATION_WORKING#class=Fighter;allocation=point-buy;nativeActionCase=racial-actions;race=Ifrit | 20260925T0456291145076Z-working-save-elemental-character-creation-regression | PASS | parameters allocation=point-buy;class=Fighter;nativeActionCase=racial-actions;race=Ifrit |
| working-save-elemental-character-creation-regression@KMG_AUTOMATION_WORKING#class=Fighter;allocation=point-buy;nativeActionCase=racial-actions;race=Oread | 20260925T0501485813744Z-working-save-elemental-character-creation-regression | PASS | parameters allocation=point-buy;class=Fighter;nativeActionCase=racial-actions;race=Oread |
| working-save-elemental-character-creation-regression@KMG_AUTOMATION_WORKING#class=Fighter;allocation=point-buy;nativeActionCase=racial-actions;race=Sylph | 20260925T0506558549217Z-working-save-elemental-character-creation-regression | PASS | parameters allocation=point-buy;class=Fighter;nativeActionCase=racial-actions;race=Sylph |
| working-save-elemental-character-creation-regression@KMG_AUTOMATION_WORKING#class=Fighter;allocation=point-buy;nativeActionCase=racial-actions;race=Undine | 20260925T0512150323621Z-working-save-elemental-character-creation-regression | PASS | parameters allocation=point-buy;class=Fighter;nativeActionCase=racial-actions;race=Undine |
| working-save-elemental-native-respec@KMG_AUTOMATION_WORKING#class=Fighter;allocation=point-buy;race=Sylph | 20260925T0516386681284Z-working-save-elemental-native-respec | PASS | parameters allocation=point-buy;class=Fighter;race=Sylph |
| working-save-elemental-native-respec@KMG_AUTOMATION_WORKING#class=Fighter;allocation=point-buy;race=Oread | 20260925T0523365598188Z-working-save-elemental-native-respec | PASS | parameters allocation=point-buy;class=Fighter;race=Oread |
| working-save-smoke@KMG_AUTOMATION_WORKING | 20260925T0533562110581Z-working-save-smoke | PASS |  |
| observe-favored-class-contract | 20260925T0535332433840Z-observe-favored-class-contract | PASS | settings profile third-party ON (L05; G16/G17/G18/G21 routes offered); the settings file was removed and verified absent |
| disposable-favored-class-gunslinger-menus | 20260925T0536147542447Z-disposable-favored-class-gunslinger-menus | PASS | settings profile third-party ON (L05; G16/G17/G18/G21 routes offered); the settings file was removed and verified absent |
| observe-favored-class-contract | 20260925T0537085074923Z-observe-favored-class-contract | PASS | settings profile Mostly Human OFF (the trait not offered; identities registered); the settings file was removed and verified absent |
| observe-favored-class-host-state | 20260925T0537500689244Z-observe-favored-class-host-state | PASS | settings profile integration OFF (L04; mechanics suppressed, identities registered); the settings file was removed and verified absent |
| observe-favored-class-contract | 20260925T0538309895677Z-observe-favored-class-contract | PASS | settings profile invalid file (schema 2; charter defaults applied and reported); the settings file was removed and verified absent |
| L01 fresh-process persistence (prepare) | 20260925T0531206167709Z-disposable-word-of-recall-favored-class-persistence | PASS | transaction 20260925T0531109526835Z_0f112e09e2c341e4b594527cf2b02a11 |
| L01 fresh-process persistence (verify) | 20260925T0532235047813Z-disposable-word-of-recall-favored-class-persistence | PASS | one subject per state/mechanic family reloaded, with native firearm shots, the paladin aura, companion replacement and a native companion unlink after the reload; settings and Mods tree restored |
| profile gunslinger-only / observe-favored-class-host-state | 20260925T0542174833633Z-observe-favored-class-host-state | PASS | compatibility transaction compat-20260925T054014Z-f21f39c78413 |
| profile gunslinger-call-of-the-wild / observe-favored-class-host-state | 20260925T0545354549902Z-observe-favored-class-host-state | PASS | compatibility transaction compat-20260925T054347Z-846f727d67cb |
| profile gunslinger-call-of-the-wild-favored-class / observe-favored-class-host-state | 20260925T0549104167941Z-observe-favored-class-host-state | PASS | compatibility transaction compat-20260925T054722Z-ca38636ee6dc |

## 5. Adversarial audit

| Area | Result | Evidence |
| --- | --- | --- |
| Optional-host absence | PASS | isolated `gunslinger-only` and CotW-only profiles: host absent, 171/171 leaves registered, core and grit mechanics unaffected, no favored-class UI (H01, H07) |
| Unsupported-host rejection | PASS (domain) | a same-version host with a different binary, MVID or shape is rejected (`SameVersionDifferentBinaryIsRejected`); disabled, unloaded or partially initialized hosts stay inactive (H02, H04, H05) |
| Idempotent registration and exact rollback | PASS | contract lane: a repeated publication is a no-op; an injected fault restores the owned arrays exactly and leaves foreign entries unchanged (H06, H08); contained registry rollback domain tests |
| Empty optional-race arrays never unrestricted | PASS | an absent provider removes only its route (`AbsentProvidersRemoveOnlyTheirRoute`); G08/G20 routes are absent natively; no route is ever registered as an empty race array (H07) |
| Duplicate aliases and cap escape | PASS | one canonical ledger per effect and target (`AliasRoutesNeverDuplicateOrMultiply`, M04); the menus lane closes capped counters; the respec lane's ancestry change keeps one counter and no stale access (E16) |
| Spent-resource refill leakage | PASS | `fcb-grit-no-refill`: every completing pick raises maximum grit without refilling spent grit (M06); the L01 reload keeps spent grit and the raised maximum |
| Shared-blueprint and cross-owner leakage | PASS | O01 widens only the casting bard's own area instance, ring and text (other performers and the blueprint native); paladin auras carry each paladin's own bonus; the pet projection follows only the current pet and leaves no orphan |
| Selected-power manifest completeness | PASS | the target manifest classifies every candidate (52 revelations of which 51 are published and Spirit of the Warrior is excluded, 4 bloodline powers, 16 performances plus 14 non-targets); unpublished and excluded targets are withheld with reasons (M24, M27) |
| Host publication gate (review 1) | PASS | owned effects apply only with the integration enabled and the exact host publication committed; host absent, disabled, unsupported, failed or rolled back gives zero while ranks resolve |
| Owned-power thresholds (review 2) | PASS | each published revelation's own level gates and each Blast's extra uses follow the effective level; no other revelation, power, progression, spell, BAB, save or capstone early |
| Pet projection transitions (review 3) | PASS | unlink, relink, qualification loss, dismissal, resummoning, respec and reload leave exactly one projection on the one qualified pet |
| Range transaction (review 4) | PASS | injected ring failures leave the native radius and ring; the cylinder is never widened alone |
| Exception-safe scopes (review 5) | PASS | the replay and Demoralize scopes close in finally blocks; injected failures leave nothing marked and retries work |
| Bootstrap aggregate (review 6) | PASS | the aggregate counts every registry; registered equals expected with the catalogs' 171 and 13 identities live |
| Save, respec and lifecycle cleanup | PASS | L01 reloads one subject per state/mechanic family in a fresh process, including a selected firearm target; L02 death, polymorph and party area reload; L03 cancelled and committed respecs including the Mostly Human identity, a companion and a selected power |

## 6. Unfinished targets, exclusions and provider absences

No scheduled row is unfinished. The remaining gaps are observational and are
named in the family table (partial entries) and below:

- O01 exclusions: Storm Call (its text promises 50 feet over a 30-foot area)
  and Mockery (its text is a single selected target while the area hits every
  creature) are registered but never published, because no truthful owner
  range can be displayed.
- I06/S04 thresholds (review finding 2, charter 8.10): each published
  revelation's own steps, tiers, breakpoints, single-level uses and own level
  gates follow the effective level, so it gains its own later abilities and
  forms earlier (83 gates moved natively); no other revelation, revelation
  choice, class feature, spell, BAB or save is granted. Spirit of the Warrior
  is excluded (its possession sets BAB from oracle level). Fourteen
  threshold-only revelations are explicitly deferred: their thresholds follow
  the same rules, but publishing them needs owned target identities the ledger
  does not contain.
- I08/S06 thresholds: Elemental Blast's extra uses at 17 and 20 follow the
  effective level (at most two steps) whether the bloodline's level entries
  (native) or the power's own gates (Call of the Wild) grant them; Elemental
  Resistance is explicitly deferred for the same identity reason; Primal
  Elemental bloodlines are not eligible (not fire/air elemental).
- Partially observed families: E10 (permission cycles and duplicate facts
  cannot arise from the installed providers; domain tested), E15
  (auto-level), M07 (every ammunition and condition combination), M10 (deed
  interruption path), M12 (turn-based Dodge timing), M13 (True Grit), M14
  (bomb splash and critical), M24 (numerical values for every one of the 52
  revelations) and L07 (turn-based for every family). Each has a native run
  of its main path; all but E15 and L07 (no domain component) are also
  domain tested.
- Row-level persistence: charter section 11 asks every scheduled row for its
  own persistence case. L01 reloads one subject per state/mechanic family
  (the continuation brief's list, including a selected firearm target), which
  covers the leaves of G01 G02 G04 G06 (Nimble) G07 G08 G10 G14 G16 G18 I06
  I08 O01 O06 O07 U04. The rows G05, G06 (Dodge), G11, G17, G21, I01, I05,
  I07, O04, O05, O08, U02, S04 and S06 (and G20, which shares G05's counter
  and has no playable provider) have no reload of their own counters yet
  (NOT RUN); they use the same saved feature-rank mechanism, which is not
  claimed as their own evidence.
- Not staged natively: H02 (disabled or partially initialized host), H04
  (Gunslinger absent from the host map) and H05 (changed host binary) would
  require altering the host, its binary or UMM settings, which no existing
  authorized workflow does; they are domain tested and fail closed. L06
  (missing external dependency in a saved build) would mean loading a
  host-dependent campaign with its dependency removed, which the mission
  forbids; its recovery is documented and identity retention is domain
  tested (every KMG leaf registers on every load, host present or absent).

## 7. Pre-existing defects D1 and D2

Both are pre-existing KMG defects outside the adopted rows; they are
recorded, not redesigned, and need the owner's decision
(`FAVORED-CLASS-BLOCKERS.md`).

- D1: a base Gunslinger cannot complete level 17. `GunslingerClassBlueprints`
  grants the obligatory Gun Training selection at 5, 9, 13 and 17, but it
  offers only the three rank-one types (Pistol, Musket, Blunderbuss), so once
  all three are taken `LevelUpState.IsComplete()` stays false at level 17
  (native evidence `20260924T0054089932945Z-disposable-favored-class-grit`:
  `openSelections=[KMG_GunTraining_Selection]`, every other completion term
  satisfied, for the test and the control unit). The Pistolero and Musket
  Master replace those picks and the Mysterious Stranger keeps exactly three,
  so they are not stuck.
  Effect on this feature: none of the favored-class rules or counters is
  wrong, but a base Gunslinger never reaches levels 17 to 20 and therefore
  never earns the favored-class rewards of those levels. The twenty-level
  favored-class proof uses the Pistolero for that reason.
- D2: Dead Shot critical confirmation is unreachable. `DeadShotRuntime`
  probes set `RuleAttackRoll.ImmuneToCriticalHit`, and the native attack rule
  computes a threat only when the target is not immune, so no probe records
  a threat and no confirmation is ever rolled. If it were reachable,
  `DeadShotRuntime.ConfigureDelivery` assigns (rather than adds)
  `CriticalConfirmationBonus`, discarding Critical Focus and every other
  confirmation bonus (static evidence: source and the decompiled native
  rule; not reproduced natively).
  Effect on this feature: the firearm confirmation counter (G02, and the
  G08 and G16 routes to the same counter) raises every reachable firearm
  critical confirmation roll, natively tested on ordinary firearm attacks.
  Dead Shot makes no confirmation roll today, so the counter never applies
  to Dead Shot; no favored-class code depends on Dead Shot.

## 8. Installation restoration

The owner's pre-mission KMG install (0.0.136, backup
`C:\Dev\KingmakerGunslingerLab\runtime-backups\live-mod\20260924T0027014050156Z`)
was restored through `scripts/Restore-Live-Mod.ps1` (under this lab's lease)
after the last run, and verified byte for byte:

- `Info.json` SHA-256 `f66de05d5c6282eece8218b6c4f31d49dfc8ef9efa27dfeceeda712034717e17` (match True)
- `FeatureModules.json` SHA-256 `6e24b2788a0c8f063d6e561a27c93f9c5349f2fc21b5217689da8aefdbb385d0` (match True)
- `KingmakerGunslinger.dll` SHA-256 `c6cccdac914ed59fa4d85d020108588d7d12cfb4ac38cf5a162772bacc9b465c` (match True)
- whole tree: 238 backup files, 238 live files, 0 only in the backup, 0 only live
- no `FavoredClassIntegration.json` remains (the default settings apply)
- other mod settings unchanged: `CallOfTheWild\settings.json` SHA-256 `24cc3f80269992a53ebbfd1f5986e5aab056841d6b2f43d8e22e764cdb73f6e8` (match True)
- other mod settings unchanged: `RacesUnleashed\Settings.json` SHA-256 `270899c3f6c3d29bfe777fc2b55a0bb0404ae50786dbac8f1dccf77e8b9cabbf` (match True)
- other mod settings unchanged: `ZFavoredClass\settings.json` SHA-256 `bdceed77d2bf4a31dd9e4eeb64ef9d55a42ef59d23f46abcb1ddbcc6ef66754b` (match True)
- each temporary settings profile removed `FavoredClassIntegration.json` after
  its own runs (verified absent after each profile and at the end)
- the persistence transaction restored the settings and the complete Mods tree
  (`20260925T0531109526835Z_0f112e09e2c341e4b594527cf2b02a11`); each compatibility profile restored the exact original Mods tree
  and FeatureModules bytes before releasing the lock.

Restoration status: VERIFIED.

## 9. Blockers

`FAVORED-CLASS-BLOCKERS.md` keeps only what is genuinely open: the
pre-existing KMG defects D1 and D2 (owner decisions), the environment note
B1, and two suspected host defects. B2 is resolved; D3 and D4 were fixed in
this mission. Every former owner question (OD-1 to OD-10) is resolved there
with the charter rule or evidence that decides it.

Remaining qualification gaps (why this candidate is PARTIAL):

- the partially observed families E10, E15, M07, M10, M12, M13, M14, M24 and
  L07 (section 6);
- the own persistence cases of G05, G06 (Dodge), G11, G17, G21, I01, I05, I07,
  O04, O05, O08, U02, S04 and S06 (NOT RUN; section 6);
- the unstaged states H02, H04, H05 and L06. H02, H04 and H05 would require
  altering the host, its binary or UMM settings (domain tested, fail closed);
  L06 would require loading a host-dependent campaign with its dependency
  removed, which the mission forbids (recovery documented).

## 10. Review findings (PR #24)

Each finding was addressed in order, then the whole candidate was requalified
on the exact final package (section 4). Native evidence names the lane
assertion and its run under
`C:\Dev\KingmakerGunslingerLab\runtime-evidence\`.

| Finding | Commits | Domain tests | Native evidence |
| --- | --- | --- | --- |
| 1. Runtime host gate: owned effects need the integration and a committed qualified host | `ae0f1dafd` | `favored-class.host-activation-transitions`, `favored-class.host-mechanics-truth-table`, `favored-class.host-commit-activates` | `fcb-mechanics-follow-integration`: Published with the host committed gives the two grit steps (`20260925T0442470377650Z-observe-favored-class-host-state`); integration OFF keeps the rank and gives zero (`20260925T0537500689244Z-observe-favored-class-host-state`); host absent in the `gunslinger-only` and CotW-only profiles gives zero (`20260925T0542174833633Z-observe-favored-class-host-state`, `20260925T0545354549902Z-observe-favored-class-host-state`); the host-present profile gives the steps again (`20260925T0549104167941Z-observe-favored-class-host-state`) |
| 2. Selected revelation and bloodline thresholds (charter 8.10) | `f9a8e6cfb`, follow-up `27176f424` | `favored-class.gate-decision-native`, `favored-class.power-use-thresholds`, `favored-class.power-use-gates`, updated revelation and power source tests | `fcb-oracle-level-gates`: 83 own level gates of the published revelations decided at the effective level, 83 moved, none skipped (`20260925T0445118827121Z-disposable-favored-class-oracle-revelations`); `fcb-advanced-bloodline-powers`: Blast's extra uses at 17 and 20 follow the effective level with at most two steps, Ray's uses unchanged (`20260925T0444268317587Z-disposable-favored-class-elemental-advanced`); Spirit of the Warrior is not published (the contract lane's published graphs, `20260925T0442014336065Z-observe-favored-class-contract`); the first requalification run `20260925T0414053062811Z` exposed that Call of the Wild moves Blast's extra uses into the power's own gates, fixed by the follow-up |
| 3. Pet projection follows one qualified desired pet | `078978a4c`, follow-up `aa298650e` | `favored-class.pet-sync-transitions`, `favored-class.pet-sync-plan`, `favored-class.pet-sync-wiring` | `fcb-lifecycle-pet-transitions`: native unlink without replacement, relink, the same pet losing and regaining qualification, native dismissal and a resummoned companion (`20260925T0446375733122Z-working-save-favored-class-lifecycle`); `fcb-lifecycle-respec-pet` (`20260925T0446375733122Z-working-save-favored-class-lifecycle`); persistence verify `word-of-recall-fcb-persistence-family-ranger-replacement-after-reload` and `word-of-recall-fcb-persistence-family-ranger-unlink-after-reload` (`20260925T0532235047813Z-disposable-word-of-recall-favored-class-persistence`) |
| 4. Bard range transaction | `da07c4e07` | `favored-class.range-widening-success`, `favored-class.range-widening-injected-failures`, `favored-class.range-widening-ringless-deferred` | `fcb-performance-injected-failure`: a scaler that scales then throws and one that scales nothing leave the real instance at its native radius with its ring restored exactly; the healthy instance widens both (`20260925T0445550085579Z-disposable-favored-class-performance-range`); radius, ring, text, exclusion and pool-release assertions (`20260925T0445550085579Z-disposable-favored-class-performance-range`); native visuals (`20260925T0455038884902Z-observe-favored-class-performance-visuals`) |
| 5. Exception-safe scopes (level-up replay, Demoralize) | `f4f5b9931` | `favored-class.scope-nested-restores-previous`, `favored-class.scope-exception-closes`, `favored-class.scope-guard-closes-stale-frame`, `favored-class.scope-retry-after-failure`, `favored-class.scope-finally-sources`, updated replay source test | `fcb-elemental-native-mechanics`: a real `[Demoralize, Intimidate check]` action list gives +2 then +0 healthy, +0 after a failure injected inside the frame (depth 0 after each list), and +2 then +0 on retry (`20260925T0443327592809Z-disposable-favored-class-elemental-core`); `fcb-advanced-bloodline-powers`: a failure injected into the reward's replayed pick leaves nothing counted and the retried native replay applies the reward (`20260925T0444268317587Z-disposable-favored-class-elemental-advanced`); every level-up lane replays through the scoped calls |
| 6. Bootstrap aggregate counts | `719085d51`, follow-up `7bb4a7040` | `favored-class.source-invariants` (both sides of the aggregate, all five registries, 171 catalog identities) | `fcb-bootstrap-aggregate`: registered 2,085 = expected 2,085; 171/171 favored-class and 13/13 Mostly Human identities live (`20260925T0442014336065Z-observe-favored-class-contract`); the first run `20260925T0411366030026Z` showed a manifest equality was not a property of the aggregate (see below) |

Intermediate candidates of this round (superseded; each exposed a defect that
the next commit fixed):

- `719085d51` (deployment `20260925T0411348835015Z`): contract
  `20260925T0411366030026Z` FAIL only on the new aggregate pin, whose added
  manifest equality was wrong (the aggregate itself was exact at 2,085);
  host state `20260925T0412292153641Z` and elemental core
  `20260925T0413132204168Z` PASS; elemental advanced `20260925T0414053062811Z`
  FAIL on the Blast use thresholds (finding 2 follow-up); the remaining four
  stage-1 lanes were refused by the guarded launcher before launch because a
  source edit made during the batch left the worktree dirty.
- `27176f424` (deployment `20260925T0426279539132Z`): seven stage-1 lanes
  PASS; lifecycle `20260925T0430574929905Z` ERROR in the new pet-transition
  step, which waited for fixtures an earlier step had destroyed (finding 3
  follow-up); every earlier lifecycle assertion of that run passed.

The installed manifest lists 2,138 active identities: the aggregate's 2,085,
the Brown-Fur extension's 25 (its own registry) and 28 elemental-race
visual assets that are not blueprints (KMG.ElementalRaces.Ifrit.Visual.Body.Male ... KMG.ElementalRaces.Undine.Visual.Head.Female.02).
