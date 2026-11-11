# Summon menu order, summon Sunder and the Lillend label: status

**Candidate `3f170d4a9c`: native FAIL, 44 PASS / 1 FAIL. The single failure (`sunder-census`) is an observer
defect; a reader-only re-evaluation of the recorded data passes, and the original FAIL is retained. Every other
cell passed, including all ordering, label, spider and Sunder behavior cells. Review checkpoint; nothing merged or
released.** (2026-11-11)

The earlier candidate `c6e861a98a` keeps its own initialization FAIL; it established no menu, label or Sunder
behavior, and none of the results below belong to it.

## Continuation result (candidate `3f170d4a9c`)

### Observer correction (source `3f170d4a9c`)

- `RuntimeTesting/SunderObservationVerdict.cs` follows the R1 discipline. It reads ProperFlanking2's actual
  activation and verifies the exact chain:
  - Improved Sunder `9719015e...` grants the Sunder action `fa9bfb9f...` and `SunderActionToggleAbility`
    `d9604ead...` (name exact, default-on);
  - that toggle applies `SunderActionToggleBuff` `f9f7edc1...`, which carries `AttackReplacementWithCombatManeuver`.
- The chain classifies as `Inactive`, `InactiveIdentitiesPresent`, `ActiveExact` or `ActiveMismatch`. Absence
  can be asserted only when the feat and action resolve and the chain is `ActiveExact` (or genuinely `Inactive`,
  which is a qualified case). A cleanup passes only if the claimed pre-removal state was observed and everything
  was gone afterwards.
- The observer adds a `sunder-provider-chain` prerequisite. Fresh-absence cells and the structural census
  require it. The legacy check is split into one cell per unit.
- The Greater Earth Elemental's induced state rebuilds the exact former native brain action list, read from the
  unmodified shared `GreaterEarthElementalBrain`. A fresh elemental native attack is observed as well.
- Four verdict regression tests: active exact chain, active mismatched chain (each missing part), genuinely
  inactive provider (qualified), and missing pre-removal state.
- Product code is unchanged from `896c089a19`, including the exact-type `BlueprintAiCastSpell` fix.

### Focused checks

`Invoke-KmgGate.ps1 -Filter` with the recorded filter: 95 selected of 2,934, 0 failures, production compiled. Two
runs, 23 s and 20 s; the second followed one census strictness edit. Selected per prefix: alternate-summoning 33,
expanded-summoning 17, summon-cleric-alignment 19, summon-menu-order 5, summon-playtest-r1 5,
summon-spider-summoner 9, summon-sunder 7. Sorted selected-name list SHA-256 `1c19809dcb18...9766`. The build ran
the same 95.

### Candidate

| Item | Value |
| --- | --- |
| Source | `3f170d4a9ceaad457546c4a714a4c92d2f68cfc1`, v0.0.181 development, purpose `summon-menu-order-and-sunder` |
| Package SHA-256 | `012e469a079181eee58cdbf72a65dc5f3a777398ed13172bb5693084a8a9b89d` |
| DLL SHA-256 / MVID | `cec3f24c82671556a1b82f5bf46e6118da132873cd4576aebea1703d0b69e72a` / `2be9b4e9-1dbc-4154-a993-4ebea76b798c` (also the loaded module) |
| Build | 416 s; 95/2,934 tests; development contract PASS 89/0; strict UMM package validation passed; explicit `-ReferenceBundleDir artifacts/summon-overhaul-baseline/reference-input/KingmakerGunslinger-private-build-references` |

### Native invocation

One guarded run on DATA: wrapper `Invoke-ExpandedSummoningRuntimeScenario.ps1` (Params.xml-protected), launched
through a Windows PowerShell 5.1 child.
- Scenario `disposable-expanded-summoning-sprint32-native-review`, `reviewPhase=summon-menu-order-sunder`, on the
  combined installation.
- Run `20261111T0318117060805Z-00abec6c34c34058b5893177511d5ffd`; evidence
  `runtime-evidence/20261111T0318116815327Z-disposable-expanded-summoning-sprint32-native-review` (private).
