# Summon menu order, summon Sunder and the Lillend label: status

**Implemented; native checkpoint FAIL on candidate `c6e861a98a` (mod initialization failed closed on a product defect); fix `896c089a19` has focused-check evidence only. One rebuild and one guarded run await owner authorization** (2026-11-10)

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
`SunderArmor` maneuver roll. It was not treated as Sunder and is unchanged.

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