- Wall time 154.7 s; runner 162 s.
- Memory go/no-go 10.7 / 11.2 / 9.6 GB free. The pointer stayed at (300, 200) in all 31 samples.

**Mod initialization succeeded.** The loaded build is 0.0.181 at `3f170d4a9c`. **Result: 44 PASS, 1 FAIL.**

| Area | Cell | Observed |
| --- | --- | --- |
| Order census (structural) | `order-census` PASS | 93 arrays (18 canonical, 75 alternate), 5,273 rows: all quantity then name; 0 unclassified; 0 Stirge. The late pass reordered all 18 canonical menus, as expected once generated spider rows are appended to every tier |
| Spider census (structural) | `order-spider-census` PASS | 46/46 placements once each in their own matrix quantity group; integration `repaired=8;generated=38;problems=` |
| Provider prerequisite | `sunder-provider-chain` PASS | ProperFlanking2 active; chain `ActiveExact`; Improved Sunder grants exactly `SunderAction` and `SunderActionToggleAbility` |
| Sunder census (structural) | `sunder-census` **FAIL** (observer defect) | 148 spawnable units from 93 roots, scanned. The only flagged references are inside the four `SummonWithdrawnCapabilityFixer` components (Storm, Fire, Greater and Elder Earth): their own withdrawal lists, not grants. No unit fact or brain holds a Sunder source. Elemental brains are ordinary, the shared native donor, brain and feat are unchanged, and the new fixers are attached. Reader-only re-evaluation excluding the fixers' own lists: **PASS**, 0 carriers (`runtime-evidence/summon-menu-order-and-sunder/20261111-sunder-census-reader-reevaluation.json`, SHA-256 `5df455802e50...`; source result SHA-256 `3db91eb96835...`). Original FAIL retained; observer source unchanged |
| Lillend labels (structural) | `order-lillend-labels` PASS | SM VI "Azata, Lillend"; VII "(1d3)"; VIII and IX "(1d4+1)"; identities `KMG_Summoning_Ability_SM_TierN_LillendAzata_*` unchanged |
| Feat-absent SM V popup | `order-spider-feat-absent` PASS | 75 rows; 0 Spider Summoner rows; the ordinary Giant Spider row present |
| Feat-enabled SM V popup | `order-spider-tier5-mixed` PASS | 78 rows. Original `Summon Monster V (Ogre Spider)` (RU identity `...SummonMonsterOgreSpider`) at index 13 in the single group. Generated `Spider, Giant Black Widow (1d3)` at 41, inside the 1d3 group (indexes 15-43). Generated `Spider, Giant Crab (1d4+1)` at 74, inside the 1d4+1 group (from 44). Exactly 3 spider rows, no duplicates. Feature fixture-granted |
| Unrestricted SM VII popup | `order-sm7-single-group-first` PASS | 107 rows; singles 0-12 (first: Bebelith); 1d3 group ends at 28; `Ankylosaurus (1d4+1)` at 29 |
| SM VI popup | `order-lillend-menu` PASS | "Azata, Lillend" at index 1 (after Axiomite, before Bear, Dire), identity `..._Tier6_LillendAzata_One` |
| Druid level 5 conversion | `order-druid-conversion-spiders` PASS | 76 rows, the native factory equals the popup, no duplicates. Spider rows: the original SNA V Ogre Spider plus the generated Widow (1d3) and Crab (1d4+1) |
| Generated-row selection | `order-druid-select-generated-spider` PASS | Row 40, `KMG_Summoning_Spider_SNA_Tier5_GiantBlackWidow_OneD3` (`d9145ca9...`): the exact row object selected. It is converted from the native quantity conversion `03e8e960...` of the exact prepared source, in the same book; the receipt source matches; placement `KMG.Summoning.Spider.SNA.Tier5.GiantBlackWidow.OneD3`. Selection only, no cast |
| Fresh Storm Giant | `sunder-storm-fresh` PASS | Absence assertable. No feat, action or toggle, 0 buffs. Greatsword kept; brain is its own spells plus the native `AttackAiAction`; fixer attached |
| Storm Giant attack | `sunder-storm-attack` PASS | Native full attack: 3 weapon attacks, 0 maneuvers of any kind, 0 replaced slots. Only `BullRushActionToggleBuff` was active (charge-only), not counted as Sunder. The Sprint 4 routine verdict also passed |
| Fresh Greater Earth Elemental | `sunder-earth-elemental-fresh` PASS | No feat, action or toggle; brain is native Dumb Monster Brain `5abc8884...` = [`AttackAiAction` (`BlueprintAiAttack`, AttackTargetsPriority)] |
| Elemental attack | `sunder-earth-elemental-attack` PASS | Native full attack: 2 slam attacks, 0 maneuvers, 0 replacement buffs |
| Legacy Storm Giant (induced) | `sunder-legacy-storm-giant` PASS, outcome `Proven` | Before: feat, action, toggle on/running, replacement buff 1/1 active. After the real fixer: all absent within the same frame; weapon and brain unchanged; a repeat changed nothing |
| Legacy Greater Earth Elemental (induced) | `sunder-legacy-earth-elemental` PASS, outcome `Proven` | Before: feat, action, toggle running, buff active, and the rebuilt former brain [`SunderArmorAiAction` (`BlueprintAiCastSpell`), `GreaterEarthElementalAttackAiAction` (`BlueprintAiAttack`)]. After: all Sunder state gone; the brain holds exactly `GreaterEarthElementalAttackAiAction`; a repeat changed nothing |
| Controls | `sunder-fixer-exact-owner` PASS | The non-summon caster keeps an induced Improved Sunder through both fixers; the elemental fixer leaves the Storm Giant alone |
| Restoration in-game | `order-owned-state-restored`, `fixture-restored`, native-profile restored PASS | Owned books, facts, party and UI selection restored |

**Screenshots (private, 5, 1920x1200).** These were inspected. The feat-enabled SM V grid (78 rows) and the Druid
conversion grid (76 rows) are readable, with every row carrying an icon and no blanks. The three Races Unleashed
spider rows in the Druid popup show its green paw icon at grid positions 11, 40 and 72, consistent with the
recorded single, 1d3 and 1d4+1 indexes. The grids show icons, not labels, so order and labels rest on the
recorded row objects.

**Precise limits.**
- Legacy state was **induced** on fresh summons, and each fixer's `OnPostLoad` was called directly. No older save
  was cold-loaded; native `m_AppliedFixers` bookkeeping and real load order were not exercised.
- A saved former elemental keeps `GreaterEarthElementalAttackAiAction`. Its target considerations
  (AttackTargetsPriority, LightArmorConsideration, HasBuffSunder) are recorded, but AI decisions with that action
  were not observed. Its ordinary attack was shown by a commanded native attack on a fresh elemental, not by AI.
  Fresh summons use the native ordinary-attack brain. Whether the fixer should also swap that retained action for
  the native `AttackAiAction` is a possible follow-up for the owner, not done here.
- Attacks were native commands observed passively, not AI choices. Popups were the fixture's known-book SM V, VI
  and VII and one Druid level 5 conversion. Herald, alternate sources and every other tier are census-only.
- This does not claim that no summon can produce any `SunderArmor` rule event. The Bebelith dismantle (below) does.

**Bebelith (unchanged).** Its Dismantle Armor is a post-claw-hit creature ability that rolls a native
`SunderArmor` combat maneuver. It is not a standard Sunder action, feat grant or automatic attack replacement.
Treating it as out of SUNDER-01's scope is a **proposed** distinction for final owner review, not an owner
approval already received.

### Restoration (this invocation)

Wrapper record `runtime-evidence/expanded-summoning-restoration/20261111T0320134304754Z-...json`:
- `restoration: verified`;
- live tree before and after restore 254 files `5C3E4AD4...BFBB`;
- `Params.xml` restored byte-exact `E44F4495...30BB`.

Independent Windows PowerShell 5.1 check afterwards:
- live v0.0.140, DLL `00EF6757...66FD`, 254 files, tree `5C3E4AD4...BFBB`;
- `Params.xml` `E44F4495...30BB` (3,077 bytes);
- the same 16 enabled mods; 95 saves, none modified since the run started;
- no Kingmaker process.

`KMG_AUTOMATION_BASELINE` and every campaign save were untouched; the scenario is save-free. There was no foreign
lock and no other session.

### Ledger (continued)

| Step | Seconds |
| --- | ---: |
| Before this continuation | about 627 |
| Focused gates (23 + 20) | 43 |
| Candidate build `3f170d4a9c` | 416 |
| Guarded invocation (runner wall) | 162 |
| **Cumulative** | **about 1,248 of 1,800** |

Not counted: reading evidence, the reader-only re-evaluation and documentation.

---

The sections below are the earlier record, kept as written for candidate `c6e861a98a`.


A three-correction development checkpoint on `codex/summon-menu-order-and-sunder`, started from master
`3fd5bf5d0b` (the PR #125 merge of the Races Unleashed Spider Summoner integration). Nothing here is merged,
released or permanently installed. The owner reports everything else looks fine. The merged spider integration is
preserved scope, not deferred work.

## Owner decisions recorded (2026-11-10)

1. **Quantity-first summon menus.** Primary key: base multiplicity (one, then 1d3, then 1d4+1). Secondary key: the
   displayed family-first creature name, compared ordinally and case-insensitively. Tertiary key: stable identity.
   This **supersedes the name-first rule** of the alphabetical-menu mission
   ([summon-alphabetical-menus/STATUS.md](../summon-alphabetical-menus/STATUS.md)), whose records and verdicts stay
   with their own candidates. "1d4" means the existing 1d4+1 group; no mechanics change.
2. **No Sunder on summons (SUNDER-01).** No summon offered through this module's Summon Monster / Nature's Ally
   routes keeps or uses Sunder. This supersedes R1 CUT-05's "Storm Giant keeps its grant"
   ([summon-playtest-repair/R1-STATUS.md](../summon-playtest-repair/R1-STATUS.md)); R1's verdicts are unchanged.
3. **Lillend label.** `lillend-azata` presents as Family `Azata`, Subtype `Lillend`: "Azata, Lillend",
   "Azata, Lillend (1d3)", "Azata, Lillend (1d4+1)".

## Intake (DATA, 2026-11-10)

- Host `Data`. Master `3fd5bf5d0b` = PR #125 merge; final spider candidate `0908e3415fa4`; merged, not released.
  No earlier `codex/summon-menu-order-and-sunder` branch existed locally or on origin.
- Live install: KingmakerGunslinger **v0.0.140** (merged code is not installed), DLL `00EF6757...66FD`, 254 files,
  tree `5C3E4AD4...BFBB` (Windows PowerShell 5.1 fingerprint).
- UMM `Params.xml` `E44F4495...30BB` (3,077 bytes); 16 enabled mods: BagOfTricks, BetterVendors, CallOfTheWild,
  CheatMenu, CraftMagicItems, EddicKingmakerRespec, KingmakerBuffPlanner, KingmakerBugfixes, KingmakerDiceRoller,
  KingmakerGunslinger, KingmakerLastAzlantiPreserver, ProperFlanking2, RacesUnleashed, SkipIntro, TweakOrTreat,
  ZFavoredClass.
- 95 save files. No other Claude session and no Kingmaker process at intake.

## 1. Quantity-first ordering

`SummonDisplayOrderPolicy`'s single comparer now compares base multiplicity first, then name, then identity. A
creature row whose base multiplicity is unknown follows the three groups, alphabetized; unclassified rows keep
their existing trailing placement. Every existing boundary uses that one comparer unchanged:

- canonical publication (`ExpandedSummoningPublisher.PublishParent`) for Summon Monster and Summon Nature's Ally
  I-IX, including higher-tier quantity choices and retained native rows (native quantity from
  `SummonNativeOptionCatalog`);
- the alternate-source composer (`AlternateSummoningMenuPolicy.Compose`), per menu root, including direct
  per-quantity roots;
- the late pass after foreign `LoadDictionary` postfixes (`ReorderLateHostRows`), still after the Spider Summoner
  integration (`AlternateSummoningCoordinator.TryReconcile`);
- Druid and Herald conversion popups, which take each child's position from its own published parent per quantity
  conversion (`DruidSummonConversionVariantPatch`), so separate spell levels and conversion contexts stay separate.

**Spider rows.** Generated rows already carry owned keys from their placement (`AddLateOwnedRow`). The eight
original Races Unleashed rows now take their quantity from the placement matrix by exact identity and tier
(`SpiderSummonerCatalog.OriginalRowQuantity`, always one at base tier); they keep the label Races Unleashed gives
them, as recorded at RU closeout, and sort by it inside the single group. No row is renamed, re-identified or
derived from list position. No per-frame sort, no registry reorder, no new UI patch; the late pass still writes a new
array to this package's own component and leaves the publication receipt unrefreshed, so rollback still refuses
after a foreign append.

**Decision for review.** The original rows display "Summon Monster V (Ogre Spider)" etc., so they sort as that
label (the "S" region of the single group), not as "Spider, Ogre". Relabelling another mod's rows was out of scope.

## 2. Sunder audit and removal (SUNDER-01)

Exact-identity audit of the installed 2.1.7b blueprints (offline reverse-reference scan of
`Kingmaker_Data/sharedassets1.assets`, read-only) plus every KMG builder:

| Source | Holders | Reachable through this module's summon routes? |
| --- | --- | --- |
| Native Improved Sunder `9719015e...` (grants `SunderAction` `fa9bfb9f...`; ProperFlanking2 adds `SunderActionToggleAbility` `d9604ead...` -> buff `f9f7edc1...`) | KMG Storm Giant (own grant); KMG Fire Giant (already withheld, R1); native `SummonedEarthElementalGreater`/`Elder` and hostile `CR9`/`CR11` earth elementals (unit facts); Living Armor and Fredero Sinnet (class levels) | **Storm Giant**, and the KMG **Greater and Elder Earth Elementals**, whose unit clones copy the native donor's facts |
| Native `SunderArmorAiAction` `23bcb18f...` | `GreaterEarthElementalBrain` `d8342bca...`, `SunderMonsterBrain`, Fredero's brain | the KMG Greater and Elder Earth Elementals (donor brain copied by reference) |
| Native Greater Sunder `54d82402...` | feat selections; Living Armor and Fredero class levels | no |
| Bebelith dismantle (resolved through a `SunderArmor` maneuver roll) | KMG Bebelith printed special attack | kept: a printed creature rider, not a Sunder feat, action, toggle or AI use (see open question) |

No donor, retained native unit or native-expansion unit other than those two earth elementals references any of
these sources. Living Armor and Fredero are encounter units, not summon choices.

**Changes.**
- Storm Giant: `SummonPlaytestWithdrawalPolicy.GrantedNativeFeats("storm-giant", ...)` withholds Improved Sunder
  (the same route as Fire Giant). Other feats, weapons, attacks, SLAs and resources unchanged.
- Greater and Elder Earth Elementals: the withdrawal step drops Improved Sunder from each KMG unit's facts and
  points its brain at the unmodified native Dumb Monster Brain `5abc8884...` (ordinary attack only, already used by
  the Huge Earth Elemental). The native brain's other action, `GreaterEarthElementalAttackAiAction`, scores targets
  around sundered armor. Any unexpected input brain fails closed. No native feat, ability, brain, AI action or donor
  is modified, and ProperFlanking2 is untouched.
- Older saves: new `SummonWithdrawnCapabilityFixer` components `$KMG_R1_WithdrawnCapabilities_StormGiant`,
  `..._GreaterEarthElemental` and `..._ElderEarthElemental`. None of these units carried a fixer before, so each
  name is new and runs once per older saved summon (native `m_AppliedFixers`). Each removes only the exact feat
  (native AddFacts deactivation takes the Sunder action, toggle and buff with it) and, for the elementals, the exact
  native Sunder AI action from the saved brain, on the exact KMG unit blueprint only. A saved elemental keeps its
  saved attack action.

**Open question for the owner.** The Bebelith's printed armor dismantle resolves through the engine's
`SunderArmor` maneuver roll. It was not treated as Sunder and is unchanged (see the continuation result above for
the proposed scope distinction).

## 3. Lillend label

`SummonChoicePresentationPolicy`: `P("lillend-azata", "Azata", "Lillend")`. All menus and adapters use this one
formatter. Keys, blueprint identities, manifest symbols, assets, icons, spells, performances and the unit's own
in-world name are unchanged.

## Focused validation

`scripts/Invoke-KmgGate.ps1 -Filter` (production compiled), filter
`summon-menu-order;summon-sunder;expanded-summoning.display-order;expanded-summoning.alphabetical;alternate-summoning;expanded-summoning.menu-scale-order;summon-playtest-r1;summon-spider-summoner;summon-cleric-alignment;expanded-summoning.sprint32-public-lillend-routes;expanded-summoning.sprint32-native-admission;expanded-summoning.cleanup2-repeat-publication;druid`:

| Run | Source | Result | Seconds |
| --- | --- | --- | ---: |
| 1 | uncommitted | 91 selected of 2,930; **1 failure**: `summon-playtest-r1.withdrawn-identities-retained` still pinned the pre-SUNDER-01 withdrawal inventory (expected; updated) | 28 |
| 2 | `c6e861a98a` content | 91 of 2,930, 0 failures | 4 |
| 3 | `896c089a19` | 91 of 2,930, 0 failures | 18 |

New tests (`SummonMenuOrderAndSunderTests`): `summon-menu-order.sm7-single-group-first`,
`.spider-46-placements`, `.spider-tier5-mixed`, `.spider-conversions`, `.lillend-label`,
`summon-sunder.withdrawn-everywhere`, `.fixer-names`, `.legacy-cleanup-exact`. Superseding updates (historical
verdicts unchanged): `expanded-summoning.display-order` (now quantity then name, plus an unknown-quantity row),
`alternate-summoning.alphabetical-every-family-tier`, `expanded-summoning.alphabetical-druid-flattened`,
`.alphabetical-late-host-rows`, `.alphabetical-late-sort-rollback`, `expanded-summoning.menu-scale-order`, the
Master Summoner composition check in `AlternateSummoningTests`, `expanded-summoning.sprint32-public-lillend-routes`
(label), and `summon-playtest-r1.only-named-capabilities-withdrawn` / `.withdrawn-identities-retained` (Storm
Giant and the two elementals now withdrawn). The independent label-order check shared by the alphabetical and
cleric observers (`AlphaOrderVerdict`) and the old alphabetical phase's Druid cast-target predicate were moved to
quantity-first. The historical R1 observer is unchanged; it already could not rerun after R5 added Cloud Giant.

Limits: domain tests cannot load game types, so they did not catch the exact-type defect below. Not full
qualification.

## Candidate

| Item | `c6e861a98a` (executed natively) | `896c089a19` (current, not built) |
| --- | --- | --- |
| Source | `c6e861a98a93433984de5302289224c00f3ac48b` | `896c089a19` |
| Purpose / version | `summon-menu-order-and-sunder`, 0.0.181 development (baseline tag `v0.0.181`) | same |
| Package SHA-256 | `a24041f3da0be4678b5d114032904411726e309843cd86c808b2520a6b0aca1c` | none |
| DLL SHA-256 | `fbbf599979a25bebe0177480e860f3f43dfad895bdec649ff598452702230409` | none |
| Source-state fingerprint | `32e9dd1105b743c55c9a181240525db4f0c153001ba76b928f4ac0d127074161` | none |
| Build | 465 s; candidate filter 91/2,930, 0 failures; development contract PASS 89/0; strict UMM package validation passed | not built |

Built with `-ReferenceBundleDir artifacts/summon-overhaul-baseline/reference-input/KingmakerGunslinger-private-build-references`.

## Native checkpoint

One guarded invocation on DATA (`Invoke-ExpandedSummoningRuntimeScenario.ps1`, Params.xml-protected), scenario
`disposable-expanded-summoning-sprint32-native-review`, phase `summon-menu-order-sunder`, combined installation.
Run `20261111T0215184361686Z-aa6741a4b6e3473c936f65e1a4443147`, evidence
`runtime-evidence/20261111T0215184121720Z-disposable-expanded-summoning-sprint32-native-review` (private).
Wrapper 104.5 s; runner 112 s including the memory go/no-go.

**Result: FAIL, no observer assertion executed.** Loaded build 0.0.181 at `c6e861a98a`. KingmakerGunslinger's
blueprint initialization failed closed (`bootstrapFailedAtStage=request-accepted`):

> Native blueprint type mismatch: role='native Sunder Armor AI action', guid='23bcb18f2dd735c4d9d2d228f654efce',
> expectedType='...BlueprintAiAction', actualType='...BlueprintAiCastSpell'.

All 3,187 owned registrations rolled back, alternate families reported `Blocked`, and the other modules failed
closed without disabling the game. **Classification: product defect** in the new withdrawal step
(`BlueprintLibraryLookup.RequireExact<T>` requires the exact type). The two native brain lookups immediately
before it succeeded, so both brains are exactly `BlueprintBrain`. Nothing about ordering, Sunder behavior, labels
or the spider integration was observed on these bytes.

**Fix (`896c089a19`, focused checks only).** The withdrawal step and the observer request
`BlueprintAiCastSpell`; the observer reads the native Sunder action type-tolerantly. Not built, not run.

Per the mission rule (no automatic second build or run), the native itinerary below has **not** run:
unrestricted SM VII; feat-absent and feat-enabled mixed SM V spider popups; the "Azata, Lillend" SM VI popup; a
Druid level 5 conversion with one generated-spider selection; a fresh Storm Giant state and one native full attack
with a passive maneuver probe; induced legacy cleanup on fresh Storm Giant and Greater Earth Elemental summons
with exact-owner, foreign-fixer and non-summon controls; and the structural order, spider and Sunder census.

Pointer: parked at (300, 200). Samples read (0, 0) during the game session, with three other positions near
launch and exit. These are consistent with the game owning the cursor; this is not established as human input
and did not affect the outcome.

## Restoration

Wrapper restoration record
`runtime-evidence/expanded-summoning-restoration/20261111T0216296869972Z-disposable-expanded-summoning-sprint32-native-review.json`:
`restoration: verified`. Live tree before 254 files `5C3E4AD4...BFBB`, after the scenario 461 files, after restore
254 files `5C3E4AD4...BFBB`. `Params.xml` restored byte-exact `E44F4495...30BB` (receipt
`runtime-backups/umm-params/20261111T0214460918693Z-.../umm-params-receipt.json`). Independent Windows
PowerShell 5.1 check afterwards: live v0.0.140, DLL `00EF6757...66FD`, 254 files, tree `5C3E4AD4...BFBB`,
`Params.xml` `E44F4495...30BB` (3,077 bytes), the same 16 enabled mods, 95 saves, and no save file modified since
the run started. No Kingmaker process left. `KMG_AUTOMATION_BASELINE` and every campaign save untouched (save-free
scenario). No foreign lock, no other session.

## Validation ledger (900 s target, 1,800 s ceiling)

| Step | Seconds |
| --- | ---: |
| Focused gate 1 (one stale pin) | 28 |
| Focused gate 2 | 4 |
| Candidate build `c6e861a98a` | 465 |
| Guarded native invocation (runner wall, incl. memory sampling) | 112 |
| Focused gate 3 (fix) | 18 |
| **Total** | **about 627** |

Not counted: read-only offline blueprint scans (about 15 s), intake and restoration hash checks, documentation.
About 1,173 s of the ceiling remain. A second build (about 465 s) plus one guarded run (about 110-250 s if the
phase executes fully) would fit, but it requires explicit owner authorization.

## Not established

- Any native observation of the corrected ordering, the Lillend label, the Sunder removal or the preserved spider
  behavior on executable bytes. The only native run failed at initialization.
- A cold load of an older save; the planned legacy check is induced state only.
- Herald conversion natively (planned coverage was Druid only, plus census).
- The Bebelith dismantle decision (open owner question above).
- Merge, release or permanent installation.
